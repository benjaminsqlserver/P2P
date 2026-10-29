using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using P2P.Application;
using P2P.Application.Abstractions;
using P2P.Contracts.GoodsReceipts;
using P2P.Contracts.Invoices;
using P2P.Contracts.Audit;
using P2P.Contracts.Dashboard;
using P2P.Contracts.Organisation;
using P2P.Contracts.Payments;
using P2P.Contracts.PurchaseOrders;
using P2P.Contracts.Requisitions;
using P2P.Contracts.Vendors;
using P2P.Infrastructure;
using P2P.Infrastructure.Persistence;
using P2P.Web.Client.Authorization;
using P2P.Web.Components;
using P2P.Web.Endpoints;
using P2P.Web.Middleware;
using P2P.Web.Services;
using Radzen;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// --- Layers -------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- The server-side implementations of the UI contracts ---------------
builder.Services.AddScoped<IVendorApi, ServerVendorApi>();
builder.Services.AddScoped<IRequisitionApi, ServerRequisitionApi>();
builder.Services.AddScoped<IPurchaseOrderApi, ServerPurchaseOrderApi>();
builder.Services.AddScoped<IGoodsReceiptApi, ServerGoodsReceiptApi>();
builder.Services.AddScoped<IInvoiceApi, ServerInvoiceApi>();
builder.Services.AddScoped<IPaymentApi, ServerPaymentApi>();
builder.Services.AddScoped<IOrganisationApi, ServerOrganisationApi>();
builder.Services.AddScoped<IDashboardApi, ServerDashboardApi>();
builder.Services.AddScoped<IAuditApi, ServerAuditApi>();

// --- Identity, current user --------------------------------------------
// Development-only sign-in until a real identity provider is registered
// (chapter 19). Never enable this outside Development.
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<DevelopmentUserOptions>(
        builder.Configuration.GetSection(DevelopmentUserOptions.SectionName));

    builder.Services
        .AddAuthentication(DevelopmentAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
            DevelopmentAuthenticationHandler.SchemeName, configureOptions: null);
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddCascadingAuthenticationState();
// Same policy set as the WebAssembly client; see AuthorizationPolicies.
builder.Services.AddAuthorization(options => options.AddP2PPolicies());

// --- Blazor + Radzen ----------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddRadzenComponents();

builder.Services.AddEndpointModules();
builder.Services.AddOpenApi();

// --- Health checks (chapter 19) ----------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
// Before request logging, so every log line for the request carries the id.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapEndpointModules();

// Liveness must not touch the database; readiness checks dependencies.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(P2P.Web.Client._Imports).Assembly);

app.Run();
