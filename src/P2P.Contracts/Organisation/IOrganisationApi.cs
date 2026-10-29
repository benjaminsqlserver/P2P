namespace P2P.Contracts.Organisation;

public interface IOrganisationApi
{
    /// <summary>Active cost centres, for pickers.</summary>
    Task<IReadOnlyList<CostCentreLookupDto>> GetCostCentreLookupAsync(
        CancellationToken ct = default);
}
