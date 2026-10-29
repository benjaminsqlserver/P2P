using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Named factory methods (Success/Failure) are the intended construction API for wire results.",
    Scope = "type",
    Target = "~T:P2P.Contracts.Common.OperationResult`1")]

[assembly: SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "PagedResult<T>.Empty() is the intended construction API for an empty page.",
    Scope = "type",
    Target = "~T:P2P.Contracts.Common.PagedResult`1")]
