using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Mirrors MediatR's well-known RequestHandlerDelegate name.",
    Scope = "type",
    Target = "~T:P2P.Application.Abstractions.Messaging.RequestHandlerDelegate`1")]

[assembly: SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Handles domain events, not .NET events; the name states exactly that.",
    Scope = "type",
    Target = "~T:P2P.Application.Abstractions.Messaging.IDomainEventHandler`1")]

[assembly: SuppressMessage(
    "Performance",
    "CA1805:Do not initialize unnecessarily",
    Justification = "Explicit '= default' documents that Unit.Value is the single empty value.",
    Scope = "member",
    Target = "~F:P2P.Application.Abstractions.Messaging.Unit.Value")]
