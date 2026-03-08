---
phase: 14-core-parameter-model
plan: 02
subsystem: testing
tags: [dotnet-template, test-script, parameter-verification, isEnabled, allowMultipleValues]

# Dependency graph
requires:
  - phase: 14-core-parameter-model
    plan: 01
    provides: Complete parameter surface in template.json with 18 parameters, dotnetcli.host.json with isEnabled gating annotations and camelCase-to-kebab mappings
affects:
  - 15-architecture-and-project-types
  - 16-data-access-and-auth
  - 17-logging-testing-quality
  - 18-devops-containers-background-jobs

provides:
  - Extended test-template.sh with 19-step verification suite (was 12 steps)
  - Step 13: verify all 18 user-facing params in --help, no IncludeX computed symbols leaked
  - Steps 14-16: verify isEnabled 'Enabled if:' gating annotations for db/auth/api-docs
  - Step 17: verify allowMultipleValues via repeated --testing flags and 'Multiple values are allowed' annotation
  - Step 18: verify valid gated combination generation (WebApi+Jwt+EfCore+PostgreSql)
  - Step 19: verify Console project type generates and builds successfully

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Use 'Enabled if:' annotation presence in --help to verify isEnabled gating (not CLI rejection, which is advisory in .NET 10)"
    - "Use repeated flags for multi-value parameter testing: --testing xunit --testing fluentassertions"
    - "Use start-of-line anchor in grep for parameter section: grep -A5 '^  -db ' to avoid matching param references inside other descriptions"

key-files:
  created: []
  modified:
    - scripts/test-template.sh

key-decisions:
  - "Gating tests (steps 14-16) verify 'Enabled if:' annotation presence in --help rather than expecting CLI rejection — .NET 10 isEnabled is advisory and silently uses defaultValue when disabled"
  - "Multi-value test (step 17) uses repeated flags (--testing xunit --testing fluentassertions), not comma-separated syntax which is unsupported in .NET 10"
  - "grep -A5 '^  -db ' with start-of-line anchor prevents false matches from --db references inside other parameter descriptions"

patterns-established:
  - "Test script regression guard pattern: each phase adds its own numbered steps to test-template.sh"
  - "Gating verification pattern: grep for 'Enabled if' annotation rather than testing CLI rejection"
  - "Parameter help check pattern: capture HELP_OUTPUT once, run multiple grep assertions against it"

requirements-completed:
  - PARAM-01
  - PARAM-02
  - PARAM-03
  - PARAM-04
  - PARAM-05
  - PARAM-06
  - PARAM-07
  - PARAM-08
  - PARAM-09
  - PARAM-10
  - PARAM-11
  - PARAM-12
  - PARAM-13

# Metrics
duration: 4min
completed: 2026-03-08
---

# Phase 14 Plan 02: Parameter Model Test Script Summary

**Extended test-template.sh from 12 to 19 steps: --help parameter presence, isEnabled gating annotation verification, multi-value repeated-flag testing, valid gated combination, and Console project build**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-08T04:26:54Z
- **Completed:** 2026-03-08T04:30:48Z
- **Tasks:** 1
- **Files modified:** 1 (scripts/test-template.sh)

## Accomplishments

- Extended test-template.sh with 7 new Phase 14 verification steps (steps 13-19)
- Step 13 confirms all 18 user-facing parameters appear in `--help` and no computed IncludeX symbols leak through dotnetcli.host.json's isHidden mechanism
- Steps 14-16 confirm isEnabled gating constraints are communicated via 'Enabled if:' annotations in --help for db/auth/api-docs
- Step 17 confirms allowMultipleValues works via repeated flags and --help shows 'Multiple values are allowed: True'
- Step 18 confirms valid gated combinations (WebApi+Jwt+EfCore+PostgreSql) generate successfully
- Step 19 confirms Console project type generates a buildable project
- Script verified idempotent — passes on consecutive runs

## Task Commits

Each task was committed atomically:

1. **Task 1: Extend test-template.sh with Phase 14 parameter model verification** - `67e991b` (feat)

**Plan metadata:** (committed with docs commit)

## Files Created/Modified

- `scripts/test-template.sh` - Extended from 12-step to 19-step verification suite with Phase 14 parameter model checks

## Decisions Made

- **Gating test approach:** Gating tests (steps 14-16) verify the 'Enabled if:' annotation appears in `--help` output rather than asserting CLI rejection. This correctly models .NET 10's advisory isEnabled behavior discovered in Plan 01.
- **Multi-value test approach:** Step 17 uses `--testing xunit --testing fluentassertions --testing nsubstitute` (repeated flags) and also checks that --help shows "Multiple values are allowed: True", both confirming allowMultipleValues behavior.
- **grep precision:** Used `grep -A5 '^  -db '` (start-of-line anchor) instead of `grep -A3 '\-\-db'` to avoid false matches from --db references inside other parameter descriptions (e.g., Hangfire description mentions --db).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed imprecise grep pattern for gating annotation detection**
- **Found during:** Task 1 — first test run
- **Issue:** Plan's proposed pattern `grep -A3 "\-\-db"` matched `--db` references inside other descriptions (Hangfire description contains `--orm efcore|dapper + --db`), causing the grep to miss the actual `Enabled if:` line for the `-db` parameter
- **Fix:** Changed to `grep -A5 "^  -db "` with start-of-line anchor — precisely matches the `-db <choice>` parameter definition line, not embedded references
- **Files modified:** scripts/test-template.sh
- **Verification:** Step 14 PASS verified on first successful run after fix
- **Committed in:** 67e991b (Task 1 commit, inline fix)

---

**Total deviations:** 1 auto-fixed (grep precision bug)
**Impact on plan:** Minimal — one-line fix to grep pattern. All tests now pass. No scope creep.

## Issues Encountered

- **grep pattern false match:** `grep -A3 "\-\-db"` hit a `--db` occurrence inside the backgroundJobs description, causing wrong context lines to be checked for "Enabled if". Fixed with start-of-line anchor. No impact on final result.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Phase 15 (Architecture and Project Types) can begin immediately
- The test script regression guard is in place — any Phase 15 changes that break template generation or the parameter surface will be caught by test-template.sh
- The `--param:type` flag quirk (type clashes with dotnet CLI's built-in --type filter) is handled in the test: steps 18-19 use `--param:type` for the type parameter
- No blockers for subsequent phases

## Self-Check: PASSED

- scripts/test-template.sh: FOUND (modified, 115 insertions)
- Commit 67e991b: FOUND
- 14-02-SUMMARY.md: (this file, being written now)

---
*Phase: 14-core-parameter-model*
*Completed: 2026-03-08*
