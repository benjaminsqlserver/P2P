using FluentAssertions;
using P2P.Application.Vendors.Commands.CreateVendor;
using P2P.Contracts.Vendors;
using P2P.Domain.Vendors;

namespace P2P.Application.Tests;

public class CreateVendorCommandHandlerTests : IDisposable
{
    private readonly TestDatabase _db = new();

    private static CreateVendorRequest ARequest(string code = "V-0001") => new()
    {
        Code = code,
        LegalName = "Acme Supplies Limited",
        TradingName = "Acme",
        TaxIdentifier = "TIN-1234567",
        DefaultCurrency = "NGN",
        PaymentTermsDays = 30,
        RemitToAddress = new AddressDto
        {
            Line1 = "12 Ligali Ayorinde Street",
            City = "Lagos",
            StateOrProvince = "Lagos",
            PostalCode = "106104",
            CountryCode = "NG"
        }
    };

    [Fact]
    public async Task Creates_a_vendor_in_draft_status()
    {
        var handler = new CreateVendorCommandHandler(_db.Context);

        var result = await handler.Handle(
            new CreateVendorCommand(ARequest()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        await _db.Context.SaveChangesAsync();

        var saved = _db.Context.Vendors.Single();
        saved.Code.Should().Be("V-0001");
        saved.Status.Should().Be(VendorStatus.Draft);
    }

    [Fact]
    public async Task Rejects_a_duplicate_vendor_code()
    {
        var handler = new CreateVendorCommandHandler(_db.Context);

        await handler.Handle(
            new CreateVendorCommand(ARequest()), CancellationToken.None);
        await _db.Context.SaveChangesAsync();

        var second = await handler.Handle(
            new CreateVendorCommand(ARequest()), CancellationToken.None);

        second.IsFailure.Should().BeTrue();
        second.Error.Should().Be(VendorErrors.CodeAlreadyExists);
    }

    [Fact]
    public async Task Normalises_the_vendor_code_to_upper_case()
    {
        var handler = new CreateVendorCommandHandler(_db.Context);

        await handler.Handle(
            new CreateVendorCommand(ARequest("v-lower")), CancellationToken.None);
        await _db.Context.SaveChangesAsync();

        _db.Context.Vendors.Single().Code.Should().Be("V-LOWER");
    }

    public void Dispose() => _db.Dispose();
}
