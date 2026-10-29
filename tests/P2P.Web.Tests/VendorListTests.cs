using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using P2P.Contracts.Common;
using P2P.Contracts.Vendors;
using P2P.Web.Client.Pages.Vendors;
using Radzen;

namespace P2P.Web.Tests;

public class VendorListTests : TestContext
{
    private readonly IVendorApi _api = Substitute.For<IVendorApi>();

    public VendorListTests()
    {
        Services.AddSingleton(_api);
        Services.AddRadzenComponents();
        Services.AddAuthorizationCore();

        JSInterop.Mode = JSRuntimeMode.Loose;

        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("test.user");
        auth.SetRoles("VendorManager");
    }

    [Fact]
    public void Renders_the_vendors_returned_by_the_api()
    {
        _api.GetListAsync(Arg.Any<VendorGridQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<VendorListItemDto>
            {
                TotalCount = 1,
                Items =
                [
                    new VendorListItemDto
                    {
                        Id = Guid.CreateVersion7(),
                        Code = "V-0001",
                        LegalName = "Acme Supplies Limited",
                        TradingName = "Acme",
                        Status = VendorStatusDto.Active,
                        DefaultCurrency = "NGN",
                        PaymentTermsDays = 30,
                        City = "Lagos",
                        CountryCode = "NG",
                        OpenPurchaseOrderCount = 3,
                        OutstandingBalance = 1_250_000m
                    }
                ]
            });

        var component = RenderComponent<VendorList>();

        component.Markup.Should().Contain("V-0001");
        component.Markup.Should().Contain("Acme Supplies Limited");
        component.Markup.Should().Contain("₦1,250,000.00");
    }

    [Fact]
    public void Passes_the_search_term_through_to_the_api()
    {
        _api.GetListAsync(Arg.Any<VendorGridQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<VendorListItemDto>.Empty());

        var component = RenderComponent<VendorList>();

        // RadzenFormField replaces the placeholder with " " for its floating
        // label, so find the search box by class; it is the only text box.
        component.Find("input.rz-textbox").Change("acme");

        _api.Received().GetListAsync(
            Arg.Is<VendorGridQuery>(q => q.Search == "acme"),
            Arg.Any<CancellationToken>());
    }
}
