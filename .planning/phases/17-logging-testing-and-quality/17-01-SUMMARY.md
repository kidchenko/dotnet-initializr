---
phase: 17-logging-testing-and-quality
plan: 01
subsystem: infra
tags: [serilog, nlog, fluentvalidation, mapster, redis, opentelemetry, testing, xunit, testcontainers, template-engine]

# Dependency graph
requires:
  - phase: 16-data-access-and-auth
    provides: "ORM/auth conditional PackageReference patterns and template.json computed symbols"
provides:
  - "Conditional PackageReference blocks for Serilog, NLog (web/non-web split), FluentValidation 12.*, Mapster 7.*, StackExchangeRedis, Http.Resilience, OpenTelemetry in all 4 .csproj files"
  - "IncludeAnyTesting computed symbol in template.json"
  - "testing parameter defaultValue changed to none with none choice added"
  - "sources entries for tests-single/ and tests-split/ directories"
  - "sources.modifiers excluding SampleEntityValidator.cs and SampleMappingConfig.cs when features not selected"
  - "Both .slnx files with conditional test project entries"
affects: [17-02, 17-03, 18-logging-testing-and-quality]

# Tech tracking
tech-stack:
  added:
    - Serilog.AspNetCore 10.*
    - Serilog.Sinks.Console 6.*
    - Serilog.Sinks.File 6.*
    - NLog.Web.AspNetCore 6.* (web projects)
    - NLog.Extensions.Hosting 6.* (non-web projects)
    - FluentValidation 12.*
    - FluentValidation.DependencyInjectionExtensions 12.*
    - Mapster 7.*
    - Mapster.DependencyInjection 1.*
    - Microsoft.Extensions.Caching.StackExchangeRedis 10.*
    - Microsoft.Extensions.Http.Resilience 10.*
    - OpenTelemetry.Extensions.Hosting 1.*
    - OpenTelemetry.Instrumentation.AspNetCore 1.* (web only)
    - OpenTelemetry.Instrumentation.Http 1.*
    - OpenTelemetry.Exporter.Console 1.*
  patterns:
    - "NLog web/non-web split: IncludeNLog && IncludeWebProject -> NLog.Web.AspNetCore; IncludeNLog && !IncludeWebProject -> NLog.Extensions.Hosting"
    - "Layer separation: Infrastructure (caching/resilience/OTel), Application (validation/mapping), EntryPoint (logging only)"
    - "IncludeAnyTesting computed symbol excludes none value to gate test project source generation"
    - "Test project variant split: tests-single/ (without testcontainers), tests-split/ (with testcontainers)"

key-files:
  created: []
  modified:
    - templates/dotnet-initializr/src/Company.ProjectName/Company.ProjectName.csproj
    - templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/Company.ProjectName.__EntryPoint__.csproj
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/Company.ProjectName.Infrastructure.csproj
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Application/Company.ProjectName.Application.csproj
    - templates/dotnet-initializr/.template.config/template.json
    - templates/dotnet-initializr/Company.ProjectName.slnx
    - templates/dotnet-initializr/Company.ProjectName.clean.slnx

key-decisions:
  - "testing parameter defaultValue changed from xunit to none — test project only generated when user explicitly passes a --testing flag"
  - "NLog web/non-web split is the critical constraint: NLog.Web.AspNetCore only for web projects, NLog.Extensions.Hosting for Worker/Console"
  - "IncludeAnyTesting value omits none from the condition — testing==none evaluates to IncludeAnyTesting=false"
  - "FluentValidation pinned at 12.* (not FluentAssertions 7.* which is for test projects in Plan 03)"
  - "mapping description corrected from Mapperly to Mapster"
  - "OpenTelemetry.Instrumentation.AspNetCore gated on IncludeWebProject to avoid including ASP.NET Core middleware in non-web projects"

patterns-established:
  - "Quality packages split by layer: Infrastructure (caching/resilience/OTel), Application (validation/mapping)"
  - "Test sources split: tests-single maps to tests/ when no testcontainers, tests-split maps to tests/ when testcontainers selected"
  - "sources.modifiers excludes sample content files (SampleEntityValidator.cs, SampleMappingConfig.cs) when feature flags are off"

requirements-completed: [PARAM-06, PARAM-08, PARAM-13]

# Metrics
duration: 3min
completed: 2026-03-08
---

# Phase 17 Plan 01: NuGet Package References for Logging, Quality, and Testing Summary

**Conditional PackageReference blocks for Serilog/NLog/validation/caching/mapping/resilience/OpenTelemetry added to 4 .csproj files with IncludeAnyTesting symbol and test project sources configured in template.json**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-08T23:18:05Z
- **Completed:** 2026-03-08T23:21:00Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments

- Added 14 NuGet package conditional blocks to 4 .csproj files following Clean Architecture layer separation
- NLog web/non-web package split correctly gated on IncludeWebProject in both single-project and clean arch entry point
- IncludeAnyTesting computed symbol added to template.json; testing parameter defaultValue changed from xunit to none
- Both .slnx files updated with conditional test project entries (single vs split variants based on IncludeTestcontainers)
- sources.modifiers added to exclude SampleEntityValidator.cs and SampleMappingConfig.cs when validation/mapping not selected

## Task Commits

Each task was committed atomically:

1. **Task 1: Add logging, quality, and testing package references to all .csproj files** - `c5f8742` (feat)
2. **Task 2: Add IncludeAnyTesting computed symbol, test project sources entries, and .slnx entries** - `6a9cac9` (feat)

## Files Created/Modified

- `templates/dotnet-initializr/src/Company.ProjectName/Company.ProjectName.csproj` - Added Serilog, NLog web/non-web, FluentValidation 12.*, Mapster 7.*, StackExchangeRedis, Http.Resilience, OpenTelemetry with AspNetCore instrumentation gated on web
- `templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/Company.ProjectName.__EntryPoint__.csproj` - Added Serilog and NLog web/non-web logging packages (entry point layer only)
- `templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/Company.ProjectName.Infrastructure.csproj` - Added StackExchangeRedis, Http.Resilience, OpenTelemetry (infrastructure layer)
- `templates/dotnet-initializr/src-clean/Company.ProjectName.Application/Company.ProjectName.Application.csproj` - Added FluentValidation 12.* and Mapster 7.* (application layer)
- `templates/dotnet-initializr/.template.config/template.json` - Added IncludeAnyTesting symbol, changed testing defaultValue to none, fixed mapping description, added tests-single/tests-split sources, added sample file exclusion modifiers
- `templates/dotnet-initializr/Company.ProjectName.slnx` - Added conditional test project entries gated on IncludeAnyTesting
- `templates/dotnet-initializr/Company.ProjectName.clean.slnx` - Added conditional test project entries gated on IncludeAnyTesting

## Decisions Made

- testing parameter defaultValue changed from xunit to none: test project is only generated when user explicitly provides a --testing flag, per CONTEXT.md decision
- IncludeAnyTesting computed value explicitly lists all non-none choices: testing==none evaluates to IncludeAnyTesting=false
- FluentValidation is 12.* (not FluentAssertions which is for test projects and will be in Plan 03)
- OpenTelemetry.Instrumentation.AspNetCore gated on IncludeWebProject — Worker/Console projects do not need ASP.NET Core instrumentation
- mapping parameter description corrected: Mapperly changed to Mapster per user decision from CONTEXT.md

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All .csproj package foundations are in place for Plans 02 and 03
- Plan 02 can now add C# registration code (Program.cs, InfrastructureExtensions.cs, ApplicationExtensions.cs) that references these packages
- Plan 03 can add test project content files (tests-single/ and tests-split/ directories) using the sources entries configured here
- Both .slnx files are ready to receive test project paths as Plans 02/03 create the actual test .csproj files

---
*Phase: 17-logging-testing-and-quality*
*Completed: 2026-03-08*
