---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: dotnet new Templates
status: unknown
last_updated: "2026-03-08T17:49:00.000Z"
progress:
  total_phases: 3
  completed_phases: 2
  total_plans: 6
  completed_plans: 6
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-07)

**Core value:** Users can generate a fully configured .NET project with their chosen architecture, ORM, auth, observability, and testing setup in seconds — no manual scaffolding.
**Current focus:** v1.3 — Phase 15: Architecture and Project Types

## Current Position

Phase: 15 of 20 (Architecture and Project Types)
Plan: 2 complete (15-01, 15-02 done)
Status: Phase 15 complete
Last activity: 2026-03-08 — Phase 15 Plan 02 complete: 7 test-template.sh steps (20-26) verified passing; all arch x type combinations confirmed generating correct folder structures and building

Progress: [████░░░░░░] 20% (v1.3: 6/? plans complete — Phase 15 complete, Phase 16 next)

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
| 16. Data Access and Auth | 0/? | Not started |
| 17. Logging, Testing, and Quality | 0/? | Not started |
| 18. DevOps, Containers, and Background Jobs | 0/? | Not started |
| 19. Blazor CLI Panel | 0/? | Not started |
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

### Pending Todos

None.

### Blockers/Concerns

- NuGet.org package ID `Initializr.Templates` reservation — verify availability before Phase 20

## Session Continuity

Last session: 2026-03-08
Stopped at: Completed 15-02-PLAN.md — all 26 test-template.sh steps verified passing, Phase 15 complete
Resume file: None
