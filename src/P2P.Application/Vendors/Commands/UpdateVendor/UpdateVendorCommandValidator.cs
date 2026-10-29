using FluentValidation;

namespace P2P.Application.Vendors.Commands.UpdateVendor;

internal sealed class UpdateVendorCommandValidator
    : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator()
    {
        RuleFor(c => c.Request.Id).NotEmpty();

        RuleFor(c => c.Request.LegalName)
            .NotEmpty().WithErrorCode("Vendor.LegalNameRequired")
            .MaximumLength(200);

        RuleFor(c => c.Request.TradingName).MaximumLength(200);

        RuleFor(c => c.Request.TaxIdentifier).MaximumLength(50);

        RuleFor(c => c.Request.PaymentTermsDays)
            .InclusiveBetween(0, 365);

        RuleFor(c => c.Request.RemitToAddress).NotNull();
        RuleFor(c => c.Request.RemitToAddress.Line1).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Request.RemitToAddress.City).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Request.RemitToAddress.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Request.RemitToAddress.CountryCode).NotEmpty().Length(2);
    }
}
