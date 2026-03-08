---
phase: 17-logging-testing-and-quality
plan: "02"
subsystem: template-source-files
tags: [logging, serilog, nlog, health-checks, validation, caching, resilience, opentelemetry, mapping, fluentvalidation, mapster, program-cs, appsettings]
dependency_graph:
  requires: ["17-01"]
  provides: ["logging-registrations", "quality-feature-registrations", "sample-validator", "sample-mapping"]
  affects: ["all-template-generated-projects"]
tech_stack:
  added: []
  patterns: ["conditional-using-blocks", "leading-comma-json-pattern", "IncludeWebProject-gate", "NLog-namespace-split-pattern"]
key_files:
  created:
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Application/SampleEntityValidator.cs
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Application/SampleMappingConfig.cs
    - templates/dotnet-initializr/src/Company.ProjectName/Application/SampleEntityValidator.cs
    - templates/dotnet-initializr/src/Company.ProjectName/Application/SampleMappingConfig.cs
  modified:
    - templates/dotnet-initializr/src/Company.ProjectName/Program.cs
    - templates/dotnet-initializr/src/Company.ProjectName/appsettings.json
    - templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/Program.cs
    - templates/dotnet-initializr/src-clean/Company.ProjectName.__EntryPoint__/appsettings.json
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Infrastructure/InfrastructureExtensions.cs
    - templates/dotnet-initializr/src-clean/Company.ProjectName.Application/ApplicationExtensions.cs
decisions:
  - "NLog namespace split: NLog.Web for web (WebApi/MinimalApi), NLog.Extensions.Hosting for Console/Worker — consistent with Phase 17-01 constraint"
  - "Redis connection string dual-block: inner comma inside IncludeAnyOrm block (IncludeCaching), plus standalone block (IncludeCaching && !IncludeAnyOrm) — avoids duplicate ConnectionStrings key"
  - "IncludeWebProject gate on AddAspNetCoreInstrumentation in InfrastructureExtensions — Console/Worker have no HTTP context"
  - "Single-project SampleEntityValidator uses self-contained SampleRequest type (not domain entity reference) — no domain layer in single-project"
  - "ApplicationExtensions uses Mapster.TypeAdapterConfig fully qualified — prevents CS8019 with TreatWarningsAsErrors when using is inside #if block"
metrics:
  duration_seconds: 226
  completed_date: "2026-03-08"
  tasks_completed: 2
  files_modified: 6
  files_created: 4
---

# Phase 17 Plan 02: Logging, Health-Checks, and Quality Feature Registrations Summary

**One-liner:** Wired Serilog/NLog host setup, health checks, validation, caching, resilience, OpenTelemetry, and Mapster into all Program.cs, InfrastructureExtensions, ApplicationExtensions, and appsettings.json files for both template architectures with correct conditional #if blocks.

## Tasks Completed

| Task | Name | Commit | Key Files |
|------|------|--------|-----------|
| 1 | Add logging, health-checks, and quality registrations to Program.cs (both arch) and appsettings.json | 0f26962 | Program.cs (both arch), appsettings.json (both arch) |
| 2 | Add quality feature registrations to InfrastructureExtensions and ApplicationExtensions, create sample files | e4860c4 | InfrastructureExtensions.cs, ApplicationExtensions.cs, SampleEntityValidator.cs (x2), SampleMappingConfig.cs (x2) |

## What Was Built

### Task 1: Program.cs and appsettings.json Updates

**Single-project Program.cs** (`src/Company.ProjectName/Program.cs`):
- All 4 branches (WebApi, MinimalApi, Console, Worker) have conditional Serilog/NLog using statements and host setup
- Web branches: `NLog.Web` namespace, `builder.Host.UseNLog()`; Console/Worker branches: `NLog.Extensions.Hosting` namespace, `.UseNLog()` on host builder chain
- Web branches have inline quality registrations: FluentValidation (`AddValidatorsFromAssemblyContaining<Program>`), Redis cache, resilience, OpenTelemetry (with AspNetCore instrumentation), Mapster scan
- Console/Worker branches have same quality registrations inside `ConfigureServices` (without AspNetCore instrumentation)
- Health check service registration (`AddHealthChecks`) and endpoint mapping (`MapHealthChecks("/health")`) in web branches only

**Clean architecture Program.cs** (`src-clean/Company.ProjectName.__EntryPoint__/Program.cs`):
- All 4 branches have conditional Serilog/NLog using statements and host setup
- Quality features delegated to `AddApplication()` / `AddInfrastructure()` — Program.cs only handles logging and health checks
- Health check service registration and endpoint mapping in web branches only

**Both appsettings.json files**:
- Serilog section with Console and File sinks, minimum level overrides for Microsoft/System
- NLog section with console and file targets and rules
- Redis connection string: nested inside existing ORM `ConnectionStrings` block (`IncludeCaching`), plus standalone `ConnectionStrings` block (`IncludeCaching && !IncludeAnyOrm`) to avoid duplicate key

### Task 2: Extension Classes and Sample Files

**InfrastructureExtensions.cs**:
- Added `AddStackExchangeRedisCache` registration with Redis connection string
- Added `ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler())` resilience registration
- Added `AddOpenTelemetry()` with tracing and metrics, `AddAspNetCoreInstrumentation()` gated on `IncludeWebProject`, always includes `AddHttpClientInstrumentation()` and `AddConsoleExporter()`
- Added OpenTelemetry using statements (`OpenTelemetry.Resources`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`)

**ApplicationExtensions.cs**:
- Added `services.AddValidatorsFromAssemblyContaining<ApplicationExtensions>()` gated on `IncludeValidation`
- Added `Mapster.TypeAdapterConfig.GlobalSettings.Scan(...)` and `services.AddMapster()` gated on `IncludeMapping`
- Using `Mapster.TypeAdapterConfig` fully qualified to avoid CS8019 with TreatWarningsAsErrors

**Sample Files (4 new files)**:
- `src-clean/Company.ProjectName.Application/SampleEntityValidator.cs` — `AbstractValidator<SampleEntity>` referencing domain entity, wrapped in `#if (IncludeValidation)`
- `src-clean/Company.ProjectName.Application/SampleMappingConfig.cs` — `IRegister` mapping `SampleEntity` to `SampleDto`, wrapped in `#if (IncludeMapping)`
- `src/Company.ProjectName/Application/SampleEntityValidator.cs` — Self-contained `SampleRequest` + `SampleRequestValidator` (no domain entity dependency), wrapped in `#if (IncludeValidation)`
- `src/Company.ProjectName/Application/SampleMappingConfig.cs` — Self-contained `SampleSource`/`SampleDto` records + `SampleMappingConfig : IRegister`, wrapped in `#if (IncludeMapping)`

## Deviations from Plan

None — plan executed exactly as written.

## Verification Results

- Single-project Program.cs: 32 matches for feature registration keywords (4 branches x logging + quality features)
- Clean arch Program.cs: 12 matches for logging and health-check keywords (4 branches x Serilog/NLog/HealthChecks patterns)
- InfrastructureExtensions.cs: 3 matches (AddStackExchangeRedisCache, AddStandardResilienceHandler, AddOpenTelemetry)
- ApplicationExtensions.cs: 3 matches (AddValidatorsFromAssemblyContaining, TypeAdapterConfig, AddMapster)
- Both appsettings.json: 7 matches each for Serilog/NLog/Redis sections
- NLog namespace split verified: `NLog.Web` in web branches, `NLog.Extensions.Hosting` in Console/Worker
- `IncludeWebProject` gate confirmed in InfrastructureExtensions OTel block (2 occurrences)
- All 4 sample files exist and verified

## Self-Check: PASSED

Files created:
- FOUND: templates/dotnet-initializr/src-clean/Company.ProjectName.Application/SampleEntityValidator.cs
- FOUND: templates/dotnet-initializr/src-clean/Company.ProjectName.Application/SampleMappingConfig.cs
- FOUND: templates/dotnet-initializr/src/Company.ProjectName/Application/SampleEntityValidator.cs
- FOUND: templates/dotnet-initializr/src/Company.ProjectName/Application/SampleMappingConfig.cs

Commits verified:
- FOUND: 0f26962 (Task 1 — Program.cs and appsettings.json)
- FOUND: e4860c4 (Task 2 — extension classes and sample files)
