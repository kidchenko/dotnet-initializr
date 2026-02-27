# Requirements: NetStarter

**Defined:** 2026-02-26
**Core Value:** Users can generate a fully configured .NET project with their chosen architecture, ORM, auth, observability, and testing setup in seconds — no manual scaffolding.

## v1 Requirements

Requirements for initial release. Each maps to roadmap phases.

### UI

- [x] **UI-01**: User sees a dark-themed single-page form inspired by Spring Initializr (Blazorise + Tailwind CSS)
- [x] **UI-02**: User can enter project name and root namespace
- [x] **UI-03**: User sees conditional options (e.g., database picker only appears when EF Core is selected)
- [x] **UI-04**: User can preview the file tree of what will be generated before downloading
- [x] **UI-05**: User can share their configuration via a URL that encodes all selected options
- [x] **UI-06**: User sees a loading indicator while WASM initializes on first visit

### Project Configuration

- [x] **CFG-01**: User can select .NET SDK version (.NET 8, .NET 9, .NET 10)
- [x] **CFG-02**: User can select project type (Web API, Minimal API, Console, Worker Service)
- [x] **CFG-03**: User can select architecture pattern (Clean Architecture, Vertical Slice, Simple Layered)
- [x] **CFG-04**: User can select ORM (EF Core or None)
- [x] **CFG-05**: User can select database (PostgreSQL, SQL Server) when EF Core is selected
- [x] **CFG-06**: User can select auth (None, JWT)
- [x] **CFG-07**: User can select observability options (Serilog, Health Checks, OpenTelemetry) as multi-select
- [x] **CFG-08**: User can select testing setup (xUnit + FluentAssertions, Testcontainers) as multi-select
- [x] **CFG-09**: User can select container support (Dockerfile, docker-compose, .NET Aspire) as multi-select
- [x] **CFG-10**: User can select CI/CD scaffold (GitHub Actions, Azure DevOps) as multi-select
- [x] **CFG-11**: User can select mapping library (None, Mapster)

### Generation Engine

- [x] **GEN-01**: System generates a complete .NET solution using .slnx format with correct project references
- [x] **GEN-02**: System generates .csproj files with correct SDK, target framework, and NuGet package references for the selected SDK version
- [x] **GEN-03**: System generates a compilable Program.cs that wires up all selected options (ORM, auth, Serilog, health checks)
- [x] **GEN-04**: System generates architecture-specific folder structure and project organization (Clean Architecture: Domain/Application/Infrastructure/Presentation; Vertical Slice: Features/; Simple Layered: Controllers/Services/Data)
- [x] **GEN-05**: System generates EF Core DbContext, entity configuration, and connection string in appsettings.json when EF Core is selected
- [x] **GEN-06**: System generates JWT bearer auth configuration in Program.cs and middleware pipeline when JWT is selected
- [x] **GEN-07**: System generates Serilog configuration with console + file sinks when Serilog is selected
- [x] **GEN-08**: System generates health check endpoint (/health) when Health Checks is selected
- [x] **GEN-09**: System generates xUnit test project with FluentAssertions when testing is selected
- [x] **GEN-10**: System generates Testcontainers integration test setup when Testcontainers is selected and a database is configured
- [x] **GEN-11**: System generates multi-stage Dockerfile optimized for .NET when Dockerfile is selected
- [x] **GEN-12**: System generates docker-compose.yml with app + database services when docker-compose is selected
- [x] **GEN-13**: System generates .NET Aspire AppHost + ServiceDefaults projects when Aspire is selected
- [x] **GEN-14**: System generates .github/workflows/dotnet.yml with build + test steps when GitHub Actions is selected
- [x] **GEN-20**: System generates azure-pipelines.yml with build + test steps when Azure DevOps is selected
- [x] **GEN-21**: System generates Mapster configuration and mapping profiles when Mapster is selected
- [x] **GEN-22**: System generates OpenTelemetry SDK setup with OTLP exporter and ASP.NET Core/HttpClient/EF Core instrumentation when OpenTelemetry is selected
- [x] **GEN-15**: System generates .gitignore with standard .NET patterns
- [x] **GEN-16**: System generates .editorconfig with standard .NET code style settings
- [x] **GEN-17**: System generates README.md with setup instructions for the generated project
- [x] **GEN-18**: System generates appsettings.json and appsettings.Development.json with correct configuration
- [x] **GEN-19**: Generated code compiles successfully with `dotnet build` for all supported option combinations

### Output

- [x] **OUT-01**: User can download the generated project as a zip file (client-side generation, no backend)
- [x] **OUT-02**: User can see and copy the equivalent dotnet CLI commands that approximate the generated project
- [x] **OUT-03**: Zip file contains all files under a root folder named after the project

### Deployment

- [x] **DEP-01**: App is deployable as static files to any static host (GitHub Pages, Cloudflare Workers/Pages, Azure Static Web Apps) with correct SPA routing
- [x] **DEP-02**: App includes CI/CD workflow for automated deployment

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Generation

- **GEN-V2-01**: User can push generated project directly to a new GitHub repository
- **GEN-V2-02**: System provides installable `dotnet new` custom templates via NuGet package
- **GEN-V2-03**: User can select Blazor as a project type
- **GEN-V2-04**: User can select gRPC as a project type
- **GEN-V2-05**: User can select OAuth/Keycloak auth providers
- **GEN-V2-06**: User can select messaging (RabbitMQ, Azure Service Bus, MassTransit)
- **GEN-V2-07**: ~~User can select full OpenTelemetry (exporters, collectors)~~ — Basic OTel promoted to v1 (GEN-22). Full collector/dashboard config remains v2
- **GEN-V2-08**: User can select Dapper as ORM
- **GEN-V2-09**: User can select SQLite as database
- **GEN-V2-10**: User can select NUnit as test framework
- **GEN-V2-11**: ~~User can select Azure DevOps CI/CD~~ — Promoted to v1 (CFG-10, GEN-20)

## Out of Scope

| Feature | Reason |
|---------|--------|
| Server-side backend / API | Destroys zero-cost static hosting; all generation is client-side |
| User accounts / login | No backend; privacy is a feature |
| Live code editor in browser | High complexity, not needed for a scaffolding tool |
| Mobile-responsive design | Developer tool used on desktops |
| Monorepo / workspace generation | Different mental model; scope explosion |
| Interactive CLI companion | Separate project; requires distribution and install |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| UI-01 | Phase 1 | Complete |
| UI-02 | Phase 1 | Complete |
| UI-03 | Phase 1 | Complete |
| UI-04 | Phase 1 | Complete |
| UI-05 | Phase 1 | Complete |
| UI-06 | Phase 1 | Complete |
| CFG-01 | Phase 1 | Complete |
| CFG-02 | Phase 1 | Complete |
| CFG-03 | Phase 1 | Complete |
| CFG-04 | Phase 1 | Complete |
| CFG-05 | Phase 1 | Complete |
| CFG-06 | Phase 1 | Complete |
| CFG-07 | Phase 1 | Complete |
| CFG-08 | Phase 1 | Complete |
| CFG-09 | Phase 1 | Complete |
| CFG-10 | Phase 1 | Complete |
| CFG-11 | Phase 1 | Complete |
| GEN-01 | Phase 2 | Complete |
| GEN-02 | Phase 2 | Complete |
| GEN-03 | Phase 2 | Complete |
| GEN-04 | Phase 2 | Complete |
| GEN-05 | Phase 2 | Complete |
| GEN-06 | Phase 2 | Complete |
| GEN-07 | Phase 2 | Complete |
| GEN-08 | Phase 2 | Complete |
| GEN-09 | Phase 2 | Complete |
| GEN-10 | Phase 2 | Complete |
| GEN-11 | Phase 2 | Complete |
| GEN-12 | Phase 2 | Complete |
| GEN-13 | Phase 2 | Complete |
| GEN-14 | Phase 2 | Complete |
| GEN-15 | Phase 2 | Complete |
| GEN-16 | Phase 2 | Complete |
| GEN-17 | Phase 2 | Complete |
| GEN-18 | Phase 2 | Complete |
| GEN-19 | Phase 2 | Complete |
| GEN-20 | Phase 2 | Complete |
| GEN-21 | Phase 2 | Complete |
| GEN-22 | Phase 2 | Complete |
| OUT-01 | Phase 2 | Complete |
| OUT-02 | Phase 2 | Complete |
| OUT-03 | Phase 2 | Complete |
| DEP-01 | Phase 3 | Complete |
| DEP-02 | Phase 3 | Complete |

**Coverage:**
- v1 requirements: 41 total
- Mapped to phases: 41
- Unmapped: 0

---
*Requirements defined: 2026-02-26*
*Last updated: 2026-02-26 after roadmap creation (3-phase structure: UI, Generation, Deployment)*
