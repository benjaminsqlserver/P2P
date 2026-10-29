# P2P — Procure-to-Pay

A procure-to-pay application built with .NET 10, Blazor (Interactive Server + WebAssembly), Radzen and SQL Server. It covers the purchasing lifecycle from requisition through to payment:

**Requisition → Approval → Purchase Order → Goods Receipt → Invoice (three-way match) → Payment**

Supporting features include vendor management, cost centres and approval policies, a dashboard, and a full audit trail.

The code follows the accompanying book, *Procure-to-Pay* by Benjamin Fadina (`Procure-to-Pay - Benjamin Fadina.docx`).

## Solution layout

Clean Architecture, with dependencies pointing inwards:

| Project | Responsibility |
| --- | --- |
| `src/P2P.Domain` | Aggregates, value objects (`Money`), domain events, `Result`/`Error`, three-way match rules |
| `src/P2P.Application` | Use cases (commands/queries), validation (FluentValidation), pipeline behaviors, abstractions |
| `src/P2P.Contracts` | DTOs and API contracts shared between server and client |
| `src/P2P.Infrastructure` | EF Core `ApplicationDbContext`, configurations, migrations, audit and domain-event interceptors, number sequences, approval routing |
| `src/P2P.Web` | ASP.NET Core host: minimal API endpoint modules, Blazor server rendering, Serilog, health checks |
| `src/P2P.Web.Client` | Blazor WebAssembly pages and components (Radzen) |
| `tests/*` | xUnit tests for Domain, Application and Web (FluentAssertions, NSubstitute, bUnit, SQLite) |

Package versions are managed centrally in `Directory.Packages.props`; shared build settings (nullable, warnings as errors, analyzers) live in `Directory.Build.props`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Developer edition or a container)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Getting started

1. **Configure the connection string.** The app reads `ConnectionStrings:P2PDatabase`. Use user secrets for local development:

   ```bash
   dotnet user-secrets --project src/P2P.Web set "ConnectionStrings:P2PDatabase" 'Server=localhost;Database=P2P;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=false'
   ```

   The single quotes keep the value literal in both PowerShell and bash. For LocalDB, use `Server=(localdb)\MSSQLLocalDB` instead of `Server=localhost`.

2. **Create the database.**

   ```bash
   dotnet ef database update --project src/P2P.Infrastructure --startup-project src/P2P.Web
   ```

3. **Run the app.**

   ```bash
   dotnet run --project src/P2P.Web --launch-profile https
   ```

   Browse to `https://localhost:7223` (or `http://localhost:5091`).

### Development sign-in

In the `Development` environment a development authentication handler signs you in as the user configured under `DevelopmentUser` in `src/P2P.Web/appsettings.Development.json`, holding all roles (Requester, Buyer, ReceivingClerk, AccountsPayableClerk, Treasury, VendorManager, ProcurementAdministrator). Edit the role list there to try the app as a narrower role. This handler is never registered outside Development; a real identity provider must be configured for other environments.

## Tests

```bash
dotnet test P2P.slnx
```

## Operations

- **Logging:** Serilog writes to the console and to the `infrastructure.ApplicationLogs` table. Every request carries a correlation id.
- **Health checks:** `/health/live` (process only) and `/health/ready` (includes the database).
- **OpenAPI:** the document is served at `/openapi/v1.json` in Development.

## Docker

```bash
docker build -t p2p .
docker run -p 8080:8080 -e ConnectionStrings__P2PDatabase="<connection string>" p2p
```

## Deployment

`.github/workflows/deploy.yml` runs on pushes to `main`: it generates an idempotent EF migration script, applies it using the `P2P_SQL_CONNECTION` secret, then publishes the web project. The final push-to-host step is left for you to fill in for your hosting target.

## License

[MIT](LICENSE)
