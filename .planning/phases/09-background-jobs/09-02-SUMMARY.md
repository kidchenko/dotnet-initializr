---
phase: 09-background-jobs
plan: 02
subsystem: ui
tags: [blazor, razor-components, background-jobs, hangfire, quartz, ihostedservice, xunit]

# Dependency graph
requires:
  - phase: 09-background-jobs/09-01
    provides: BackgroundJobsGenerator, BackgroundJobsOption enum, CsprojGenerator packages, ProgramCsGenerator fragments, FileTreeService Jobs/ folder, ProjectGenerationService wiring

provides:
  - Background Jobs radio group section in ConfigurationForm.razor (Section 4e)
  - Conditional Hangfire option (visible only when database is selected)
  - Console project type hides Background Jobs section entirely
  - OnProjectTypeChanged resets BackgroundJobs to None when switching to Console
  - OnOrmChanged resets BackgroundJobs to None when database deselected and Hangfire active
  - Phase09BackgroundJobsTests.cs with 36 test methods (52 test cases) covering JOBS-01 through JOBS-06

affects:
  - Phase 10 (URL persistence) - BackgroundJobs must be serialized to URL param
  - Phase 11 (Responsive) - Background Jobs section will need responsive styling

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Conditional radio option: @if (Config.Database.HasValue) { <Radio ... /> } inside RadioGroup"
    - "Reset logic in OnProjectTypeChanged for non-web project types"
    - "Reset logic in OnOrmChanged for Hangfire dependency on database"

key-files:
  created:
    - tests/NetStarter.Tests/Phase09BackgroundJobsTests.cs
  modified:
    - src/NetStarter/Components/ConfigurationForm.razor

key-decisions:
  - "Background Jobs section visible for WebApi, MinimalApi, and WorkerService — not Console (broader than OpenAPI which is web-only)"
  - "Hangfire radio option conditionally shown only when Config.Database.HasValue — matches backend generator guard"
  - "Reset in OnOrmChanged checks Config.BackgroundJobs == Hangfire AND Config.Database is null (after ORM change nulls DB)"

patterns-established:
  - "FindNode recursive helper for searching FileTreeNode tree in tests"
  - "CreateConfig helper accepting BackgroundJobsOption, ProjectType, DotNetSdkVersion, DatabaseOption?, ArchitecturePattern for test configuration"

requirements-completed: [JOBS-01, JOBS-02, JOBS-03, JOBS-04, JOBS-05, JOBS-06]

# Metrics
duration: 2min
completed: 2026-03-01
---

# Phase 9 Plan 02: Background Jobs Summary

**Background Jobs radio group with Hangfire conditional visibility and Console/ORM reset logic, plus 52 automated tests covering all six JOBS requirements (JOBS-01 through JOBS-06)**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-01T06:13:57Z
- **Completed:** 2026-03-01T06:16:32Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments

- Background Jobs Section 4e added to ConfigurationForm.razor after OpenAPI, before Quality — visible for WebApi, MinimalApi, WorkerService; hidden for Console
- Hangfire radio option conditionally rendered only when `Config.Database.HasValue` — prevents invalid Hangfire+NoDatabase combinations in UI
- Two reset guards added: OnProjectTypeChanged (Console switch) and OnOrmChanged (database deselect with Hangfire active)
- 36 test methods (52 test cases) in Phase09BackgroundJobsTests.cs covering JOBS-01 through JOBS-06 comprehensively
- Total test suite: 206 tests, 0 failures (154 existing + 52 new)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add Background Jobs radio group and reset logic to ConfigurationForm.razor** - `d09dfd0` (feat)
2. **Task 2: Write comprehensive Phase 9 automated tests** - `2f22032` (feat)

## Files Created/Modified

- `src/NetStarter/Components/ConfigurationForm.razor` - Added Section 4e Background Jobs radio group with conditional Hangfire option; added reset logic to OnProjectTypeChanged and OnOrmChanged
- `tests/NetStarter.Tests/Phase09BackgroundJobsTests.cs` - 36 test methods covering all six JOBS requirements with [Fact] and [Theory] patterns

## Decisions Made

- Background Jobs section visible for WebApi, MinimalApi, AND WorkerService (broader than OpenAPI which is web-only) — WorkerService is a primary background jobs host
- Hangfire conditional inside RadioGroup uses `@if (Config.Database.HasValue)` consistent with the backend CsprojGenerator/ProgramCsGenerator guard (`config.Database.HasValue`)
- Reset guard in `OnOrmChanged` checks `Config.BackgroundJobs == Hangfire && Config.Database is null` — the `Database` is already set to null by the preceding ORM change logic before this check runs

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All six JOBS requirements (JOBS-01 through JOBS-06) covered by automated tests
- Background Jobs UI complete and wired to backend generators from Plan 01
- Phase 10 (URL persistence) can proceed — BackgroundJobs property needs to be serialized to URL param

---
*Phase: 09-background-jobs*
*Completed: 2026-03-01*
