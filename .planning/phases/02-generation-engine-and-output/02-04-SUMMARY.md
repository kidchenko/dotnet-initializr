---
phase: 02-generation-engine-and-output
plan: 04
subsystem: generation
tags: [efcore, jwt, opentelemetry, mapster, code-generation, csharp]

# Dependency graph
requires:
  - phase: 02-generation-engine-and-output
    provides: ProjectConfiguration model with Orm, Auth, Mapping, and observability fields

provides:
  - EfCoreGenerator: DbContext with primary constructor pattern and SampleEntity code templates
  - AuthGenerator: JwtSettings POCO bound to appsettings Jwt section
  - ObservabilityGenerator: OpenTelemetry extension method with conditional EF Core instrumentation
  - MappingGenerator: Mapster IRegister implementation

affects:
  - 02-generation-engine-and-output
  - ArchitectureGenerator (consumes these generators to place files)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Pure string factory generators — each generator is a static class with no dependencies beyond ProjectConfiguration
    - Namespace suffix helpers co-located with each generator (GetDbContextNamespaceSuffix, GetNamespaceSuffix)
    - Architecture-aware namespace routing: CleanArchitecture gets sub-namespace (e.g. Infrastructure.Data), others get flat suffix (Data)
    - String concatenation with $ for generated C# code containing braces (avoids raw string literal brace-escaping issues)

key-files:
  created:
    - NetStarter/NetStarter/Services/Generation/EfCoreGenerator.cs
    - NetStarter/NetStarter/Services/Generation/AuthGenerator.cs
    - NetStarter/NetStarter/Services/Generation/ObservabilityGenerator.cs
    - NetStarter/NetStarter/Services/Generation/MappingGenerator.cs
  modified:
    - NetStarter/NetStarter/Services/Generation/DockerGenerator.cs

key-decisions:
  - "String concatenation with $ used for generated C# code — raw string literals cannot easily escape {{ }} when generating property accessor syntax like { get; set; }"
  - "Namespace suffix helper methods included in each generator class for cohesion"
  - "ObservabilityGenerator EF Core instrumentation conditional on config.Orm == OrmOption.EfCore at generation time"

patterns-established:
  - "Generator pattern: static class, pure string factory, one method per generated file type, namespace suffix helper method"

requirements-completed: [GEN-05, GEN-06, GEN-07, GEN-08, GEN-21, GEN-22]

# Metrics
duration: 2min
completed: 2026-02-26
---

# Phase 02 Plan 04: Feature Generators (EF Core, Auth, Observability, Mapping) Summary

**Four static code generator classes producing EF Core DbContext/entity, JwtSettings POCO, OpenTelemetry extension method with conditional EF Core instrumentation, and Mapster IRegister implementation**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-26T17:42:08Z
- **Completed:** 2026-02-26T17:44:15Z
- **Tasks:** 2
- **Files modified:** 5 (4 created, 1 modified)

## Accomplishments

- EfCoreGenerator produces AppDbContext using C# 12 primary constructor pattern with DbSet<SampleEntity>
- AuthGenerator produces JwtSettings with Issuer/Audience/Key properties matching appsettings.json Jwt section
- ObservabilityGenerator produces AddAppOpenTelemetry extension method; includes AddEntityFrameworkCoreInstrumentation only when config.Orm == OrmOption.EfCore
- MappingGenerator produces MappingConfig : IRegister with TypeAdapterConfig parameter as Mapster convention requires
- All four generators compile with 0 errors, 0 warnings

## Task Commits

Each task was committed atomically:

1. **Task 1: Create EfCoreGenerator and AuthGenerator** - `971aee0` (feat)
2. **Task 2: Create ObservabilityGenerator and MappingGenerator** - `1662d97` (feat)

## Files Created/Modified

- `NetStarter/NetStarter/Services/Generation/EfCoreGenerator.cs` - Static generator for AppDbContext and SampleEntity with architecture-aware namespace suffixes
- `NetStarter/NetStarter/Services/Generation/AuthGenerator.cs` - Static generator for JwtSettings POCO with architecture-aware namespace suffix
- `NetStarter/NetStarter/Services/Generation/ObservabilityGenerator.cs` - Static generator for OpenTelemetry extension method with conditional EF Core instrumentation
- `NetStarter/NetStarter/Services/Generation/MappingGenerator.cs` - Static generator for Mapster MappingConfig : IRegister
- `NetStarter/NetStarter/Services/Generation/DockerGenerator.cs` - Fixed raw string literal delimiter bug (CS9000)

## Decisions Made

- String concatenation with `$"..."` used for generated C# file content — raw string literals (`$$"""..."""`) require `{{` to produce `{` but when generating property accessors like `{ get; set; }`, the compiler sees inner content as interpolation expressions, causing parse errors. String concatenation avoids this entirely.
- Namespace suffix helper methods (`GetDbContextNamespaceSuffix`, `GetEntityNamespaceSuffix`, `GetNamespaceSuffix`) placed in each generator class so callers have a single import.
- EF Core instrumentation in OpenTelemetry is conditional at string generation time (not runtime), keeping generated code clean without runtime branches.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed raw string literal delimiter error CS9000 in DockerGenerator.cs**
- **Found during:** Task 1 (initial build verification)
- **Issue:** Line 98 of DockerGenerator.cs had `- {{dbServiceName}}"""` on the same line — C# raw string literals require the closing `"""` delimiter to be on its own line
- **Fix:** Replaced the multi-line `$$"""..."""` block with a simple `$"..."` string interpolation for the `dependsOn` variable
- **Files modified:** NetStarter/NetStarter/Services/Generation/DockerGenerator.cs
- **Verification:** `dotnet build` passes with 0 errors after fix
- **Committed in:** `971aee0` (Task 1 commit)

**2. [Rule 1 - Bug] Fixed raw string interpolation with C# property accessor syntax**
- **Found during:** Task 1 (initial write and build)
- **Issue:** Initial implementation used `$"""..."""` raw string literals; `{ get; set; }` inside was parsed as interpolation expression, causing CS1073/CS9006/CS1525 compile errors across EfCoreGenerator.cs and AuthGenerator.cs
- **Fix:** Rewrote all generated code output as string concatenation using `$"line\n" + $"line\n"` pattern
- **Files modified:** EfCoreGenerator.cs, AuthGenerator.cs, ObservabilityGenerator.cs, MappingGenerator.cs
- **Verification:** `dotnet build` passes with 0 errors after fix
- **Committed in:** `971aee0` (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (both Rule 1 bugs)
**Impact on plan:** Both fixes needed for compilation — no scope creep.

## Issues Encountered

- C# raw string literals (`$$"""..."""`) are unsuitable for generating C# code that contains property accessor syntax `{ get; set; }` — the parser interprets inner `{` as interpolation even when using `$$`. String concatenation is the correct pattern for this codebase's generators.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All four feature generators are ready for consumption by ArchitectureGenerator (Plan 02-05 or 02-06)
- Each generator has architecture-aware namespace suffix helpers that ArchitectureGenerator can call directly
- ObservabilityGenerator and ProgramCsGenerator coordination: when OTel is selected, Program.cs should call `builder.Services.AddAppOpenTelemetry(builder.Configuration)` — both plans are in wave 2, this file establishes the pattern

---
*Phase: 02-generation-engine-and-output*
*Completed: 2026-02-26*
