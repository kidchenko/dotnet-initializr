---
phase: 17-logging-testing-and-quality
plan: "03"
subsystem: testing
tags: [xunit, fluentassertions, nsubstitute, bogus, testcontainers, dotnet-templates]

# Dependency graph
requires:
  - phase: 17-01
    provides: template.json sources entries for tests-single/ and tests-split/ directories + IncludeAnyTesting/IncludeTestcontainers computed symbols

provides:
  - tests-single/Company.ProjectName.Tests/ directory with .csproj and SampleTests.cs (non-Testcontainers path)
  - tests-split/Company.ProjectName.UnitTests/ directory with .csproj and SampleTests.cs (Testcontainers path, unit tests)
  - tests-split/Company.ProjectName.IntegrationTests/ directory with .csproj and SampleIntegrationTests.cs (Testcontainers path, integration tests)
  - Complete test project scaffold for all testing library combinations

affects: [17-04, 18-logging-testing-and-quality, test-template.sh verification steps for testing parameters]

# Tech tracking
tech-stack:
  added:
    - xunit 2.* (test framework, always included in test projects)
    - xunit.runner.visualstudio 2.* (VS test runner adapter, pinned to avoid .NET 10 issues with v3)
    - Microsoft.NET.Test.Sdk 17.* (test SDK base)
    - FluentAssertions 7.* (assertions library, conditional, pinned to 7.* to avoid commercial v8)
    - NSubstitute 5.* (mocking framework, conditional)
    - Bogus 35.* (fake data generator, conditional)
    - Testcontainers 4.* (container-based integration testing, conditional)
    - Testcontainers.PostgreSql 4.* (PostgreSQL container, conditional on DB selection)
    - Testcontainers.MsSql 4.* (SQL Server container, conditional on DB selection)
    - Testcontainers.MySql 4.* (MySQL container, conditional on DB selection)
  patterns:
    - Template conditional directives (<!--#if --> in .csproj, #if in .cs) for optional package inclusion
    - IAsyncLifetime pattern for async container lifecycle management in xUnit
    - Split test project pattern: UnitTests + IntegrationTests when Testcontainers selected

key-files:
  created:
    - templates/dotnet-initializr/tests-single/Company.ProjectName.Tests/Company.ProjectName.Tests.csproj
    - templates/dotnet-initializr/tests-single/Company.ProjectName.Tests/SampleTests.cs
    - templates/dotnet-initializr/tests-split/Company.ProjectName.UnitTests/Company.ProjectName.UnitTests.csproj
    - templates/dotnet-initializr/tests-split/Company.ProjectName.UnitTests/SampleTests.cs
    - templates/dotnet-initializr/tests-split/Company.ProjectName.IntegrationTests/Company.ProjectName.IntegrationTests.csproj
    - templates/dotnet-initializr/tests-split/Company.ProjectName.IntegrationTests/SampleIntegrationTests.cs
  modified: []

key-decisions:
  - "FluentAssertions pinned to 7.* (v8 is commercial/paid) — applies to all test projects"
  - "xunit.runner.visualstudio pinned to 2.* (v3.x has known .NET 10 compatibility issues)"
  - "xunit + Microsoft.NET.Test.Sdk are always included unconditionally as the test runner base"
  - "IntegrationTests references Infrastructure (CleanArch) or main project (single-project) — not Application layer — for data access testing"
  - "SampleIntegrationTests.cs uses IAsyncLifetime for proper async container start/stop lifecycle"
  - "DB-specific Testcontainers packages (PostgreSql/MsSql/MySql) are conditional on ORM DB selection"
  - "NSubstitute and Bogus are not included in IntegrationTests — unit test concerns only"
  - "ProjectReference uses ../../src/ relative path because template maps tests-single|tests-split to tests/"

patterns-established:
  - "Split test projects pattern: IncludeTestcontainers flag triggers UnitTests + IntegrationTests split instead of single Tests project"
  - "Template conditional in .csproj uses XML comment style: <!--#if (Symbol) --> / <!--#endif -->"
  - "Conditional package references in test .csproj mirror the testing parameter choices from template.json"
  - "SampleIntegrationTests.cs: DB-agnostic fallback with Assert.True(true) smoke test when no DB container configured"

requirements-completed: [PARAM-08]

# Metrics
duration: 2min
completed: 2026-03-08
---

# Phase 17 Plan 03: Test Project Directory Structure Summary

**xUnit + FluentAssertions 7.* + NSubstitute + Bogus + Testcontainers test scaffold with single and split project variants for the dotnet-initializr template**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-08T23:24:16Z
- **Completed:** 2026-03-08T23:26:06Z
- **Tasks:** 2
- **Files modified:** 6 (all created)

## Accomplishments

- Created tests-single/Company.ProjectName.Tests/ with conditional .csproj and SampleTests.cs demonstrating all optional libraries
- Created tests-split/Company.ProjectName.UnitTests/ (same sample as single, different namespace)
- Created tests-split/Company.ProjectName.IntegrationTests/ with Testcontainers lifecycle and DB-specific container selection
- All FluentAssertions references pinned to 7.* (commercial v8 avoided), xunit.runner.visualstudio pinned to 2.*
- Integration test project correctly references Infrastructure (not Application) for CleanArchitecture

## Task Commits

Each task was committed atomically:

1. **Task 1: Create single test project files (tests-single/)** - `fc5ae65` (feat)
2. **Task 2: Create split test project files (tests-split/)** - `73a87ad` (feat)

**Plan metadata:** TBD (docs: complete plan)

## Files Created/Modified

- `templates/dotnet-initializr/tests-single/Company.ProjectName.Tests/Company.ProjectName.Tests.csproj` - Single test project with xunit 2.*, Microsoft.NET.Test.Sdk 17.*, and conditional FluentAssertions/NSubstitute/Bogus references
- `templates/dotnet-initializr/tests-single/Company.ProjectName.Tests/SampleTests.cs` - Sample test class with conditional use of all testing libraries
- `templates/dotnet-initializr/tests-split/Company.ProjectName.UnitTests/Company.ProjectName.UnitTests.csproj` - Unit test project for Testcontainers split (same packages as single project)
- `templates/dotnet-initializr/tests-split/Company.ProjectName.UnitTests/SampleTests.cs` - Same sample tests with Company.ProjectName.UnitTests namespace
- `templates/dotnet-initializr/tests-split/Company.ProjectName.IntegrationTests/Company.ProjectName.IntegrationTests.csproj` - Integration test project with Testcontainers 4.* and DB-specific container packages
- `templates/dotnet-initializr/tests-split/Company.ProjectName.IntegrationTests/SampleIntegrationTests.cs` - Integration test implementing IAsyncLifetime with DB container setup/teardown

## Decisions Made

- FluentAssertions pinned to `7.*` across all test projects — v8 is commercial, v7 is OSS
- `xunit.runner.visualstudio` pinned to `2.*` — v3.x has known .NET 10 compatibility issues
- xunit and Microsoft.NET.Test.Sdk are unconditional (always present as test runner base)
- IntegrationTests references `Company.ProjectName.Infrastructure` (CleanArch) not Application — data access testing requires Infrastructure layer
- `IAsyncLifetime` chosen over constructor/`[TestInitialize]` — xUnit's idiomatic async lifecycle pattern
- DB-specific Testcontainers packages conditioned on `IncludePostgreSql`, `IncludeSqlServer`, `IncludeMySql` — Sqlite excluded (no Testcontainers.Sqlite package exists)
- NSubstitute/Bogus excluded from IntegrationTests (unit test concerns); no FluentAssertions exclusion — it's useful in integration tests too

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Test project templates complete; template.json sources entries from Plan 01 now have backing directories
- When combined with Plan 01's IncludeAnyTesting source condition, `--testing xunit --testing fluentassertions` will produce `tests/Company.ProjectName.Tests/` in generated output
- `--testing xunit --testing testcontainers` will produce `tests/Company.ProjectName.UnitTests/` + `tests/Company.ProjectName.IntegrationTests/`
- Phase 17 Plan 04 (verification test steps) can now add test matrix coverage for all testing combinations

---
*Phase: 17-logging-testing-and-quality*
*Completed: 2026-03-08*

## Self-Check: PASSED

All files verified present on disk. Both task commits confirmed in git log.

| Check | Result |
|-------|--------|
| tests-single/Company.ProjectName.Tests/Company.ProjectName.Tests.csproj | FOUND |
| tests-single/Company.ProjectName.Tests/SampleTests.cs | FOUND |
| tests-split/Company.ProjectName.UnitTests/Company.ProjectName.UnitTests.csproj | FOUND |
| tests-split/Company.ProjectName.UnitTests/SampleTests.cs | FOUND |
| tests-split/Company.ProjectName.IntegrationTests/Company.ProjectName.IntegrationTests.csproj | FOUND |
| tests-split/Company.ProjectName.IntegrationTests/SampleIntegrationTests.cs | FOUND |
| 17-03-SUMMARY.md | FOUND |
| Commit fc5ae65 (Task 1) | FOUND |
| Commit 73a87ad (Task 2) | FOUND |
