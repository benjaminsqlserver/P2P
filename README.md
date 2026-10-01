# P2P — Procure-to-Pay

Source code accompanying the book *Procure-to-Pay* by Benjamin Fadina: a procure-to-pay
line-of-business application built with .NET 10, Clean Architecture, CQRS and a Blazor
Web App (Auto interactivity) using Radzen components.

This branch contains the state of the solution at the end of **Chapter 5 — The Domain Layer**.

## Solution structure

```
P2P.slnx
Directory.Build.props        Shared build settings (nullable, warnings as errors, analysers)
Directory.Packages.props     Central package version management
src/
  P2P.Domain                 Entities, value objects, business rules (no dependencies)
    Common/                  Error, Result, Money, Entity, AggregateRoot, domain events
    Vendors/                 Vendor aggregate, BankAccount, Address
    Requisitions/            Requisition aggregate, lines, approval steps
    PurchaseOrders/          PurchaseOrder aggregate and lines
    GoodsReceipts/           GoodsReceipt aggregate and lines
    Invoices/                Invoice aggregate, match exceptions, ThreeWayMatchService
    Payments/                Payment aggregate and allocations
    Organisation/            CostCentre and ApprovalPolicy reference data
  P2P.Contracts              DTOs and service interfaces shared with the UI
  P2P.Application            Commands, queries, handlers, ports
  P2P.Infrastructure         EF Core, SQL Server and other adapters
  P2P.Web                    ASP.NET Core host and composition root
  P2P.Web.Client             Blazor WebAssembly client (references Contracts only)
tests/
  P2P.Domain.Tests           Domain and architecture tests
  P2P.Application.Tests
  P2P.Web.Tests
```

### Dependency rule

```
P2P.Web ──► P2P.Infrastructure ──► P2P.Application ──► P2P.Domain
   │                                     │
   └──► P2P.Web.Client ──► P2P.Contracts ◄┘
```

Source code dependencies point only inwards. `P2P.Domain` references nothing, and
`P2P.Web.Client` references only `P2P.Contracts`. `ArchitectureTests` in
`tests/P2P.Domain.Tests` enforces the Domain rules automatically.

## Prerequisites

- .NET 10 SDK (with the .NET WebAssembly build tools)
- Visual Studio 2026, or any editor with the .NET CLI
- SQL Server 2019 or later (or LocalDB) — needed from Chapter 9 onwards

## Build and test

```bash
dotnet build
dotnet test
```

Run the web host:

```bash
dotnet run --project src/P2P.Web
```

## Notes on differences from the book text

- **Radzen.Blazor** is pinned to `7.4.3`. The book lists `7.5.7`, which was never published to NuGet.
- The Blazor template nests the server and client projects; they have been moved so both sit directly under `src/`.
- `P2P.Domain.csproj` suppresses analyser rules CA1716 (`Error` matches a VB keyword) and
  CA1711 (`MatchException` ends in "Exception" but is an entity). Both names come from the book,
  and the build treats warnings as errors.
- `tests/Directory.Build.props` suppresses analyser rule CA1707 so test names may use underscores.

## License

Released under the [MIT License](LICENSE). Copyright © 2026 HepziBen Technologies Ltd.
