using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "A match exception is a domain concept (an invoice discrepancy), not a System.Exception.",
    Scope = "type",
    Target = "~T:P2P.Domain.Invoices.MatchException")]
