using P2P.Contracts.Requisitions;
using P2P.Contracts.Vendors;
using P2P.Domain.Requisitions;
using P2P.Domain.Vendors;

namespace P2P.Application.Common;

/// <summary>
/// Domain-to-contract enum mappings for use inside query projections.
/// Statuses are stored as strings (chapter 9), so an inline
/// <c>(Dto)(int)entity.Status</c> cast is translated to SQL as
/// <c>CAST(Status AS int)</c> and fails. A method call cannot be
/// translated, so EF reads the column, converts it back to the domain
/// enum, and applies the mapping in memory.
/// </summary>
internal static class StatusMappings
{
    public static VendorStatusDto ToDto(VendorStatus status) => (VendorStatusDto)(int)status;

    public static RequisitionStatusDto ToDto(RequisitionStatus status) =>
        (RequisitionStatusDto)(int)status;
}
