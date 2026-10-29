using FluentValidation;

namespace P2P.Application.Vendors.Commands.CreateVendor;

internal sealed class CreateVendorCommandValidator
    : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(c => c.Request.Code)
            .NotEmpty().WithErrorCode("Vendor.CodeRequired")
            .MaximumLength(20)
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage("Vendor code may contain only letters, digits and hyphens.");

        RuleFor(c => c.Request.LegalName)
            .NotEmpty().WithErrorCode("Vendor.LegalNameRequired")
            .MaximumLength(200);

        RuleFor(c => c.Request.TradingName).MaximumLength(200);

        RuleFor(c => c.Request.TaxIdentifier).MaximumLength(50);

        RuleFor(c => c.Request.DefaultCurrency)
            .NotEmpty().Length(3)
            .WithMessage("Currency must be a three-letter ISO 4217 code.");

        RuleFor(c => c.Request.PaymentTermsDays)
            .InclusiveBetween(0, 365);

        RuleFor(c => c.Request.RemitToAddress).NotNull();
        RuleFor(c => c.Request.RemitToAddress.Line1).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Request.RemitToAddress.City).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Request.RemitToAddress.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Request.RemitToAddress.CountryCode).NotEmpty().Length(2);
    }
}
