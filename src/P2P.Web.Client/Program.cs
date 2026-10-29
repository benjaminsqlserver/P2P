using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using P2P.Contracts.GoodsReceipts;
using P2P.Contracts.Invoices;
using P2P.Contracts.Audit;
using P2P.Contracts.Dashboard;
using P2P.Contracts.Organisation;
using P2P.Contracts.Payments;
using P2P.Contracts.PurchaseOrders;
using P2P.Contracts.Requisitions;
using P2P.Contracts.Vendors;
using P2P.Web.Client.Authorization;
using P2P.Web.Client.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// The same policies the server registers; AuthorizeView Policy="…" needs
// them once rendering moves to WebAssembly.
builder.Services.AddAuthorizationCore(options => options.AddP2PPolicies());
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddRadzenComponents();

// One HttpClient pointed at our own origin.
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// --- The WebAssembly implementations of the same contracts -------------
builder.Services.AddScoped<IVendorApi, HttpVendorApi>();
builder.Services.AddScoped<IRequisitionApi, HttpRequisitionApi>();
builder.Services.AddScoped<IPurchaseOrderApi, HttpPurchaseOrderApi>();
builder.Services.AddScoped<IGoodsReceiptApi, HttpGoodsReceiptApi>();
builder.Services.AddScoped<IInvoiceApi, HttpInvoiceApi>();
builder.Services.AddScoped<IPaymentApi, HttpPaymentApi>();
builder.Services.AddScoped<IOrganisationApi, HttpOrganisationApi>();
builder.Services.AddScoped<IDashboardApi, HttpDashboardApi>();
builder.Services.AddScoped<IAuditApi, HttpAuditApi>();

await builder.Build().RunAsync();
