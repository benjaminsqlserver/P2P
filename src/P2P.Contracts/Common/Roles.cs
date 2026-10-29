namespace P2P.Contracts.Common;

/// <summary>
/// Role names used in authorisation policies. String constants rather than
/// an enum because ASP.NET Core's role system is string-based and because
/// roles are configuration, not domain concepts.
/// </summary>
public static class Roles
{
    public const string Requester = "Requester";
    public const string DepartmentHead = "DepartmentHead";
    public const string FinanceDirector = "FinanceDirector";
    public const string Buyer = "Buyer";
    public const string ReceivingClerk = "ReceivingClerk";
    public const string AccountsPayableClerk = "AccountsPayableClerk";
    public const string Treasury = "Treasury";
    public const string VendorManager = "VendorManager";
    public const string ProcurementAdministrator = "ProcurementAdministrator";
    public const string Auditor = "Auditor";
}
