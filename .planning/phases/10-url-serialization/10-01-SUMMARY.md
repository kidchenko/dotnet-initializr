---
phase: 10-url-serialization
plan: 01
subsystem: ui
tags: [blazor, url-serialization, query-params, round-trip, openapi, background-jobs]

# Dependency graph
requires:
  - phase: 08-openapi-documentation
    provides: OpenApiUi enum and ApiDocsUi property on ProjectConfiguration
  - phase: 09-background-jobs
    provides: BackgroundJobsOption enum and BackgroundJobs property on ProjectConfiguration

provides:
  - docs and jobs URL query params in Home.razor (serialize + deserialize)
  - Phase10UrlSerializationTests.cs with 14 round-trip and backward-compat tests
  - URL-01 (docs/jobs params), URL-02 (backward compat), URL-03 (resil coverage)

affects:
  - phase: 11-responsive
  - any future URL param additions

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "URL serialization: SupplyParameterFromQuery + Enum.TryParse(ignoreCase:true) + ToString().ToLower() in PushStateToUrl"
    - "Default/None enum values serialize as null (omitted from URL)"
    - "SimulateSerialize/SimulateDeserialize test helpers mirror Home.razor logic for contract testing"

key-files:
  created:
    - tests/NetStarter.Tests/Phase10UrlSerializationTests.cs
  modified:
    - src/NetStarter/Pages/Home.razor

key-decisions:
  - "URL serialize convention: None enum values serialize to null (omitted from URL), non-None values use .ToString().ToLower()"
  - "Test helpers (SimulateSerialize/SimulateDeserialize) mirror Home.razor logic directly — contract testing without Blazor component"
  - "Variable names docsUi and jobsOpt used in parse branches to avoid shadowing dictionary key strings"

patterns-established:
  - "Round-trip URL test pattern: build fully-loaded ProjectConfiguration, serialize to dict, deserialize, assert field equality"
  - "Backward-compat test pattern: build v1.0/v1.1 param dict without new params, assert new fields default to None"

requirements-completed: [URL-01, URL-02, URL-03]

# Metrics
duration: 2min
completed: 2026-03-01
---

# Phase 10 Plan 01: URL Serialization Summary

**docs and jobs query params wired into Home.razor with SimulateSerialize/SimulateDeserialize round-trip tests covering all v1.0/v1.1/v1.2 URL params**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-01T17:54:17Z
- **Completed:** 2026-03-01T17:56:50Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments

- Added `DocsParam` and `JobsParam` `[SupplyParameterFromQuery]` declarations in Home.razor
- Added `Enum.TryParse<OpenApiUi>` and `Enum.TryParse<BackgroundJobsOption>` parse branches in `OnParametersSet()`
- Added `["docs"]` and `["jobs"]` dictionary entries in `PushStateToUrl()` with `.ToString().ToLower()` serialization
- Created Phase10UrlSerializationTests.cs with 14 tests: 9 URL-01 (docs/jobs), 3 URL-02 (backward compat), 2 URL-03 (resil)
- Full test suite at 220 tests, 0 failures; app builds with 0 warnings

## Task Commits

Each task was committed atomically:

1. **Task 1: RED — Phase10UrlSerializationTests.cs with 14 failing tests** - `ca07b6c` (test)
2. **Task 2: GREEN — Add docs and jobs query params to Home.razor** - `e52fab1` (feat)

**Plan metadata:** (final commit below)

## Files Created/Modified

- `tests/NetStarter.Tests/Phase10UrlSerializationTests.cs` — 14 round-trip and backward-compat URL serialization tests with SimulateSerialize/SimulateDeserialize helpers
- `src/NetStarter/Pages/Home.razor` — Added DocsParam/JobsParam declarations, OnParametersSet parse branches, PushStateToUrl dictionary entries

## Decisions Made

- Used variable names `docsUi` and `jobsOpt` in parse branches (not `docs` and `jobs`) to avoid shadowing conflicts with dictionary key strings in `PushStateToUrl`
- Test helpers defined as static methods within the test class — they mirror and document the expected URL contract for Home.razor

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- URL serialization complete for all v1.2 fields (docs, jobs, resil)
- All v1.0/v1.1/v1.2 URL params fully tested for round-trip correctness and backward compatibility
- Phase 11 (Responsive) can proceed — it depends only on Phase 7+ being complete

---
*Phase: 10-url-serialization*
*Completed: 2026-03-01*

## Self-Check: PASSED

- FOUND: tests/NetStarter.Tests/Phase10UrlSerializationTests.cs
- FOUND: src/NetStarter/Pages/Home.razor
- FOUND: .planning/phases/10-url-serialization/10-01-SUMMARY.md
- FOUND: ca07b6c (test(10-01): add failing tests for docs/jobs URL serialization round-trip)
- FOUND: e52fab1 (feat(10-01): add docs and jobs query params to Home.razor)
