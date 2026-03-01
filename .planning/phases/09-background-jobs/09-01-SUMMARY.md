---
phase: 09-background-jobs
plan: 01
subsystem: api
tags: [hangfire, quartz, ihostedservice, background-jobs, dotnet, csproj, program-cs, filetree]

# Dependency graph
requires:
  - phase: 08-openapi-documentation
    provides: OpenAPI service/middleware fragments and package emission pattern used as reference

provides:
  - BackgroundJobsGenerator.cs with GenerateSampleBackgroundService, GenerateSampleHangfireJob, GenerateSampleQuartzJob, GetFilePath
  - CsprojGenerator.AppendFeaturePackages emits Hangfire (core + AspNetCore + storage) and Quartz (3 packages) based on selection
  - ProgramCsGenerator.AddBackgroundJobsServicesFragment emits correct service registrations for all three strategies
  - ProgramCsGenerator.AddHangfireDashboardFragment emits dashboard middleware only for WebApi/MinimalApi
  - FileTreeService Jobs/ folder with correct sample file for all three architecture builder methods
  - ProjectGenerationService conditionally generates sample job class file

affects: [10-url-persistence, 11-responsive]

# Tech tracking
tech-stack:
  added: [Hangfire 1.*, Hangfire.AspNetCore 1.*, Hangfire.PostgreSql, Hangfire.SqlServer, Hangfire.MySqlStorage, Quartz 3.*, Quartz.Extensions.Hosting, Quartz.Extensions.DependencyInjection]
  patterns:
    - Static generator class with GenerateSample* + GetFilePath methods (matches DapperGenerator/AuthGenerator pattern)
    - BackgroundJobs == None guard at each injection point
    - ProjectType.Console excluded from all background job emission
    - Hangfire dashboard only for WebApi/MinimalApi in IsDevelopment() guard

key-files:
  created:
    - src/NetStarter/Services/Generation/BackgroundJobsGenerator.cs
  modified:
    - src/NetStarter/Services/Generation/CsprojGenerator.cs
    - src/NetStarter/Services/Generation/ProgramCsGenerator.cs
    - src/NetStarter/Services/FileTreeService.cs
    - src/NetStarter/Services/Generation/ProjectGenerationService.cs

key-decisions:
  - "IHostedService emits zero NuGet packages — BackgroundService is already in Microsoft.Extensions.Hosting.Abstractions"
  - "Hangfire SQLite has no official storage package — null returned, no package emitted (SQLite silently skipped)"
  - "Hangfire dashboard uses IsDevelopment() guard and only emitted for WebApi/MinimalApi project types"
  - "Worker Service gets background job registrations but no Hangfire dashboard"
  - "Console project type gets no background job code at all (guard at every injection point)"

patterns-established:
  - "BackgroundJobs != None && ProjectType != Console: universal guard for background jobs emission"
  - "Fragment methods follow existing pattern: early return on None/inapplicable, append to StringBuilder"

requirements-completed: [JOBS-02, JOBS-03, JOBS-04, JOBS-05, JOBS-06]

# Metrics
duration: 15min
completed: 2026-03-01
---

# Phase 9 Plan 01: Background Jobs Backend Wiring Summary

**IHostedService, Hangfire, and Quartz.NET generators with NuGet package emission, Program.cs fragments, Jobs/ file tree folder, and sample class file generation for all three strategies**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-01T06:08:16Z
- **Completed:** 2026-03-01T06:23:00Z
- **Tasks:** 2
- **Files modified:** 5 (1 created, 4 modified)

## Accomplishments

- Created BackgroundJobsGenerator.cs with three Generate* methods and GetFilePath following the established static generator pattern
- Wired CsprojGenerator.AppendFeaturePackages to emit Hangfire (core+AspNetCore+storage) or three Quartz packages based on selection; zero packages for IHostedService
- Wired ProgramCsGenerator with AddBackgroundJobsServicesFragment and AddHangfireDashboardFragment; using Hangfire; emitted when Hangfire selected
- All three FileTreeService architecture builder methods add Jobs/ folder with correct sample filename
- ProjectGenerationService generates sample job class file for any (BackgroundJobs, Database, ProjectType) combination
- All 154 existing tests pass with zero regressions

## Task Commits

Each task was committed atomically:

1. **Task 1: Add BackgroundJobsGenerator.cs and wire CsprojGenerator packages** - `bcf3b05` (feat)
2. **Task 2: Add ProgramCsGenerator fragments, FileTreeService Jobs/ folder, and ProjectGenerationService wiring** - `5412cf2` (feat)

**Plan metadata:** (docs commit below)

## Files Created/Modified

- `src/NetStarter/Services/Generation/BackgroundJobsGenerator.cs` - New static generator with sample class content for all three strategies and GetFilePath
- `src/NetStarter/Services/Generation/CsprojGenerator.cs` - Background jobs NuGet package block added to AppendFeaturePackages
- `src/NetStarter/Services/Generation/ProgramCsGenerator.cs` - AddBackgroundJobsServicesFragment and AddHangfireDashboardFragment methods; usings and worker wiring
- `src/NetStarter/Services/FileTreeService.cs` - Jobs/ folder added to all three architecture builder methods
- `src/NetStarter/Services/Generation/ProjectGenerationService.cs` - Conditional background job sample file injection (section 3.5)

## Decisions Made

- IHostedService emits zero NuGet packages: BackgroundService lives in Microsoft.Extensions.Hosting.Abstractions, already referenced by all project types
- SQLite Hangfire storage: no official package exists, null is returned, no package emitted (silently skipped)
- Hangfire dashboard guarded by IsDevelopment() and limited to WebApi/MinimalApi only; Worker Service omits dashboard
- Console project type excluded from all background job emission at every injection point

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Background jobs generation fully wired into all backend generators
- Phase 9 Plan 01 complete; ready for any Phase 9 Plan 02 or Phase 10 (URL persistence)
- Hangfire MySQL blocker from STATE.md (MySqlStorage 2.*) resolved: Hangfire.MySqlStorage package is in NuGetVersionMap and correctly emitted

---
*Phase: 09-background-jobs*
*Completed: 2026-03-01*
