# Project Research Summary

**Project:** NetStarter — Blazor WASM .NET Project Generator
**Domain:** Browser-based static project scaffolding tool (.NET ecosystem)
**Researched:** 2026-02-26
**Confidence:** MEDIUM (architectural decisions HIGH; library versions MEDIUM — NuGet verification required)

## Executive Summary

NetStarter is a browser-only .NET project scaffolding tool in the tradition of Spring Initializr — the user selects options, clicks Generate, and receives a zip of production-ready project files. The key constraint is zero backend: all generation happens inside Blazor WebAssembly, output is delivered as a browser download, and the tool deploys as static files to GitHub Pages or Azure Static Web Apps at zero cost. This architecture is well-proven but has several WASM-specific pitfalls that must be addressed from the very first commit.

The recommended approach is: MudBlazor for the UI (dark theme first, Material Design, no external CSS pipeline), `System.IO.Compression.ZipArchive` from the BCL for in-browser zip generation (no third-party library), and plain C# string interpolation with a composable template class pattern for code generation (no template engine in v1). The generation engine should be modeled as a pure function `Generate(ProjectConfig) -> List<GeneratedFile>` where `ProjectConfig` is a single C# record that drives all logic. The biggest differentiator over `dotnet new` is architecture pattern selection (Clean Architecture, Vertical Slice Architecture, Simple Layered) — this is the #1 feature the .NET ecosystem is missing and the feature that justifies NetStarter's existence.

The critical risks are: combinatorial template explosion if the generation engine is not designed compositionally from the start, generated code that compiles but violates the chosen architecture pattern (reputation-damaging), and GitHub Pages deployment issues (routing 404, missing `.nojekyll`, EOL normalization of JS files). All three must be addressed before any public release. The correct mitigation for the template problem is a layered composition model (~16–20 template classes, not 36+ full combination templates), validated by a CI smoke-test matrix that generates and `dotnet build`s every supported SDK version.

## Key Findings

### Recommended Stack

The project already targets .NET 10 with `Microsoft.AspNetCore.Components.WebAssembly` 10.0.2. The only NuGet addition required for v1 is MudBlazor (7.x) for the UI. Everything else — zip creation, template generation, JS interop for file download — uses BCL types and Blazor built-ins. This minimal dependency footprint is deliberate: every third-party NuGet package added to a Blazor WASM app risks bundle size growth and trimmer incompatibility.

Deployment targets GitHub Pages as primary (with a well-documented routing workaround) and Azure Static Web Apps as secondary. Both are free-tier static hosts that serve Blazor WASM without server-side configuration.

**Core technologies:**
- **.NET 10 + Blazor WASM**: Runtime and SPA framework — already in use, zero-backend constraint
- **MudBlazor 7.x**: Full UI component library — best dark-theme support, Material Design, Blazor-native (not a JS wrapper)
- **System.IO.Compression (BCL)**: In-browser zip creation — WASM-safe, trimmer-friendly, no external dependency
- **C# string interpolation + StringBuilder**: Template generation — no runtime engine dependency, full C# control, unit-testable
- **GitHub Pages**: Primary deployment — zero cost, static file hosting, direct repo integration

### Expected Features

The feature set is anchored by Spring Initializr as the gold-standard UX reference. See `FEATURES.md` for the full feature dependency tree.

**Must have (table stakes):**
- Project name, namespace, SDK version selection — foundational metadata
- Project type selection (Web API, Minimal API, Console, Worker Service) — core identity
- Architecture pattern selection (Clean, Vertical Slice, Simple Layered) — #1 differentiator
- ZIP download of working generated code — the entire value delivery mechanism
- Generated code must compile on first `dotnet build` — the primary trust signal
- `.gitignore`, `.editorconfig`, `.sln`, correct `.csproj` — professional scaffolding baseline
- Dark theme UI — developer tool expectation; required per project spec
- Conditional option visibility (database picker only when ORM selected, etc.)

**Should have (competitive differentiators):**
- ORM + database wiring (EF Core + PostgreSQL/SQL Server) — dotnet new does not do this
- JWT auth scaffolding wired into Program.cs — commonly forgotten boilerplate
- Testing project (xUnit + FluentAssertions + Testcontainers) — most scaffolders omit this
- Container support (Dockerfile, docker-compose, .NET Aspire) — modern .NET norm
- Observability setup (Serilog, Health Checks) — production-readiness signal
- GitHub Actions CI scaffold — low complexity, high value
- CLI equivalent display showing dotnet commands — unique to NetStarter

**Defer (v2+):**
- Shareable configuration URL (URL-encoded preset sharing)
- Project file tree preview panel
- Push to GitHub repo
- Blazor, gRPC as project types
- OAuth/Keycloak auth, Azure DevOps CI, NUnit/MSTest, Dapper/SQLite, full OpenTelemetry
- dotnet new NuGet template package distribution

### Architecture Approach

The entire app is a single-page Blazor WASM application with no server component. The architecture separates cleanly into three layers: the UI layer (Blazor components using MudBlazor), a state/config model layer (`ProjectConfig` record as the single source of truth), and the generation engine (pure C# classes with no Blazor dependency). The generation engine uses a composable layered model — base project type templates + architecture overlays + additive option layers — yielding approximately 16–20 template classes rather than an exponentially growing set of full-combination templates. Output flows: `ProjectConfig` → `ProjectGenerator` → `List<GeneratedFile>` → `ZipPackager` → `DownloadService` (JS interop) → browser download.

**Major components:**
1. **ConfigForm (Blazor)** — captures all user selections; two-way bound to `ProjectConfig`
2. **ProjectConfig (C# record)** — single source of truth; drives all generation and preview logic; includes `Validate()` method
3. **ProjectGenerator** — orchestrates file generation; calls `IFileTemplate.AppliesTo(config)` to select active templates
4. **File Templates (Generation/Templates/)** — ~16-20 composable classes; base types + architecture overlays + additive layers (EF Core, JWT, Serilog, Docker, CI)
5. **ProgramCsComposer** — section-based composition of Program.cs to avoid monolithic if/else chains
6. **ZipPackager** — wraps `System.IO.Compression.ZipArchive`; prefixes all paths with `{ProjectName}/`
7. **DownloadService** — thin JS interop wrapper; triggers browser blob download
8. **CliCommandBuilder** — maps `ProjectConfig` to equivalent `dotnet` CLI commands

### Critical Pitfalls

1. **Combinatorial template explosion** — Design the generation engine as composable layers (`IFileTemplate.AppliesTo`) before adding the second project type or second architecture. A monolithic if/else approach becomes unmaintainable at 5+ options. Address in Phase 1.

2. **Generated code that violates its own architecture** — Build a hand-written reference project per architecture (Clean, Vertical Slice) that compiles and passes tests before templating it. Especially important for Clean Architecture where domain-layer dependency violations are easy to introduce silently. Address before shipping each architecture template.

3. **GitHub Pages deployment failures** — Three independent issues must all be fixed before first deploy: (a) SPA routing 404 requires `404.html` redirect + correct `<base href>`; (b) `_framework/` assets 404 requires `.nojekyll` file; (c) JS file integrity check failures require `*.js binary` in `.gitattributes` before the first commit of WASM files.

4. **WASM bundle size bloat** — Every NuGet package added to the generator app (not the generated projects) must be trimmer-friendly. Measure `_framework/` folder size on every PR. The BCL-only approach (no third-party zip, no template engine) is the primary mitigation.

5. **Missing input validation** — Project name must be validated against C# identifier rules and a reserved-namespace blocklist (`System`, `Microsoft`, etc.) before generation. Invalid combinations (e.g., JWT auth on a Console project) must be caught by `ProjectConfig.Validate()` and enforced in both UI and generation engine.

## Implications for Roadmap

Based on combined research, the following phase structure is recommended. The ARCHITECTURE.md "Build Order" section corroborates this sequencing independently.

### Phase 1: Foundation and Generation Engine Core

**Rationale:** The generation engine's composable architecture must be established before any templates are written. Getting the `IFileTemplate` pattern and `ProjectConfig` record right prevents the combinatorial explosion pitfall. GitHub Pages deployment issues must also be resolved before any other code ships — they are setup-once and catastrophic if missed.

**Delivers:** Deployable app shell (dark theme, MudBlazor layout), working generation engine with one complete combination (Web API + Simple Layered + no optional features), downloadable zip that compiles, GitHub Pages CI/CD pipeline.

**Addresses:** Project metadata input, SDK version selection, single project type (Web API), Simple Layered architecture, ZIP download, `.gitignore`/`.editorconfig`/`.sln` scaffolding.

**Avoids:** Pitfall 1 (template explosion — composable engine from day 1), Pitfall 3 (bundle bloat — BCL-only from day 1), Pitfall 4 (GitHub Pages routing), Pitfall 5 (git EOL integrity), Pitfall 11 (.nojekyll), Pitfall 12/13 (zip path prefix and dispose order).

**No deeper research needed** — patterns are well-established (Blazor WASM, GitHub Pages, BCL zip, C# template classes).

### Phase 2: Project Type Coverage and Architecture Patterns

**Rationale:** Once the engine is proven with one combination, add project types (Minimal API, Console, Worker Service) and architecture patterns (Clean Architecture, Vertical Slice). These must be validated against hand-written reference projects before templating.

**Delivers:** All 4 project types generating compilable output; Clean Architecture and Vertical Slice architecture overlays generating correct multi-project solutions.

**Addresses:** Minimal API, Console, Worker Service project types; Clean Architecture (4-project solution); Vertical Slice Architecture (features-folder structure).

**Avoids:** Pitfall 2 (architecture correctness — reference project first), Pitfall 6 (version drift — CI smoke test matrix per SDK version added here), Pitfall 8 (invalid combinations — `Validate()` method enforced).

**Research flag — Phase 2 likely needs research-phase:** Clean Architecture template correctness and Vertical Slice Architecture conventions are opinionated. Verify against community-accepted reference implementations (ardalis/CleanArchitecture, jbogard/vertical-slice-architecture) before templating.

### Phase 3: Additive Options (ORM, Auth, Observability, Testing)

**Rationale:** Additive options (EF Core, JWT, Serilog, Health Checks, xUnit) are compositional layers that layer onto whatever base project exists. They should be built after base types and architectures are stable so each overlay can be validated against all base combinations.

**Delivers:** EF Core wired with PostgreSQL and SQL Server connection strings; JWT bearer auth configured in Program.cs; Serilog with console and file sink; Health Checks endpoint; xUnit + FluentAssertions + Testcontainers test project.

**Addresses:** ORM + database selection, auth selection (None/JWT), observability setup, testing project generation.

**Avoids:** Pitfall 1 (each option is an independent `IFileTemplate`, not a branch in existing templates), Pitfall 8 (database picker only visible when ORM is EF Core; JWT only valid for Web API/Minimal API).

**Research flag — Phase 3 needs research-phase for EF Core + multi-SDK:** NuGet package versions differ per target SDK (EF Core 8.x for .NET 8, EF Core 9.x for .NET 9, EF Core 10.x for .NET 10). The version matrix must be verified against NuGet before implementing.

### Phase 4: Container and CI/CD Options

**Rationale:** Container and CI/CD options (Dockerfile, docker-compose, .NET Aspire, GitHub Actions) are independent of the ORM/auth options and can be built and shipped in parallel with Phase 3 or after it. They are additive overlays with minimal interaction with other options.

**Delivers:** Multi-stage Dockerfile for .NET; docker-compose with database service; .NET Aspire AppHost wired; GitHub Actions `.github/workflows/dotnet.yml` with build + test steps.

**Addresses:** Container support, CI/CD scaffold, .NET Aspire support.

**Avoids:** Pitfall 6 (version drift for Aspire — verify Aspire package versions per SDK).

**No deeper research needed** for Docker and GitHub Actions — these are standard patterns. .NET Aspire may warrant a research-phase pass due to its relative novelty (2024+).

### Phase 5: Polish, UX, and CLI Output

**Rationale:** CLI equivalent display, loading UX, accessibility checks, and service worker handling are polish items that improve trust and DX without blocking core functionality.

**Delivers:** CLI equivalent display showing `dotnet new` and `dotnet add` commands; custom loading spinner matching the dark theme; WCAG AA contrast validation; service worker cache versioning or removal.

**Addresses:** CLI equivalent display feature, cold-start UX, accessibility.

**Avoids:** Pitfall 9 (cold-start UX — custom loading indicator), Pitfall 14 (service worker cache), Pitfall 15 (dark theme contrast ratios).

**No deeper research needed** — standard Blazor WASM loading customization and accessibility patterns.

### Phase Ordering Rationale

- Phase 1 must establish the composable engine architecture before ANY templates are added — this is non-negotiable per Pitfall 1.
- Phase 2 must precede Phase 3 because additive options compose onto base project types and architectures; those bases must exist and be validated first.
- Phase 3 and Phase 4 are partially parallelizable — Docker/CI options are independent of ORM/Auth options.
- Phase 5 is genuinely last — polish on a foundation that is feature-complete.
- The CI smoke-test matrix (generate + `dotnet build` per SDK version) should be established in Phase 2 and extended in Phase 3 and 4.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 2:** Architecture template correctness — verify Clean Architecture and Vertical Slice reference implementations against community-accepted patterns before templating
- **Phase 3:** EF Core + multi-SDK version matrix — NuGet package versions differ per target SDK; verify before implementation
- **Phase 4 (.NET Aspire only):** Aspire AppHost integration patterns are relatively new (2024+); verify against current Aspire documentation

Phases with standard patterns (skip research-phase):
- **Phase 1:** Blazor WASM setup, GitHub Pages deployment, BCL zip — well-documented, established patterns
- **Phase 4 (Docker/CI only):** Multi-stage Dockerfile and GitHub Actions for .NET are thoroughly documented
- **Phase 5:** Loading UX, accessibility, service worker — standard web patterns

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | MEDIUM | Architectural decisions (BCL zip, C# templates, GitHub Pages) are HIGH. MudBlazor version and .NET 10 GA status need NuGet verification. |
| Features | MEDIUM | Feature categorization based on direct knowledge of Spring Initializr, JHipster, dotnet new — LOW staleness risk. WebSearch unavailable; no live source verification. |
| Architecture | HIGH | Architectural decisions are independent of library versions. Composable template pattern is sound and well-reasoned. Data flow and component breakdown are clear. |
| Pitfalls | HIGH | GitHub Pages pitfalls sourced from official Microsoft documentation (2026-02-24). Template and WASM pitfalls corroborated by multiple community patterns. |

**Overall confidence:** MEDIUM-HIGH

### Gaps to Address

- **MudBlazor 7.x NuGet compatibility with .NET 10:** Verify MudBlazor 7.x supports .NET 10 / ASP.NET Core 10 before Phase 1 begins. If not, evaluate MudBlazor 8.x or Fluent UI Blazor as fallback.
- **EF Core version matrix per SDK:** Before Phase 3, establish the EF Core package version for each of .NET 8, 9, and 10. Document as a lookup table in the generator.
- **Clean Architecture reference project:** Build or validate a hand-written Clean Architecture reference project before Phase 2 templating. The template must enforce zero-Infrastructure-reference in the Domain project.
- **Vertical Slice Architecture conventions:** The .NET community has not standardized VSA folder structure. Choose one reference (jbogard/vertical-slice-architecture or equivalent) and document it as the NetStarter interpretation before Phase 2.
- **.NET 10 GA status:** .NET 10 was RC at training cutoff (August 2025). Verify GA release date and whether 10.0.2 packages are stable before Phase 1 begins.

## Sources

### Primary (HIGH confidence)
- Microsoft Docs: Host and deploy Blazor WASM — GitHub Pages (2026-02-24) — routing, `.nojekyll`, base href
- Microsoft Docs: Host and deploy Blazor WASM standalone — compression, webcil, routing
- Microsoft Docs: Blazor performance best practices — bundle size, trimming
- Existing project file `NetStarter/NetStarter/NetStarter.csproj` — confirmed .NET 10, 10.0.2 packages
- `System.IO.Compression.ZipArchive` BCL — WASM-safe since .NET 5, well-established

### Secondary (MEDIUM confidence)
- Training data: MudBlazor 7.x ecosystem, theming API, dark mode support
- Training data: Spring Initializr feature set and UX patterns (direct observation)
- Training data: JHipster feature set — architecture choices, auth integration
- Training data: `dotnet new` official documentation and known limitations
- Training data: Clean Architecture and Vertical Slice Architecture conventions in .NET community
- Training data: .NET Aspire AppHost integration patterns (2024+ feature)

### Tertiary (LOW confidence)
- Template engine pattern rationale (Scriban v2 fallback) — inference from community patterns, not benchmarked
- .NET 10 GA release timeline — RC at training cutoff; needs live verification

---
*Research completed: 2026-02-26*
*Ready for roadmap: yes*
