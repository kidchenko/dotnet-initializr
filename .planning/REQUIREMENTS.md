# Requirements: NetStarter

**Defined:** 2026-02-28
**Core Value:** Users can generate a fully configured .NET project with their chosen architecture, ORM, auth, observability, and testing setup in seconds — no manual scaffolding.

## v1.2 Requirements

Requirements for v1.2 Infrastructure, UX & Polish. Each maps to roadmap phases.

### Logging

- [x] **LOG-01**: User can select NLog as an alternative to Serilog in the logging picker
- [x] **LOG-02**: Generated project includes `NLog.Web.AspNetCore` package for WebApi/MinimalApi project types
- [x] **LOG-03**: Generated project includes `NLog.Extensions.Hosting` package for Worker Service/Console project types
- [x] **LOG-04**: Generated `Program.cs` includes NLog bootstrap with `UseNLog()` call
- [x] **LOG-05**: Generated project includes NLog configuration in `appsettings.json` with `throwConfigExceptions: true`

### Resilience

- [x] **RESIL-01**: User can enable Polly/Resilience option in the generator UI (WebApi/MinimalApi only)
- [x] **RESIL-02**: Generated project includes `Microsoft.Extensions.Http.Resilience` package with SDK-aligned versioning
- [x] **RESIL-03**: Generated `Program.cs` includes named HttpClient with `AddStandardResilienceHandler()` scaffold

### API Documentation

- [x] **DOCS-01**: User can select an API documentation UI (Scalar, SwaggerUI, or Redoc) in the generator (WebApi/MinimalApi only)
- [x] **DOCS-02**: Generated project includes `Microsoft.AspNetCore.OpenApi` package with `AddOpenApi()` and `MapOpenApi()` calls
- [x] **DOCS-03**: Generated project includes Scalar.AspNetCore package and `MapScalarApiReference()` when Scalar is selected
- [x] **DOCS-04**: Generated project includes Swashbuckle.AspNetCore package with SwaggerUI middleware when SwaggerUI is selected
- [x] **DOCS-05**: Generated project includes Redoc UI when Redoc is selected
- [x] **DOCS-06**: Generated code branches correctly by SDK version (.NET 8 vs .NET 9/10) for OpenAPI setup

### Background Jobs

- [x] **JOBS-01**: User can select a background job option (None, IHostedService, Hangfire, Quartz.NET) in the generator
- [x] **JOBS-02**: Generated project includes a `SampleBackgroundService : BackgroundService` class when IHostedService is selected
- [x] **JOBS-03**: Generated project includes Hangfire packages with storage auto-matched to database choice (PostgreSQL, SQL Server, or InMemory fallback)
- [x] **JOBS-04**: Generated project includes Hangfire dashboard middleware for WebApi/MinimalApi project types
- [x] **JOBS-05**: Generated project includes Quartz packages (`Quartz`, `Quartz.Extensions.Hosting`, `Quartz.Extensions.DependencyInjection`) with a sample `IJob` implementation
- [x] **JOBS-06**: Generated file tree includes `Jobs/` or `Workers/` folder for all three architecture patterns when background jobs are selected

### URL Serialization

- [x] **URL-01**: All new v1.2 options (logging, docs, jobs, resilience) are serialized in shareable URL query parameters
- [x] **URL-02**: Existing v1.0 and v1.1 URLs continue to work without breaking (backward compatibility)
- [x] **URL-03**: Pre-existing `IncludeResilience` URL serialization gap from v1.1 is closed

### Code Preview

- [ ] **PREV-01**: User can click any file in the file tree to see its generated content in a modal
- [ ] **PREV-02**: Code preview modal displays syntax-highlighted code using highlight.js
- [ ] **PREV-03**: Code preview modal supports C#, XML, JSON, YAML, Dockerfile, and Bash syntax highlighting
- [ ] **PREV-04**: highlight.js is loaded lazily (only on modal open, not on page load) via JS module import

### Responsive Design

- [ ] **RESP-01**: Generator UI displays in a single-column stacked layout on phone-width viewports (below 768px)
- [ ] **RESP-02**: All form controls have a minimum 44px touch target for mobile usability
- [ ] **RESP-03**: Code preview modal is scrollable and properly sized on mobile viewports
- [ ] **RESP-04**: Responsive layout works correctly in production builds deployed to GitHub Pages (Tailwind purge safe)

## Future Requirements

Deferred to future releases. Tracked but not in current roadmap.

### Presets

- **PRES-01**: User can select a preset template (e.g., "Microservice", "CRUD API") for one-click project configuration

### Messaging

- **MSG-01**: User can select a messaging broker (RabbitMQ, Azure Service Bus)
- **MSG-02**: User can enable Wolverine as a messaging abstraction layer

## Out of Scope

| Feature | Reason |
|---------|--------|
| `dotnet new` custom template NuGet package | Deferred to v2+ |
| Push to GitHub repo | Deferred to v2+ |
| Monaco Editor for code preview | Over-engineered for read-only display; highlight.js is sufficient |
| Wolverine background jobs | Blocked on .NET 10 compatibility (GitHub issue #1830) |
| NLog database target | Too opinionated for a starter project |
| Preset templates | Layer on top of scaffolding — deferred to v1.3+ |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| LOG-01 | Phase 7 | Complete |
| LOG-02 | Phase 7 | Complete |
| LOG-03 | Phase 7 | Complete |
| LOG-04 | Phase 7 | Complete |
| LOG-05 | Phase 7 | Complete |
| RESIL-01 | Phase 7 | Complete |
| RESIL-02 | Phase 7 | Complete |
| RESIL-03 | Phase 7 | Complete |
| DOCS-01 | Phase 8 | Complete |
| DOCS-02 | Phase 8 | Complete |
| DOCS-03 | Phase 8 | Complete |
| DOCS-04 | Phase 8 | Complete |
| DOCS-05 | Phase 8 | Complete |
| DOCS-06 | Phase 8 | Complete |
| JOBS-01 | Phase 9 | Complete |
| JOBS-02 | Phase 9 | Complete |
| JOBS-03 | Phase 9 | Complete |
| JOBS-04 | Phase 9 | Complete |
| JOBS-05 | Phase 9 | Complete |
| JOBS-06 | Phase 9 | Complete |
| URL-01 | Phase 10 | Complete |
| URL-02 | Phase 10 | Complete |
| URL-03 | Phase 10 | Complete |
| PREV-01 | Phase 11 | Pending |
| PREV-02 | Phase 11 | Pending |
| PREV-03 | Phase 11 | Pending |
| PREV-04 | Phase 11 | Pending |
| RESP-01 | Phase 11 | Pending |
| RESP-02 | Phase 11 | Pending |
| RESP-03 | Phase 11 | Pending |
| RESP-04 | Phase 11 | Pending |

**Coverage:**
- v1.2 requirements: 31 total
- Mapped to phases: 31
- Unmapped: 0 ✓

---
*Requirements defined: 2026-02-28*
*Last updated: 2026-02-28 after roadmap creation (Phases 7-11)*
