using System.Reflection;
using FluentAssertions;

namespace P2P.Domain.Tests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Vendors.Vendor).Assembly;

    [Fact]
    public void Domain_should_not_reference_any_infrastructure_concern()
    {
        string[] forbidden =
        [
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "System.Data.SqlClient",
            "Microsoft.Data.SqlClient",
            "Radzen",
            "FluentValidation",
            "Newtonsoft.Json"
        ];

        var referenced = DomainAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name!)
            .ToArray();

        foreach (var name in forbidden)
        {
            referenced.Should().NotContain(
                r => r.StartsWith(name, StringComparison.OrdinalIgnoreCase),
                because: $"the Domain layer must remain free of {name}");
        }
    }

    [Fact]
    public void Domain_entities_should_not_have_public_parameterless_constructors()
    {
        var entityTypes = DomainAssembly.GetTypes()
            .Where(t => t.IsClass
                        && !t.IsAbstract
                        && typeof(Common.Entity).IsAssignableFrom(t));

        foreach (var type in entityTypes)
        {
            var ctor = type.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                binder: null, types: Type.EmptyTypes, modifiers: null);

            ctor.Should().BeNull(
                because: $"{type.Name} must be created through a factory method "
                         + "so that its invariants are established on construction");
        }
    }
}
