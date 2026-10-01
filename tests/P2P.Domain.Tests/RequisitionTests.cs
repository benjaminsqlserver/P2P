using FluentAssertions;
using P2P.Domain.Requisitions;

namespace P2P.Domain.Tests;

public class RequisitionTests
{
    private static readonly DateOnly Today = new(2026, 8, 24);

    private static readonly DateTimeOffset Now =
        new(2026, 8, 24, 9, 0, 0, TimeSpan.Zero);

    private static Requisition ADraftRequisition(Guid? requesterId = null)
    {
        var result = Requisition.Create(
            number: "REQ-2026-000001",
            requesterId: requesterId ?? Guid.CreateVersion7(),
            costCentreId: Guid.CreateVersion7(),
            title: "Laptops for new intake",
            justification: "Twelve new starters in September.",
            neededBy: Today.AddDays(30),
            currency: "NGN",
            today: Today);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }

    private static IReadOnlyList<ApprovalRouteStep> ASingleStepRoute(Guid approverId) =>
        [new ApprovalRouteStep(1, "DepartmentHead", approverId)];

    [Fact]
    public void Cannot_submit_a_requisition_with_no_lines()
    {
        var requisition = ADraftRequisition();

        var result = requisition.Submit(
            ASingleStepRoute(Guid.CreateVersion7()), Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RequisitionErrors.AtLeastOneLineRequired);
        requisition.Status.Should().Be(RequisitionStatus.Draft);
    }

    [Fact]
    public void Total_is_the_sum_of_line_totals()
    {
        var requisition = ADraftRequisition();

        requisition.AddLine(null, "Laptop", 12m, "EA", 850_000m, null);
        requisition.AddLine(null, "Docking station", 12m, "EA", 95_000m, null);

        requisition.Total.Amount.Should().Be(11_340_000m);
        requisition.Total.Currency.Should().Be("NGN");
    }

    [Fact]
    public void A_requester_cannot_approve_their_own_requisition()
    {
        var requesterId = Guid.CreateVersion7();
        var requisition = ADraftRequisition(requesterId);
        requisition.AddLine(null, "Laptop", 1m, "EA", 850_000m, null);
        requisition.Submit(ASingleStepRoute(requesterId), Now);

        var result = requisition.Approve(requesterId, "Looks fine to me", Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RequisitionErrors.SelfApprovalForbidden);
    }

    [Fact]
    public void Approval_completes_only_after_every_step_has_acted()
    {
        var requisition = ADraftRequisition();
        requisition.AddLine(null, "Server rack", 2m, "EA", 4_000_000m, null);
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();
        requisition.Submit(
        [
            new ApprovalRouteStep(1, "DepartmentHead", first),
            new ApprovalRouteStep(2, "FinanceDirector", second)
        ], Now);

        requisition.Approve(first, null, Now).IsSuccess.Should().BeTrue();
        requisition.Status.Should().Be(RequisitionStatus.PendingApproval);

        requisition.Approve(second, null, Now).IsSuccess.Should().BeTrue();
        requisition.Status.Should().Be(RequisitionStatus.Approved);
    }

    [Fact]
    public void Rejection_at_step_one_skips_remaining_steps()
    {
        var requisition = ADraftRequisition();
        requisition.AddLine(null, "Server rack", 2m, "EA", 4_000_000m, null);
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();
        requisition.Submit(
        [
            new ApprovalRouteStep(1, "DepartmentHead", first),
            new ApprovalRouteStep(2, "FinanceDirector", second)
        ], Now);

        requisition.Reject(first, "Budget already committed elsewhere.", Now);

        requisition.Status.Should().Be(RequisitionStatus.Rejected);
        requisition.ApprovalSteps
            .Single(s => s.Sequence == 2).Status
            .Should().Be(ApprovalStepStatus.Skipped);
    }
}
