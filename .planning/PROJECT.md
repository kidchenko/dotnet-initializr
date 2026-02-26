# NetStarter

## What This Is

A Spring Initializr-style project generator for .NET. A Blazor WebAssembly app that runs entirely in the browser — users configure their .NET project options, hit Generate, and download a ready-to-go project as a zip file. Zero backend, deployable to static web hosting (GitHub Pages, Azure Static Web Apps) for zero running cost.

## Core Value

Users can generate a fully configured .NET project with their chosen architecture, ORM, auth, observability, and testing setup in seconds — no manual scaffolding, no forgetting boilerplate.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] Blazor WASM single-page app with dark theme UI (Spring Initializr style)
- [ ] Project metadata input (project name, namespace)
- [ ] SDK version selection (.NET 8, .NET 9, .NET 10)
- [ ] Project type selection (Web API, Minimal API, Console, Worker Service)
- [ ] Architecture pattern selection (Clean Architecture, Vertical Slice, Simple Layered)
- [ ] ORM selection (EF Core or None) with database choice (PostgreSQL, SQL Server) when EF Core selected
- [ ] Auth selection (None, JWT)
- [ ] Observability options (Serilog, Health Checks)
- [ ] Testing setup (xUnit + FluentAssertions, Testcontainers)
- [ ] Container support (Dockerfile, docker-compose, .NET Aspire)
- [ ] CI/CD scaffold (GitHub Actions)
- [ ] Client-side template engine that generates .csproj, Program.cs, folder structures, config files as strings
- [ ] Client-side zip generation and download
- [ ] Copyable dotnet CLI equivalent commands shown alongside zip download
- [ ] Conditional options (e.g., database choice only appears when EF Core is selected)

### Out of Scope

- Backend/server — all generation happens client-side in WASM
- `dotnet new` custom templates NuGet package — v2
- Push to GitHub repo — v2
- Blazor as a project type option — v2 (avoid confusion with the app itself being Blazor)
- gRPC project type — v2
- OAuth/Keycloak auth — v2
- RabbitMQ/Azure Service Bus/MassTransit messaging — v2
- NUnit test framework — v2
- Azure DevOps CI/CD — v2
- OpenTelemetry (full) — v2
- Dapper ORM — v2
- SQLite database — v2

## Context

- Existing Blazor WASM project scaffolded at `NetStarter/NetStarter/` targeting .NET 10
- Inspired by Spring Initializr (start.spring.io) — single-page dark UI with radio buttons, checkboxes, and a Generate button
- The Blazor WASM app itself runs on .NET 10, but generates projects for .NET 8/9/10
- No `dotnet` CLI available in browser — must generate all files from string templates
- Zip generation in browser is feasible with libraries like `System.IO.Compression` (available in WASM)
- Architecture patterns (Clean Architecture, Vertical Slice) are not available in `dotnet new` built-in templates — this is the key value add

## Constraints

- **Runtime**: Blazor WebAssembly only — no server-side code, no backend API
- **Hosting**: Must be deployable as static files (GitHub Pages, Azure Static Web Apps)
- **Project**: Build on existing `NetStarter/NetStarter/` Blazor WASM project (.NET 10)
- **UI**: Dark theme, single-page layout inspired by Spring Initializr
- **Templates**: All project file generation done via string templates/builders — no file system access

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Client-side only (no backend) | Zero hosting cost, static deployable, simpler architecture | — Pending |
| String template engine (not dotnet new) | dotnet CLI can't run in browser; templates as strings gives full control | — Pending |
| Show CLI equivalent commands | Users learn what the generated project maps to; bridges to v2 dotnet new templates | — Pending |
| v1 focused scope (3 project types, 3 architectures) | Manageable combination matrix; can expand later | — Pending |

---
*Last updated: 2026-02-26 after initialization*
