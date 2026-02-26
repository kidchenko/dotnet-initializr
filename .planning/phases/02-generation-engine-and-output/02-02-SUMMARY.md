---
phase: 02-generation-engine-and-output
plan: "02"
subsystem: generation
tags: [dotnet, csproj, slnx, appsettings, editorconfig, gitignore, code-generation]

requires:
  - phase: 02-01
    provides: NuGetVersionMap (static version resolution), ProjectConfiguration model

provides:
  - SlnxGenerator.Generate() — .slnx XML for all 3 architecture patterns with optional Aspire/test projects
  - CsprojGenerator.GenerateWebProject() — Web SDK .csproj with conditional NuGet packages
  - CsprojGenerator.GenerateClassLibrary() — Class library .csproj with infrastructure EF Core awareness
  - CsprojGenerator.GenerateTestProject() — xUnit .csproj with FluentAssertions and coverlet
  - CsprojGenerator.GenerateIntegrationTestProject() — xUnit + Testcontainers .csproj
  - CsprojGenerator.GenerateConsoleProject() — Console project .csproj with Exe output
  - CsprojGenerator.GenerateWorkerProject() — Worker service .csproj with Microsoft.Extensions.Hosting
  - CsprojGenerator.GenerateAspireAppHostProject() — Aspire AppHost .csproj
  - CsprojGenerator.GenerateAspireServiceDefaultsProject() — Aspire ServiceDefaults .csproj
  - StaticFileGenerator.GenerateGitignore() — Standard .NET .gitignore
  - StaticFileGenerator.GenerateEditorconfig() — .editorconfig with .NET naming conventions
  - AppSettingsGenerator.GenerateAppSettings() — appsettings.json with conditional sections
  - AppSettingsGenerator.GenerateAppSettingsDevelopment() — Dev overrides appsettings.Development.json

affects: [02-03-architecture-generators, 02-04-feature-generators]

tech-stack:
  added: []
  patterns:
    - "Static generator classes with method-per-project-type pattern"
    - "StringBuilder for multi-line XML generation"
    - "System.Text.Json Dictionary<string,object> for valid JSON without trailing commas"
    - "NuGetVersionMap.GetPackageVersion(sdk, packageName) for all version references"

key-files:
  created:
    - NetStarter/NetStarter/Services/Generation/SlnxGenerator.cs
    - NetStarter/NetStarter/Services/Generation/CsprojGenerator.cs
    - NetStarter/NetStarter/Services/Generation/StaticFileGenerator.cs
    - NetStarter/NetStarter/Services/Generation/AppSettingsGenerator.cs
  modified: []

key-decisions:
  - "CsprojGenerator.GenerateClassLibrary uses projectSuffix='Infrastructure' check to conditionally add EF Core packages — infrastructure layer owns DB access"
  - "AppSettingsGenerator uses Dictionary<string,object> + JsonSerializer.Serialize for JSON to avoid manual string concatenation (no trailing comma bugs)"
  - "All .csproj outputs include TreatWarningsAsErrors=true and AnalysisLevel=latest-recommended per locked project decision"
  - "StaticFileGenerator uses raw string literals (triple-quote) for clean multi-line template content"

patterns-established:
  - "Static generator classes: all generators are static classes with no DI dependency — pure functions on ProjectConfiguration"
  - "Conditional NuGet packages: each generator conditionally adds packages based on config flags (Orm, Auth, IncludeSerilog, IncludeOpenTelemetry, etc.)"
  - "Version resolution via NuGetVersionMap: never hardcode package versions in generator methods"

requirements-completed: [GEN-01, GEN-15, GEN-16, GEN-18]

duration: 2min
completed: 2026-02-26
---

# Phase 2 Plan 02: Core Template Generators Summary

**Four static generator classes producing .slnx, .csproj, .gitignore, .editorconfig, and appsettings.json for all .NET architecture patterns and feature combinations**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-26T17:42:04Z
- **Completed:** 2026-02-26T17:44:00Z
- **Tasks:** 2
- **Files modified:** 4 created

## Accomplishments

- SlnxGenerator handles all 3 architecture patterns (CleanArchitecture 4-project, VerticalSlice, SimpleLayered) plus optional Aspire and test project paths
- CsprojGenerator provides 8 methods covering every .NET project type with conditional NuGet package selection based on config flags
- StaticFileGenerator produces static .gitignore and .editorconfig templates with proper .NET naming conventions
- AppSettingsGenerator uses System.Text.Json for valid JSON with conditional sections for EF Core connection strings, JWT, and Serilog

## Task Commits

Each task was committed atomically:

1. **Task 1: Create SlnxGenerator and CsprojGenerator** - `fc11c4b` (feat)
2. **Task 2: Create StaticFileGenerator and AppSettingsGenerator** - `9438fa7` (feat)

## Files Created/Modified

- `NetStarter/NetStarter/Services/Generation/SlnxGenerator.cs` - .slnx XML generation for all architecture patterns and optional project inclusions
- `NetStarter/NetStarter/Services/Generation/CsprojGenerator.cs` - .csproj generation for web, classlib, test, integration test, console, worker, and Aspire project types
- `NetStarter/NetStarter/Services/Generation/StaticFileGenerator.cs` - .gitignore and .editorconfig static templates with .NET naming conventions
- `NetStarter/NetStarter/Services/Generation/AppSettingsGenerator.cs` - appsettings.json/appsettings.Development.json with conditional EF Core, JWT, Serilog sections

## Decisions Made

- `CsprojGenerator.GenerateClassLibrary` checks `projectSuffix == "Infrastructure"` to decide whether to add EF Core packages — the infrastructure layer owns DB access, not Domain or Application
- `AppSettingsGenerator` builds settings as `Dictionary<string, object>` and serializes with `JsonSerializer.Serialize` to guarantee valid JSON (no trailing comma bugs from manual string concatenation)
- `StaticFileGenerator` uses C# raw string literals (triple-quote `"""`) for clean multi-line content without escape sequences

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All 4 generator classes compile and build with 0 errors, 0 warnings
- SlnxGenerator and CsprojGenerator ready to be composed by architecture-specific generators (Plan 02-03)
- AppSettingsGenerator and StaticFileGenerator ready for use in full project output assembly
- All methods follow the NuGetVersionMap.GetPackageVersion pattern for consistent version resolution

---
*Phase: 02-generation-engine-and-output*
*Completed: 2026-02-26*
