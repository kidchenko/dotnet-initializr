---
phase: 16-data-access-and-auth
plan: "02"
subsystem: database
tags: [efcore, dapper, jwt, keycloak, aspnet-identity, apikey, swagger, dbcontext, repository-pattern]

# Dependency graph
requires:
  - phase: 16-01
    provides: conditional PackageReferences, computed symbols (IncludeEfCore, IncludeDapper, IncludeAnyOrm, IncludeJwt, IncludeKeycloak, IncludeAspNetIdentity, IncludeApiKey, IncludeSwaggerUI, IncludeNet8, IncludeAnyAuth), sources.modifiers for Data/ and Models/ folders

provides:
  - AppDbContext with conditional IdentityDbContext/DbContext inheritance and SampleEntity DbSet (Clean Architecture and single-project)
  - ISampleRepository interface in Domain/Repositories (Clean Architecture)
  - SampleRepository EF Core and Dapper implementations (Clean Architecture)
  - InfrastructureExtensions.cs with AddDbContext and IDbConnection registration per ORM+DB combo (Clean Architecture)
  - ApiKeyAuthenticationHandler class for both Clean Architecture and single-project
  - Program.cs auth registration blocks (JWT, Keycloak, ASP.NET Identity, API Key) for all web project types
  - Program.cs SwaggerUI branching (AddSwaggerGen on net8, SwaggerEndpoint on net9/10)
  - appsettings.json conditional ConnectionStrings and Authentication sections for all DB/auth combos
  - Inline ORM registration in single-project Program.cs for all project types (WebApi, MinimalApi, Console, Worker)

affects: [16-03, 17-logging-testing-quality, 18-devops-containers-background-jobs, test-template.sh verification]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Fully qualified type names in #if blocks to avoid CS8019 unused-using warnings with TreatWarningsAsErrors"
    - "Repository interface in Domain project (not Application) per Clean Architecture user decision"
    - "appsettings.json conditional sections with leading comma on first content line (template engine strips #if lines)"
    - "AppDbContext conditional inheritance: IdentityDbContext<IdentityUser> when IncludeAspNetIdentity, DbContext otherwise"
    - "Single-project Data/ references Models.SampleEntity via namespace qualifier (not using directive)"
    - "ORM registration added to Console/Worker branches for background job data access scenarios"

key-files:
  created:
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/Data/AppDbContext.cs
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Domain/Repositories/ISampleRepository.cs
    - templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/Auth/ApiKeyAuthenticationHandler.cs
    - templates/dotnet-initializr/src/Company.ProjectName/Auth/ApiKeyAuthenticationHandler.cs
  modified:
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/Data/SampleRepository.cs
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/InfrastructureExtensions.cs
    - templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/Program.cs
    - templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/appsettings.json
    - templates/dotnet-initializr/src/Company.ProjectName/Data/Placeholder.cs
    - templates/dotnet-initializr/src/Company.ProjectName/Models/Placeholder.cs
    - templates/dotnet-initializr/src/Company.ProjectName/Program.cs
    - templates/dotnet-initializr/src/Company.ProjectName/appsettings.json

key-decisions:
  - "ISampleRepository lives in Domain/Repositories (not Application) — maintains pure Clean Architecture separation"
  - "Fully qualified type names used for cross-namespace types in #if blocks (e.g., Microsoft.AspNetCore.Identity.IdentityUser) to prevent CS8019 errors with TreatWarningsAsErrors enabled"
  - "appsettings.json leading comma pattern: comma placed on first content line of conditional block because template engine strips #if lines, making the preceding AllowedHosts line the last unconditional entry"
  - "ORM registration added to Console/Worker branches in single-project — background workers legitimately need data access"
  - "SampleRepository #else branch preserves ISampleService implementation as fallback when no ORM selected"

requirements-completed: [PARAM-03, PARAM-04, PARAM-05, GEN-06]

# Metrics
duration: 3min
completed: 2026-03-08
---

# Phase 16 Plan 02: ORM, Auth, and API Docs C# Source Content Summary

**EF Core/Dapper DbContext and repositories, 4-strategy auth middleware (JWT/Keycloak/Identity/ApiKey), and SwaggerUI net8 vs net9/10 branching added to both Clean Architecture and single-project template trees**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-08T17:52:32Z
- **Completed:** 2026-03-08T17:55:35Z
- **Tasks:** 2
- **Files modified:** 12 (4 created, 8 modified)

## Accomplishments

- AppDbContext with conditional IdentityDbContext inheritance and SampleEntity DbSet generated for both Clean Architecture and single-project
- Repository pattern wired: ISampleRepository in Domain, SampleRepository with EF Core and Dapper implementations in Infrastructure
- All 4 auth strategies (JWT, Keycloak, ASP.NET Identity, API Key) produce correct middleware registration in Program.cs for web project types
- ApiKeyAuthenticationHandler class generated in both architecture trees with config-based key validation
- SwaggerUI branching: AddSwaggerGen on net8, SwaggerEndpoint pointing at /openapi/v1.json on net9/10
- appsettings.json has conditional ConnectionStrings (per DB provider) and Authentication sections (per auth strategy)
- ORM registration included in Console and Worker project type branches for background job data access

## Task Commits

Each task was committed atomically:

1. **Task 1: Add ORM, auth, and API docs content to Clean Architecture source tree** - `a8e7c10` (feat)
2. **Task 2: Add ORM, auth, and API docs content to single-project source tree** - `c5465e4` (feat)

**Plan metadata:** (docs commit — see below)

## Files Created/Modified

### Created
- `templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/Data/AppDbContext.cs` - EF Core DbContext with conditional IdentityDbContext/DbContext inheritance and SampleEntity DbSet
- `templates/dotnet-initializr/src-clean/Company.ProjectName.Domain/Repositories/ISampleRepository.cs` - Repository interface in Domain project
- `templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/Auth/ApiKeyAuthenticationHandler.cs` - Config-based API key handler for Clean Architecture
- `templates/dotnet-initializr/src/Company.ProjectName/Auth/ApiKeyAuthenticationHandler.cs` - Config-based API key handler for single-project architectures

### Modified
- `templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/Data/SampleRepository.cs` - EF Core and Dapper implementations behind #if blocks; #else preserves ISampleService stub
- `templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/InfrastructureExtensions.cs` - AddDbContext and IDbConnection registration per ORM+DB combination
- `templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/Program.cs` - Auth registration blocks and SwaggerUI branching for web project types
- `templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/appsettings.json` - Conditional ConnectionStrings and Authentication sections
- `templates/dotnet-initializr/src/Company.ProjectName/Data/Placeholder.cs` - Conditional AppDbContext (EfCore) / SampleRepository (Dapper) / placeholder (None)
- `templates/dotnet-initializr/src/Company.ProjectName/Models/Placeholder.cs` - Conditional SampleEntity when ORM is selected
- `templates/dotnet-initializr/src/Company.ProjectName/Program.cs` - Inline ORM registration, auth blocks, SwaggerUI branching for all project types
- `templates/dotnet-initializr/src/Company.ProjectName/appsettings.json` - Conditional ConnectionStrings and Authentication sections

## Decisions Made

- ISampleRepository lives in Domain/Repositories (not Application) — per user decision recorded in Phase 16 CONTEXT.md for pure Clean Architecture separation
- Fully qualified type names used for cross-namespace types in #if blocks (e.g., `Microsoft.AspNetCore.Identity.IdentityUser`) to prevent CS8019 unused-using warnings with `TreatWarningsAsErrors` enabled
- appsettings.json leading comma pattern: comma placed on first content line of conditional block because the template engine strips `#if` lines, making `"AllowedHosts": "*"` the last unconditional entry without a trailing comma
- ORM registration added to Console/Worker branches in single-project — background workers legitimately need data access
- SampleRepository `#else` branch preserved with ISampleService implementation as fallback when no ORM is selected

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Both Clean Architecture and single-project template trees now have complete ORM, auth, and API docs source content
- Phase 16 Plan 03 (verification and integration tests) can validate the full end-to-end template generation across all ORM/DB/auth/apiDocs combinations
- All 13 template files from Plan 01 (package references) now have matching source code content

---
*Phase: 16-data-access-and-auth*
*Completed: 2026-03-08*
