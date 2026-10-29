using Microsoft.AspNetCore.Authorization;
using P2P.Contracts.Common;

namespace P2P.Web.Client.Authorization;

/// <summary>
/// The application's authorisation policies, defined once and registered by
/// both hosts. The server needs them for endpoints and server rendering; the
/// WebAssembly client needs them too, because <c>AuthorizeView Policy="…"</c>
/// throws "policy was not found" once rendering moves to the browser.
/// </summary>
public static class AuthorizationPolicies
{
    public static AuthorizationOptions AddP2PPolicies(this AuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddPolicy(Policies.CanApproveRequisitions, policy =>
            policy.RequireRole(Roles.DepartmentHead, Roles.FinanceDirector));

        options.AddPolicy(Policies.CanManageVendors, policy =>
            policy.RequireRole(Roles.VendorManager, Roles.ProcurementAdministrator));

        options.AddPolicy(Policies.CanIssuePurchaseOrders, policy =>
            policy.RequireRole(Roles.Buyer, Roles.ProcurementAdministrator));

        options.AddPolicy(Policies.CanRecordReceipts, policy =>
            policy.RequireRole(Roles.ReceivingClerk, Roles.ProcurementAdministrator));

        options.AddPolicy(Policies.CanProcessInvoices, policy =>
            policy.RequireRole(Roles.AccountsPayableClerk,
                               Roles.ProcurementAdministrator));

        options.AddPolicy(Policies.CanAuthorisePayments, policy =>
            policy.RequireRole(Roles.Treasury));

        return options;
    }
}
