---
phase: 02-generation-engine-and-output
plan: "03"
subsystem: generation
tags: [dotnet, program-cs, architecture, code-generation, clean-architecture, vertical-slice, simple-layered]

requires:
  - phase: 02-01
    provides: NuGetVersionMap (static version resolution), ProjectConfiguration model
  - phase: 02-02
    provides: CsprojGenerator, AppSettingsGenerator, SlnxGenerator, StaticFileGenerator
  - phase: 02-04
    provides: ObservabilityGenerator.GenerateOpenTelemetryExtensions (called by ArchitectureGenerator)
    note: ObservabilityGenerator already existed at time of execution (02-04 was completed before 02-03)

provides:
  - ProgramCsGenerator.Generate() — Program.cs for all 4 project types with conditional feature wiring
  - ArchitectureGenerator.GenerateCleanArchitecture() — 4-project structure with inter-project references
  - ArchitectureGenerator.GenerateVerticalSlice() — single project with Features/ placeholder
  - ArchitectureGenerator.GenerateSimpleLayered() — single project with Controllers/Services/Data layout
  - ArchitectureGenerator.GenerateConsoleOrWorker() — single-project helper for Console/WorkerService types

affects: [02-06-project-generation-service]

tech-stack:
  added: []
  patterns:
    - "String concatenation with $ for generated C# code — avoids {{ }} escaping issues in raw string literals"
    - "Private helper methods BuildUsings/BuildServiceRegistrations/BuildMiddleware for clean Program.cs composition"
    - "InjectProjectReferences helper inserts ProjectReference ItemGroups before closing </Project> tag"
    - "Architecture generators compose all feature generators (EfCoreGenerator, AuthGenerator, ObservabilityGenerator, MappingGenerator)"

key-files:
  created:
    - NetStarter/NetStarter/Services/Generation/ProgramCsGenerator.cs
    - NetStarter/NetStarter/Services/Generation/ArchitectureGenerator.cs
  modified: []

key-decisions:
  - "ProgramCsGenerator uses project type switch at top level — Console/WorkerService get entirely separate templates, not stripped-down WebApplication"
  - "ArchitectureGenerator.InjectProjectReferences inserts before closing </Project> tag — avoids re-parsing or duplicating csproj structure"
  - "Features/.gitkeep and Services/.gitkeep used for empty folders — zip archives cannot store empty directories"
  - "Worker.cs generated inline in ArchitectureGenerator.GenerateConsoleOrWorker — keeps all architecture assembly logic in one class"

metrics:
  duration: 1min
  completed: 2026-02-26T17:51:57Z
  tasks: 2
  files: 2 created
---

# Phase 2 Plan 03: Program.cs and Architecture Generators Summary

**ProgramCsGenerator produces compilable Program.cs for all 4 project types with conditional wiring for 6 feature flags; ArchitectureGenerator composes all generators into complete file dictionaries for Clean Architecture (4-project), Vertical Slice, and Simple Layered patterns**

## Performance

- **Duration:** 1 min
- **Started:** 2026-02-26T17:50:04Z
- **Completed:** 2026-02-26T17:51:57Z
- **Tasks:** 2
- **Files modified:** 2 created

## Accomplishments

- ProgramCsGenerator handles all 4 project types: WebApi (MapControllers), MinimalApi (MapGet sample endpoint), Console (Console.WriteLine), WorkerService (Host.CreateApplicationBuilder + AddHostedService)
- Conditional usings for EF Core, JWT (JwtBearer + IdentityModel.Tokens + System.Text), Serilog, OpenTelemetry (namespace import for AddAppOpenTelemetry extension), Mapster
- Conditional service registrations: DbContext with correct provider method (UseNpgsql/UseSqlServer), JWT AddAuthentication+AddJwtBearer with TokenValidationParameters, Serilog UseSerilog, Health Checks AddHealthChecks, OpenTelemetry AddAppOpenTelemetry, Mapster TypeAdapterConfig + ServiceMapper
- Conditional middleware: UseAuthentication+UseAuthorization for JWT, MapHealthChecks, MapControllers/MapGet
- ArchitectureGenerator.GenerateCleanArchitecture produces Domain/Application/Infrastructure/Api sub-projects with correct inter-project references (Infrastructure→Application, Application→Domain, Api→Infrastructure+Application)
- ArchitectureGenerator.GenerateVerticalSlice produces single project with Features/ placeholder folder
- ArchitectureGenerator.GenerateSimpleLayered produces single project with Controllers (WebApi) or Endpoints (MinimalApi) and Services/Data folders
- ArchitectureGenerator.GenerateConsoleOrWorker handles Console/WorkerService as single-project output regardless of architecture setting
- All feature generators composed: CsprojGenerator, ProgramCsGenerator, AppSettingsGenerator, EfCoreGenerator, AuthGenerator, ObservabilityGenerator, MappingGenerator

## Task Commits

Each task was committed atomically:

1. **Task 1: Create ProgramCsGenerator with conditional feature wiring** - `2f09624` (feat)
2. **Task 2: Create ArchitectureGenerator for all three architecture patterns** - `a40737e` (feat)

## Files Created/Modified

- `NetStarter/NetStarter/Services/Generation/ProgramCsGenerator.cs` — Program.cs generation for WebApi, MinimalApi, Console, WorkerService with BuildUsings/BuildServiceRegistrations/BuildMiddleware helpers
- `NetStarter/NetStarter/Services/Generation/ArchitectureGenerator.cs` — Architecture-specific file dictionary population with Clean Architecture (4-project with inter-project refs), Vertical Slice, Simple Layered, and Console/Worker patterns

## Decisions Made

- `ProgramCsGenerator` switches on `ProjectType` first — Console and WorkerService get entirely separate templates (no WebApplication at all), not conditional blocks that strip down the web template. This produces cleaner, more idiomatic generated code.
- `InjectProjectReferences` helper in ArchitectureGenerator inserts `<ProjectReference>` ItemGroups by string-searching for `</Project>` in the already-generated csproj content — avoids XML parsing or re-generating the whole file.
- Empty folder placeholders use `.gitkeep` files — zip archives cannot represent empty directories, so a zero-byte placeholder ensures the folder appears in the generated output.
- `Worker.cs` BackgroundService generated inline in `GenerateConsoleOrWorker` — keeps all architecture assembly logic co-located in ArchitectureGenerator rather than creating a separate WorkerGenerator.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- ProgramCsGenerator and ArchitectureGenerator compile with 0 errors, 0 warnings
- All architecture patterns produce file dictionaries ready for ProjectGenerationService (Plan 02-06) to consume
- ArchitectureGenerator correctly calls all existing feature generators (EfCoreGenerator, AuthGenerator, ObservabilityGenerator, MappingGenerator)
- Console and WorkerService produce single-project output regardless of architecture setting

---
*Phase: 02-generation-engine-and-output*
*Completed: 2026-02-26*
