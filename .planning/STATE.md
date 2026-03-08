---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: dotnet new Templates
status: unknown
last_updated: "2026-03-08T04:32:06.199Z"
progress:
  total_phases: 2
  completed_phases: 2
  total_plans: 4
  completed_plans: 4
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-07)

**Core value:** Users can generate a fully configured .NET project with their chosen architecture, ORM, auth, observability, and testing setup in seconds — no manual scaffolding.
**Current focus:** v1.3 — Phase 14: Core Parameter Model

## Current Position

Phase: 14 of 20 (Core Parameter Model)
Plan: 2 complete (14-01, 14-02 done)
Status: Phase 14 complete
Last activity: 2026-03-08 — Phase 14 Plan 02 complete: test-template.sh extended with 7 Phase 14 parameter model steps (--help validation, isEnabled gating annotation checks, multi-value repeated flags, Console build)

Progress: [███░░░░░░░] 15% (v1.3: 3/? plans complete — Phase 14 in progress)

## Performance Metrics

**Velocity (v1.2, for reference):**
- Total plans completed: 11 (across 6 phases in 7 days)
- Tests: 244 total (189 new in v1.2)

**By Phase (v1.3):**

| Phase | Plans | Status |
|-------|-------|--------|
| 13. Template Foundation | 2/2 | Complete |
| 14. Core Parameter Model | 2/2 | Complete |
| 15. Architecture and Project Types | 0/? | Not started |
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

### Pending Todos

None.

### Blockers/Concerns

- NuGet.org package ID `Initializr.Templates` reservation — verify availability before Phase 20

## Session Continuity

Last session: 2026-03-08
Stopped at: Completed 14-02-PLAN.md — test-template.sh extended to 19-step verification suite, all Phase 14 parameter model checks passing
Resume file: None
