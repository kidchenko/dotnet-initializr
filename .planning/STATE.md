---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: dotnet new Templates
status: unknown
last_updated: "2026-03-09T15:42:21.490Z"
progress:
  total_phases: 7
  completed_phases: 7
  total_plans: 18
  completed_plans: 18
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-07)

**Core value:** Users can generate a fully configured .NET project with their chosen architecture, ORM, auth, observability, and testing setup in seconds — no manual scaffolding.
**Current focus:** v1.3 — Phase 19: Blazor CLI Panel

## Current Position

Phase: 19 of 20 (Blazor CLI Panel)
Plan: 2 complete (19-02 done — Phase19DotNetNewCommandTests: 43 unit tests for DotNetNewCommandService flag mapping)
Status: Phase 19 in progress (2/? plans done)
Last activity: 2026-03-09 — Phase 19 Plan 02 complete: 43 comprehensive unit tests for DotNetNewCommandService; all 306 tests pass; CLI-01, CLI-02, CLI-03 verified

Progress: [████████░░] 58% (v1.3: 18/? plans complete — Phase 19 in progress)

## Performance Metrics

**Velocity (v1.2, for reference):**
- Total plans completed: 11 (across 6 phases in 7 days)
- Tests: 244 total (189 new in v1.2)

**By Phase (v1.3):**

| Phase | Plans | Status |
|-------|-------|--------|
| 13. Template Foundation | 2/2 | Complete |
| 14. Core Parameter Model | 2/2 | Complete |
| 15. Architecture and Project Types | 2/2 | Complete |
| 16. Data Access and Auth | 3/3 | Complete |
| 17. Logging, Testing, and Quality | 4/4 | Complete |
| 18. DevOps, Containers, and Background Jobs | 3/3 | Complete |
| 19. Blazor CLI Panel | 2/? | In progress |
| 20. NuGet Packaging and Distribution | 0/? | Not started |

## Accumulated Context

### Decisions

**Phase 13, Plan 01 (2026-03-08):**
- Controller-based HelloController chosen over MinimalApi — mirrors existing ArchitectureGenerator baseline output
- noEmit escape markers (//-:cnd:noEmit, //+:cnd:noEmit) applied to #if DEBUG in Program.cs
- NoDefaultExcludes=true confirmed required in Initializr.Templates.csproj — verified dotfiles appear in .nupkg
- Content Include uses backslash separators for MSBuild cross-platform compatibility

Key decisions carried into v1.3:

- Template short name: `dotnet-initializr` (not `netstarter`)
- NuGet package name: `Initializr.Templates` (not `NetStarter.Templates`)
- Template uses native `#if` directives — no shared generation engine with Blazor app
- sourceName placeholder: `Company.ProjectName`
- Template project location: `templates/NetStarter.Templates/` (parallel to `src/` and `tests/`)
- Test project: `tests/NetStarter.Templates.Tests/` — kept separate from 244 Blazor unit tests
- `NoDefaultExcludes=true` mandatory in template `.csproj` to include dotfiles
- Development workflow: scripted `dotnet new uninstall / dotnet new install` cycle (never rely on cache)
- All `#if DEBUG` guards in template source files must be wrapped with `noEmit` escape markers
- YAML files (CI/CD, docker-compose) use `sources.modifiers` exclusion only — no inline `#if`
- NuGet.org package ID `Initializr.Templates` must be reserved before Phase 20
- [Phase 13]: 12-step test script structure: clean -> pack -> uninstall -> install -> verify -> generate -> dotfiles -> name substitution -> #if DEBUG -> build -> dir names -> cleanup
- [Phase 13]: Test dir /tmp/dotnet-initializr-test — ephemeral, cleaned up after each run, script is idempotent
- [Phase 13]: noEmit leakage check: grep for noEmit in generated Program.cs confirms template engine consumed markers (not leaked)
- [Phase 14-core-parameter-model]: camelCase symbol names for multi-word params (backgroundJobs, apiDocs, healthChecks) with dotnetcli.host.json longName overrides for kebab-case CLI presentation
- [Phase 14-core-parameter-model]: backgroundJobs parameter always visible (no isEnabled gate); Hangfire-requires-database constraint enforced at content generation in Phase 18
- [Phase 14-core-parameter-model]: Database computed symbols double-gate with explicit orm check: (orm != 'None' && db == 'X') prevents false positives when orm=None
- [Phase 14-core-parameter-model]: isEnabled in .NET 10 is advisory: disabled params use defaultValue silently, no CLI rejection. Multi-value testing requires repeated flags, not comma-separated
- [Phase 14-core-parameter-model]: Gating tests verify 'Enabled if:' help annotation rather than CLI rejection — .NET 10 isEnabled is advisory, silently uses defaultValue when disabled
- [Phase 14-core-parameter-model]: Multi-value --testing test uses repeated flags pattern (--testing xunit --testing fluentassertions), not comma-separated which is unsupported in .NET 10
- [Phase 14-core-parameter-model]: test-template.sh uses start-of-line grep anchor for parameter section detection: grep -A5 '^  -db ' avoids false matches from param references inside other descriptions
- [Phase 15]: EntryPointSuffix fileRename glob requires full folder name (Company.ProjectName.__EntryPoint__) not short __EntryPoint__ in sources modifiers
- [Phase 15]: Microsoft.NET.Sdk.Worker in .NET 10.0 requires explicit Microsoft.Extensions.Hosting package reference
- [Phase 15]: Worker Program.cs needs explicit using for project namespace (using Company.ProjectName.__EntryPoint__) to resolve Worker class in top-level statements
- [Phase 15]: test-template.sh must use /private/tmp (not /tmp) on macOS to avoid duplicate project.assets.json restore conflict from symlink
- [Phase 15-02]: Steps 20-26 implemented during 15-01 deviation item #5 — verification-only plan; all 26 steps pass including Phase 15 arch x type verification
- [Phase 16-01]: EfCoreVersion switch generator reused for JwtBearer and Identity.EF packages (same .NET major version alignment 8.*/9.*/10.*)
- [Phase 16-01]: IncludeAspNetIdentity double-gated on auth==AspNetIdentity AND orm==EfCore — prevents Identity code without EF Core
- [Phase 16-01]: Microsoft.Data.Sqlite uses __EfCoreVersion__ token (tracks .NET version); standalone Npgsql hardcoded 9.*
- [Phase 16-01]: Sources.modifiers Data/ and Models/ exclusion split: !IncludeSimpleLayered gates Services/, but Data/ and Models/ also require !IncludeAnyOrm to allow VerticalSlice+ORM scenarios
- [Phase 16-01]: SwashbuckleVersion uses single-case switch generator (always 10.*) for consistent token replacement pattern
- [Phase 16-02]: ISampleRepository lives in Domain/Repositories (not Application) — pure Clean Architecture separation per user decision
- [Phase 16-02]: Fully qualified type names in #if blocks (e.g., Microsoft.AspNetCore.Identity.IdentityUser) to prevent CS8019 with TreatWarningsAsErrors enabled
- [Phase 16-02]: appsettings.json conditional sections use leading comma on first content line — template engine strips #if lines so comma must be on data line
- [Phase 16-02]: ORM registration added to Console/Worker project type branches — background workers need data access
- [Phase 16-02]: SampleRepository #else branch preserves ISampleService stub fallback when no ORM selected
- [Phase 16-03]: Verification steps follow generate-assert-build-cleanup pattern; version token substitution verified via grep for __TokenName__ patterns in generated output
- [Phase 17-01]: testing parameter defaultValue changed from xunit to none — test project only generated when user explicitly passes --testing flag
- [Phase 17-01]: NLog web/non-web split critical constraint: NLog.Web.AspNetCore for web, NLog.Extensions.Hosting for Worker/Console — gated on IncludeNLog && IncludeWebProject
- [Phase 17-01]: IncludeAnyTesting computed value omits none — testing==none evaluates to IncludeAnyTesting=false, no test project sources included
- [Phase 17-01]: FluentValidation 12.* in .csproj (not FluentAssertions which is for test projects in Plan 03)
- [Phase 17-01]: OpenTelemetry.Instrumentation.AspNetCore gated on IncludeWebProject — Worker/Console do not need ASP.NET Core instrumentation
- [Phase 17-02]: NLog namespace split in Program.cs: NLog.Web for WebApi/MinimalApi, NLog.Extensions.Hosting for Console/Worker
- [Phase 17-02]: Redis connection string dual-block in appsettings.json: comma inside IncludeAnyOrm block + standalone block for caching-only scenario
- [Phase 17-02]: Single-project SampleEntityValidator uses self-contained SampleRequest type (no domain layer in single-project templates)
- [Phase 17-02]: ApplicationExtensions uses Mapster.TypeAdapterConfig fully qualified to prevent CS8019 with TreatWarningsAsErrors
- [Phase 17]: FluentAssertions pinned to 7.* in test projects (v8 is commercial); xunit.runner.visualstudio pinned to 2.* (.NET 10 issues with v3.x)
- [Phase 17]: IntegrationTests references Infrastructure (not Application) for CleanArchitecture — data access testing requires Infrastructure layer
- [Phase 17]: SampleIntegrationTests.cs uses IAsyncLifetime for async container start/stop lifecycle (xUnit idiomatic pattern)
- [Phase 17]: --openTelemetry (camelCase) used as CLI flag — no longName override in dotnetcli.host.json
- [Phase 17]: Kitchen sink step 52 uses xunit+fluentassertions+nsubstitute (no testcontainers) — produces single Tests/ project
- [Phase 18]: IncludeHangfire double-gated on (backgroundJobs == 'Hangfire' && orm \!= 'None') — Hangfire requires database storage; no ORM means no storage backend
- [Phase 18]: Jobs/ exclusion updated from project-type-only check to strategy-aware: (\!IncludeIHostedService && \!IncludeHangfire && \!IncludeQuartz && \!IncludeWorker && \!IncludeConsole)
- [Phase 18]: Template constraints block with type:expression for api-docs non-web project type validation — produces explicit error for Console/WorkerService
- [Phase 18]: CI/CD YAML files have no #if directives — two physical files per provider selected via condition-based source blocks with rename (GEN-07 compliance)
- [Phase 18]: DotNetSdkVersion switch generator maps net8.0/net9.0/net10.0 to 8.0.x/9.0.x/10.0.x via __DotNetSdkVersion__ token replacement in CI/CD YAML
- [Phase 18]: Aspire selection does NOT trigger Docker CI/CD variant — Aspire is dev-time orchestration, not Docker deployment strategy
- [Phase 18]: Aspire AppHost uses AddProject(name, path) overload — path-based approach works with sourceName and __EntryPoint__ template replacements
- [Phase 18-devops-containers-and-background-jobs]: Aspire build step conditional on workload availability — skip if aspire workload not installed to avoid CI failures
- [Phase 19-01]: Flag emission compares against template.json defaults (not Blazor defaults) — orm defaults EfCore in template, None in Blazor
- [Phase 19-01]: healthChecks and validation default true in template.json — must emit --flag false when Blazor booleans are false
- [Phase 19-01]: containers is single-choice (priority: Aspire > DockerCompose > Dockerfile); cicd is single-choice (priority: GitHubActions > AzureDevOps)
- [Phase 19-01]: DotNetNewPanel expanded by default (true) vs CliCommandPanel collapsed by default (false)
- [Phase 19-02]: Test CreateConfig factory uses Blazor-matching defaults to make default-omission intent explicit in each test
- [Phase 19-02]: GetFlagString helper extracts second command (dotnet new dotnet-initializr line) for concise Assert.Contains/DoesNotContain assertions

### Pending Todos

None.

### Blockers/Concerns

- NuGet.org package ID `Initializr.Templates` reservation — verify availability before Phase 20

## Session Continuity

Last session: 2026-03-09
Stopped at: Completed 19-02-PLAN.md — Phase19DotNetNewCommandTests: 43 unit tests for DotNetNewCommandService flag mapping; all 306 tests pass; 1 task, 1 file, commit 1658e75
Resume file: None
