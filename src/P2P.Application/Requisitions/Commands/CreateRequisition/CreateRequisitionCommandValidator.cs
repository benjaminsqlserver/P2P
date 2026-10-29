using FluentValidation;

namespace P2P.Application.Requisitions.Commands.CreateRequisition;

internal sealed class CreateRequisitionCommandValidator
    : AbstractValidator<CreateRequisitionCommand>
{
    public CreateRequisitionCommandValidator()
    {
        RuleFor(c => c.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Request.Justification).MaximumLength(2000);
        RuleFor(c => c.Request.CostCentreId).NotEmpty();
        RuleFor(c => c.Request.Currency).NotEmpty().Length(3);

        RuleFor(c => c.Request.Lines)
            .NotEmpty()
            .WithMessage("A requisition must have at least one line.");

        RuleForEach(c => c.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Description).NotEmpty().MaximumLength(500);
            line.RuleFor(l => l.Quantity).GreaterThan(0m);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0m);
            line.RuleFor(l => l.UnitOfMeasure).NotEmpty().MaximumLength(10);
        });
    }
}
