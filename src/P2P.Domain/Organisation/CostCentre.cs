using P2P.Domain.Common;

namespace P2P.Domain.Organisation;

public sealed class CostCentre : AggregateRoot
{
    private CostCentre(
        Guid id, string code, string name, Guid? departmentHeadUserId, bool isActive)
        : base(id)
    {
        Code = code;
        Name = name;
        DepartmentHeadUserId = departmentHeadUserId;
        IsActive = isActive;
    }

    private CostCentre() { }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public Guid? DepartmentHeadUserId { get; private set; }

    public bool IsActive { get; private set; }

    public static CostCentre Create(
        string code, string name, Guid? departmentHeadUserId) =>
        new(Guid.CreateVersion7(), code.Trim().ToUpperInvariant(),
            name.Trim(), departmentHeadUserId, isActive: true);

    public void Rename(string name) => Name = name.Trim();

    public void AssignHead(Guid? userId) => DepartmentHeadUserId = userId;

    public void Deactivate() => IsActive = false;
}
