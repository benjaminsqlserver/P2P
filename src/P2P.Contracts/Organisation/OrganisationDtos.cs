namespace P2P.Contracts.Organisation;

/// <summary>Option for cost-centre pickers.</summary>
public sealed record CostCentreLookupDto(Guid Id, string Code, string Name)
{
    public string Display => $"{Code} — {Name}";
}
