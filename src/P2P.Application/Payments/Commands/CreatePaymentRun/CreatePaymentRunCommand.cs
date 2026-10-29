using FluentValidation;
using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Payments;
using P2P.Domain.Common;
using P2P.Domain.Invoices;
using P2P.Domain.Payments;
using P2P.Domain.Vendors;

namespace P2P.Application.Payments.Commands.CreatePaymentRun;

public sealed record CreatePaymentRunCommand(CreatePaymentRunRequest Request)
    : ICommand<IReadOnlyList<Guid>>;

internal sealed class CreatePaymentRunCommandValidator
    : AbstractValidator<CreatePaymentRunCommand>
{
    public CreatePaymentRunCommandValidator()
    {
        RuleFor(c => c.Request.InvoiceIds)
            .NotEmpty()
            .WithMessage("Select at least one invoice.");

        RuleFor(c => c.Request.InvoiceIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("An invoice may appear only once in a payment run.");

        RuleFor(c => c.Request.Method).IsInEnum();
    }
}

internal static class PaymentRunErrors
{
    public static readonly Error InvoiceNotPayable =
        Error.Conflict("PaymentRun.InvoiceNotPayable",
            "One or more selected invoices are not approved for payment, "
          + "or are already on another payment.");

    public static readonly Error NoPrimaryBankAccount =
        Error.Conflict("PaymentRun.NoPrimaryBankAccount",
            "A vendor in this run has no primary bank account in the payment currency.");
}

/// <summary>
/// Creates one draft payment per vendor and currency, allocating each
/// selected invoice's full outstanding amount. Authorisation and execution
/// are separate steps.
/// </summary>
internal sealed class CreatePaymentRunCommandHandler(
    IApplicationDbContext context,
    INumberSequence numbers)
    : ICommandHandler<CreatePaymentRunCommand, IReadOnlyList<Guid>>
{
    public async Task<Result<IReadOnlyList<Guid>>> Handle(
        CreatePaymentRunCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var requestedIds = request.InvoiceIds.ToArray();

        var alreadyAllocated = PaymentQueries.InvoicesOnLivePayments(context);

        var invoices = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Lines)
            .Where(i => requestedIds.Contains(i.Id)
                        && (i.Status == InvoiceStatus.Approved
                            || i.Status == InvoiceStatus.PartiallyPaid)
                        && !alreadyAllocated.Contains(i.Id))
            .ToListAsync(ct);

        if (invoices.Count != requestedIds.Length)
        {
            return Result.Failure<IReadOnlyList<Guid>>(PaymentRunErrors.InvoiceNotPayable);
        }

        var vendorIds = invoices.Select(i => i.VendorId).Distinct().ToArray();

        var vendors = await context.Vendors
            .AsNoTracking()
            .Include(v => v.BankAccounts)
            .Where(v => vendorIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, ct);

        var paymentMethod = (PaymentMethod)(int)request.Method;
        var created = new List<Guid>();

        foreach (var group in invoices.GroupBy(i => (i.VendorId, i.Currency)))
        {
            if (!vendors.TryGetValue(group.Key.VendorId, out var vendor))
            {
                return Result.Failure<IReadOnlyList<Guid>>(
                    Error.NotFound(nameof(Vendor), group.Key.VendorId));
            }

            // A vendor on hold or blocked must not be paid.
            if (vendor.Status != VendorStatus.Active)
            {
                return Result.Failure<IReadOnlyList<Guid>>(VendorErrors.NotActive);
            }

            var account = vendor.BankAccounts.FirstOrDefault(
                b => b.IsPrimary && b.Currency == group.Key.Currency);

            if (account is null)
            {
                return Result.Failure<IReadOnlyList<Guid>>(
                    PaymentRunErrors.NoPrimaryBankAccount);
            }

            var number = await numbers.NextAsync("PAY", ct);

            var paymentResult = Payment.Create(
                number, vendor.Id, account.Id, request.PaymentDate,
                paymentMethod, group.Key.Currency);

            if (paymentResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<Guid>>(paymentResult.Error);
            }

            var payment = paymentResult.Value;

            foreach (var invoice in group)
            {
                var allocated = payment.Allocate(
                    invoice.Id, invoice.AmountOutstanding.Amount);

                if (allocated.IsFailure)
                {
                    return Result.Failure<IReadOnlyList<Guid>>(allocated.Error);
                }
            }

            context.Payments.Add(payment);
            created.Add(payment.Id);
        }

        return created;
    }
}
