using P2P.Domain.Common;

namespace P2P.Domain.Vendors;

/// <summary>
/// Placeholder so that the Chapter 4 architecture tests compile.
/// Chapter 5 replaces this with the full Vendor aggregate.
/// </summary>
public sealed class Vendor : Entity
{
    private Vendor(Guid id) : base(id) { }

    private Vendor() { }

    public static Vendor Create() => new(Guid.CreateVersion7());
}
