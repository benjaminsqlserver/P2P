namespace P2P.Domain.Vendors;

public sealed record Address(
    string Line1,
    string? Line2,
    string City,
    string? StateOrProvince,
    string PostalCode,
    string CountryCode)
{
    public string SingleLine =>
        string.Join(", ",
            new[] { Line1, Line2, City, StateOrProvince, PostalCode, CountryCode }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
}
