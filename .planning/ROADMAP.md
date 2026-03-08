# Roadmap: NetStarter

## Milestones

- ✅ **v1.0 MVP** — Phases 1-3 (shipped 2026-02-27)
- ✅ **v1.1 Expanded Generator Options** — Phases 4-6.1 (shipped 2026-02-28)
- ✅ **v1.2 Infrastructure, UX & Polish** — Phases 7-12 (shipped 2026-03-07)
- 🚧 **v1.3 dotnet new Templates** — Phases 13-20 (in progress)

## Phases

<details>
<summary>✅ v1.0 MVP (Phases 1-3) — SHIPPED 2026-02-27</summary>

- [x] Phase 1: UI and Configuration Form (4/4 plans) — completed 2026-02-26
- [x] Phase 2: Generation Engine and Output (7/7 plans) — completed 2026-02-26
- [x] Phase 3: Deployment (2/2 plans) — completed 2026-02-27

</details>

<details>
<summary>✅ v1.1 Expanded Generator Options (Phases 4-6.1) — SHIPPED 2026-02-28</summary>

- [x] Phase 4: Foundation (4/4 plans) — completed 2026-02-27
- [x] Phase 5: Data Layer and Caching (3/3 plans) — completed 2026-02-28
- [x] Phase 6: Auth, Validation, and Testing (3/3 plans) — completed 2026-02-28
- [x] Phase 6.1: Validation and URL Persistence Fixes (1/1 plan) — completed 2026-02-28

</details>

<details>
<summary>✅ v1.2 Infrastructure, UX & Polish (Phases 7-12) — SHIPPED 2026-03-07</summary>

- [x] Phase 7: NLog and Polly (3/3 plans) — completed 2026-03-01
- [x] Phase 8: OpenAPI Documentation (2/2 plans) — completed 2026-03-01
- [x] Phase 9: Background Jobs (2/2 plans) — completed 2026-03-01
- [x] Phase 10: URL Serialization (1/1 plan) — completed 2026-03-01
- [x] Phase 11: Code Preview and Responsive Design (2/2 plans) — completed 2026-03-01
- [x] Phase 12: Tech Debt Cleanup (1/1 plan) — completed 2026-03-07

</details>

### 🚧 v1.3 dotnet new Templates (In Progress)

**Milestone Goal:** A `dotnet new install Initializr.Templates` NuGet package with full feature parity, distributed on NuGet.org and GitHub releases, with the Blazor app showing the equivalent `dotnet new dotnet-initializr` command.

- [x] **Phase 13: Template Foundation** - Buildable NuGet template pack that generates a minimal compilable .NET project (Plan 01 complete)
- [x] **Phase 14: Core Parameter Model** - Complete template.json with all parameters, computed symbols, and dependency chains (completed 2026-03-08)
- [ ] **Phase 15: Architecture and Project Types** - Correct folder structures and .slnx for all arch + type combinations
- [ ] **Phase 16: Data Access and Auth** - ORM, database, auth, and SDK-version-aware NuGet references
- [ ] **Phase 17: Logging, Testing, and Quality** - Logging choice, testing flags, and all boolean feature flags
- [ ] **Phase 18: DevOps, Containers, and Background Jobs** - CI/CD, container files, background jobs, and API docs via sources.modifiers
- [ ] **Phase 19: Blazor CLI Panel** - Blazor app displays `dotnet new dotnet-initializr --flags` command
- [ ] **Phase 20: NuGet Packaging and Distribution** - Build verification, NuGet.org publish, GitHub release artifact

## Phase Details

### Phase 7: NLog and Polly
**Goal**: Users can select NLog as an alternative to Serilog in the logging picker, and enable Polly resilience for HTTP clients
**Depends on**: Phase 6.1
**Requirements**: LOG-01, LOG-02, LOG-03, LOG-04, LOG-05, RESIL-01, RESIL-02, RESIL-03
**Success Criteria** (what must be TRUE):
  1. User can choose None, Serilog, or NLog in a mutually exclusive logging picker; selecting NLog produces NLog packages and appsettings.json NLog section with `throwConfigExceptions: true`
  2. WebApi/MinimalApi projects with NLog selected use `NLog.Web.AspNetCore` and `builder.Host.UseNLog()`; Worker Service/Console projects use `NLog.Extensions.Hosting` and `builder.UseNLog()`
  3. User can enable resilience (WebApi/MinimalApi only); the generated project includes `Microsoft.Extensions.Http.Resilience` with a named HttpClient and `AddStandardResilienceHandler()` — `Microsoft.Extensions.Http.Polly` never appears
**Plans:** 3/3 plans complete
Plans:
- [x] 07-01-PLAN.md — NLog + Resilience backend generators (NuGetVersionMap, CsprojGenerator, ProgramCsGenerator, AppSettingsGenerator, validation)
- [x] 07-02-PLAN.md — UI wiring: Logging RadioGroup, Resilience checkbox, URL persistence
- [x] 07-03-PLAN.md — Comprehensive automated tests for all 8 Phase 7 requirements

### Phase 8: OpenAPI Documentation
**Goal**: Users can select an OpenAPI documentation UI (Scalar, SwaggerUI, or Redoc) and the generator produces SDK-version-correct output
**Depends on**: Phase 7
**Requirements**: DOCS-01, DOCS-02, DOCS-03, DOCS-04, DOCS-05, DOCS-06
**Success Criteria** (what must be TRUE):
  1. User can select Scalar, SwaggerUI, or Redoc for WebApi/MinimalApi projects; the option is hidden for Console and Worker Service project types
  2. All three UI choices include `Microsoft.AspNetCore.OpenApi` with `AddOpenApi()` and `MapOpenApi()`; API doc middleware is always inside an `IsDevelopment()` guard
  3. Scalar selection adds `Scalar.AspNetCore 2.*` and `MapScalarApiReference()`; Redoc selection uses `Swashbuckle.AspNetCore.ReDoc` with `app.UseReDoc()` middleware
  4. SwaggerUI on .NET 8 uses full `Swashbuckle.AspNetCore`; SwaggerUI on .NET 9/10 uses the UI-only sub-package and pins `Microsoft.OpenApi < 3.0.0`
**Plans:** 2/2 plans complete
Plans:
- [x] 08-01-PLAN.md — Backend generators: enum rename (OpenApiUi), NuGetVersionMap, CsprojGenerator packages, ProgramCsGenerator fragments, validation
- [x] 08-02-PLAN.md — UI wiring (ConfigurationForm.razor OpenAPI radio group) + comprehensive automated tests

### Phase 9: Background Jobs
**Goal**: Users can select a background job strategy (IHostedService, Hangfire, Quartz.NET, or None) and the generated project includes the correct scaffold and file tree
**Depends on**: Phase 8
**Requirements**: JOBS-01, JOBS-02, JOBS-03, JOBS-04, JOBS-05, JOBS-06
**Success Criteria** (what must be TRUE):
  1. User can choose None, IHostedService, Hangfire, or Quartz.NET in the generator; Hangfire is hidden when no database is selected
  2. IHostedService selection generates a `SampleBackgroundService : BackgroundService` class with `AddHostedService<>` registration and no NuGet packages
  3. Hangfire selection auto-matches the storage NuGet package to the selected database (PostgreSQL, SQL Server, or InMemory fallback); Hangfire dashboard middleware is generated only for WebApi/MinimalApi projects
  4. Quartz.NET selection generates all three required packages (`Quartz`, `Quartz.Extensions.Hosting`, `Quartz.Extensions.DependencyInjection`) and a sample `IJob` implementation
  5. File tree includes a `Jobs/` or `Workers/` folder for all three architecture patterns when any background job option is selected
**Plans:** 2/2 plans complete
Plans:
- [x] 09-01-PLAN.md — Backend generators: BackgroundJobsGenerator.cs, CsprojGenerator packages, ProgramCsGenerator fragments, FileTreeService Jobs/ folder, ProjectGenerationService wiring
- [x] 09-02-PLAN.md — UI wiring (ConfigurationForm.razor Background Jobs radio group, reset logic) + comprehensive automated tests

### Phase 10: URL Serialization
**Goal**: All v1.2 generator options are serialized in shareable URLs and all existing v1.0/v1.1 URLs continue to resolve correctly
**Depends on**: Phase 9
**Requirements**: URL-01, URL-02, URL-03
**Success Criteria** (what must be TRUE):
  1. A URL containing `logging`, `docs`, `jobs`, and `resilience` query parameters fully restores those configuration values when visited; a round-trip test asserts serialize → deserialize → equality for all non-default config fields
  2. All existing v1.0 and v1.1 URLs (including `obs=serilog` and all v1.1 params) continue to work without producing a broken or default-reset configuration
  3. The pre-existing v1.1 gap where `IncludeResilience` was not serialized in `PushStateToUrl()` is closed and covered by the round-trip test
**Plans:** 1/1 plans complete
Plans:
- [x] 10-01-PLAN.md — TDD: URL round-trip tests + Home.razor docs/jobs query param wiring

### Phase 11: Code Preview and Responsive Design
**Goal**: Users can preview the content of any generated file with syntax highlighting directly in the browser, and the generator UI works correctly on phones and tablets
**Depends on**: Phase 7
**Requirements**: PREV-01, PREV-02, PREV-03, PREV-04, RESP-01, RESP-02, RESP-03, RESP-04
**Success Criteria** (what must be TRUE):
  1. Clicking any file node in the file tree opens a modal displaying the generated file content with syntax highlighting for C#, XML, JSON, YAML, Dockerfile, and Bash
  2. highlight.js is loaded as a local JS module via dynamic `import()` on first modal open — it is not loaded in `index.html` on page load; the component implements `IAsyncDisposable`
  3. The generator UI displays in a single-column stacked layout on viewports below 768px, and all form controls have a minimum 44px touch target
  4. The code preview modal is scrollable and correctly sized on mobile viewports; the responsive layout is verified against a production build on GitHub Pages (not localhost)
**Plans:** 2/2 plans complete
Plans:
- [ ] 11-01-PLAN.md — Code preview modal with highlight.js, file tree click handlers, folder toggle, Home.razor wiring
- [x] 11-02-PLAN.md — Responsive layout CSS, 44px touch targets, Phase 11 automated tests

### Phase 12: Tech Debt Cleanup
**Goal**: Close all integration gaps identified by the v1.2 milestone audit — NLog CLI commands, NLog development appsettings, FileTreePreview mobile touch targets, and Hangfire+SQLite user warning
**Depends on**: Phase 11
**Gap Closure**: Closes gaps LOG-01-CLI, LOG-04-DEV-SETTINGS, RESP-02-FILETREE from v1.2-MILESTONE-AUDIT.md
**Success Criteria** (what must be TRUE):
  1. CliCommandService.BuildCommands emits `dotnet add` commands for NLog packages (NLog.Web.AspNetCore for web, NLog.Extensions.Hosting for non-web), matching the existing Serilog block
  2. AppSettingsGenerator.GenerateAppSettingsDevelopment emits an NLog development override section (e.g., MinLevel adjustments) when NLog is selected, instead of empty `{}`
  3. FileTreePreview file-clickable nodes have `min-height: 44px` CSS on mobile viewports (below 768px), consistent with ConfigurationForm's touch target treatment
  4. When Hangfire + SQLite is selected, the UI shows a visual indicator that memory storage will be used (jobs won't persist across restarts)
**Plans:** 1/1 plans complete
Plans:
- [ ] 12-01-PLAN.md — NLog CLI commands, NLog dev appsettings, FileTreePreview touch targets, Hangfire+SQLite warning

---

### Phase 13: Template Foundation
**Goal**: Users can install and run a locally-built `dotnet-initializr` template that generates a minimal compilable .NET WebApi project with name substitution, dotfiles, and no silent template engine pitfalls
**Depends on**: Phase 12
**Requirements**: TMPL-01, TMPL-02, TMPL-03, TMPL-04, TMPL-05
**Success Criteria** (what must be TRUE):
  1. `dotnet new install Initializr.Templates` installs from the locally-packed `.nupkg` without errors and `dotnet new list` shows `dotnet-initializr`
  2. `dotnet new dotnet-initializr -n MyApp` generates files where every filename, directory name, and namespace containing `Company.ProjectName` is replaced with `MyApp`
  3. `dotnet build` on the generated project succeeds with zero errors and zero warnings on the first try
  4. `.gitignore` and `.editorconfig` are present in the generated output (not silently excluded by NuGet pack)
  5. All `#if DEBUG` guards and MSBuild `Condition` attributes in template source files survive instantiation unchanged (protected by `noEmit` escape markers)
**Plans:** 2/2 plans complete
Plans:
- [x] 13-01-PLAN.md — Create template source project files, template.json, and packaging csproj
- [ ] 13-02-PLAN.md — End-to-end verification script (pack, install, generate, build, validate)

### Phase 14: Core Parameter Model
**Goal**: The complete template parameter surface is declared in `template.json` — all choice parameters, boolean flags, computed symbols, and dependency chain enforcement — so that `dotnet new dotnet-initializr --help` shows the full set and invalid combinations are prevented before any feature content is written
**Depends on**: Phase 13
**Requirements**: PARAM-01, PARAM-02, PARAM-03, PARAM-04, PARAM-05, PARAM-06, PARAM-07, PARAM-08, PARAM-09, PARAM-10, PARAM-11, PARAM-12, PARAM-13
**Success Criteria** (what must be TRUE):
  1. `dotnet new dotnet-initializr --help` lists all 13 parameters with correct choices and descriptions
  2. `--db postgresql --orm none` is silently ignored or rejected — database is only active when an ORM is selected
  3. `--auth aspnetidentity --orm none` is silently ignored or rejected — ASP.NET Identity requires EF Core
  4. `--background-jobs hangfire --orm none` is silently ignored or rejected — Hangfire requires a database
  5. All computed boolean symbols (`IncludeEfCore`, `IsWebProject`, `HasDatabase`, etc.) are hidden from `--help` output via `dotnetcli.host.json`
**Plans:** 2/2 plans complete
Plans:
- [ ] 14-01-PLAN.md — Declare all parameters, computed symbols, and isEnabled gating in template.json + dotnetcli.host.json
- [ ] 14-02-PLAN.md — Extend test-template.sh with parameter verification (--help, gating, multi-value)

### Phase 15: Architecture and Project Types
**Goal**: `dotnet new dotnet-initializr --arch` and `--type` generate the correct folder structure and `.slnx` solution for all twelve architecture + project type combinations, and all generated solutions compile
**Depends on**: Phase 14
**Requirements**: GEN-01, GEN-02
**Success Criteria** (what must be TRUE):
  1. `--arch cleanarchitecture` generates a multi-project solution (`.Api`, `.Domain`, `.Application`, `.Infrastructure`) with correct `.slnx` project references
  2. `--arch verticalslice` and `--arch simplelayered` generate a single-project solution with the correct folder layout for each
  3. All four project types (WebApi, MinimalApi, Console, WorkerService) generate correct entry point files and compile across all three architectures
  4. The generated `.slnx` file lists exactly the projects that exist in the output directory — no missing or phantom references
**Plans**: TBD

### Phase 16: Data Access and Auth
**Goal**: `--orm`, `--db`, `--auth`, and `--framework` produce compilable generated projects with correct package references (SDK-version-aware), connection string configuration, and enforced dependency chains
**Depends on**: Phase 15
**Requirements**: PARAM-03, PARAM-04, PARAM-05, PARAM-07, GEN-04, GEN-06
**Success Criteria** (what must be TRUE):
  1. `--orm efcore --db postgresql --framework net9.0` generates a project with the correct versioned `Npgsql.EntityFrameworkCore.PostgreSQL` reference and `AddDbContext<>` registration that compiles on .NET 9
  2. `--orm dapper --db sqlserver` generates a project with `Dapper` and `Microsoft.Data.SqlClient` references and an `IDbConnection` factory — no EF Core packages present
  3. `--auth jwt` generates JWT bearer middleware; `--auth aspnetidentity` generates Identity scaffolding with EF Core and fails gracefully when EF Core is not selected
  4. `--auth keycloak` generates JwtBearer with Authority configuration; `--auth apikey` generates the inline `ApiKeyAuthenticationHandler` class
  5. `--api-docs swaggerui --framework net8.0` uses full Swashbuckle; `--api-docs swaggerui --framework net9.0` uses the UI sub-package with `Microsoft.OpenApi` pin
**Plans**: TBD

### Phase 17: Logging, Testing, and Quality
**Goal**: Logging choice, testing library flags, and all boolean quality feature flags produce correct conditional content in `Program.cs`, `.csproj`, and `appsettings.json`
**Depends on**: Phase 16
**Requirements**: PARAM-06, PARAM-08, PARAM-13, GEN-03, GEN-05
**Success Criteria** (what must be TRUE):
  1. `--logging nlog` on a WebApi project uses `NLog.Web.AspNetCore`; on a WorkerService uses `NLog.Extensions.Hosting` — the wrong package never appears
  2. `--testing xunit,fluentassertions,nsubstitute` generates a test project with all three packages and no packages for omitted flags
  3. `--validation` adds FluentValidation v12 packages; `--resilience` adds `Microsoft.Extensions.Http.Resilience`; `--caching` adds StackExchange.Redis; `--mapping` adds Mapster — each flag independently controls exactly its own packages
  4. `Program.cs` in the generated project contains only the `#if` blocks relevant to the selected features — no dead conditional blocks for unselected features appear in the output
  5. `appsettings.json` contains configuration sections only for selected features (e.g., no `ConnectionStrings` section when `--orm none`)
**Plans**: TBD

### Phase 18: DevOps, Containers, and Background Jobs
**Goal**: CI/CD workflow files, container files, and background job scaffolding are included or excluded via `sources.modifiers` for the correct parameter combinations
**Depends on**: Phase 17
**Requirements**: PARAM-09, PARAM-10, PARAM-11, PARAM-12, GEN-07
**Success Criteria** (what must be TRUE):
  1. `--cicd githubactions` includes `.github/workflows/build.yml`; `--cicd azuredevops` includes `azure-pipelines.yml`; `--cicd none` includes neither — selection is via `sources.modifiers`, not inline YAML conditionals
  2. `--containers dockerfile` includes only `Dockerfile`; `--containers dockercompose` includes `Dockerfile` and `docker-compose.yml`; `--containers aspire` includes the Aspire AppHost project
  3. `--background-jobs hangfire` generates Hangfire registration with the storage package matching the selected database; `--background-jobs quartz` generates all three Quartz packages and a sample `IJob`
  4. `--background-jobs ihostedservice` generates a `BackgroundService` subclass with `AddHostedService<>` and zero NuGet packages
  5. `--api-docs` (Scalar/SwaggerUI/Redoc) is only active for WebApi and MinimalApi project types — Console and WorkerService receive no API docs content regardless of flag
**Plans**: TBD

### Phase 19: Blazor CLI Panel
**Goal**: The Blazor app displays a `dotnet new dotnet-initializr --flags` command that exactly matches the current configuration, alongside the existing `dotnet add` commands
**Depends on**: Phase 18
**Requirements**: CLI-01, CLI-02, CLI-03
**Success Criteria** (what must be TRUE):
  1. The Blazor generator page shows a CLI panel containing `dotnet new install Initializr.Templates` and `dotnet new dotnet-initializr -n <name> --flags` built from the current `ProjectConfiguration`
  2. The `--flags` in the displayed command use the same parameter names and PascalCase choice values as the finalized `template.json` (e.g., `--arch CleanArchitecture`, not `--arch clean-architecture`)
  3. The existing `dotnet add` package commands panel remains visible and unchanged alongside the new `dotnet new` panel
**Plans**: TBD

### Phase 20: NuGet Packaging and Distribution
**Goal**: `Initializr.Templates` is published to NuGet.org as a pre-release and then stable package, with a GitHub Actions workflow and a `.nupkg` artifact attached to the GitHub release
**Depends on**: Phase 19
**Requirements**: GEN-08, DIST-01, DIST-02, DIST-03, DIST-04
**Success Criteria** (what must be TRUE):
  1. All critical parameter combinations (at minimum: each `--arch` x each `--type`, `--orm efcore --db postgresql`, `--auth jwt`, `--logging serilog`, `--logging nlog`) pass `dotnet build` via the automated test suite
  2. `dotnet new install Initializr.Templates` from NuGet.org installs successfully and `dotnet new dotnet-initializr -n Smoke` generates a buildable project
  3. The pre-release `1.3.0-beta.1` is published first, verified manually (dotfiles present, correct package contents via `unzip -l`), before the stable `1.3.0` is published
  4. A GitHub Actions `templates.yml` workflow builds and tests on every push, and publishes to NuGet.org only on version-tag push (`v1.3.*`)
  5. The `.nupkg` file is attached as an artifact to the GitHub release for the `v1.3.0` tag
**Plans**: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 13 → 14 → 15 → 16 → 17 → 18 → 19 → 20

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1. UI and Configuration Form | v1.0 | 4/4 | Complete | 2026-02-26 |
| 2. Generation Engine and Output | v1.0 | 7/7 | Complete | 2026-02-26 |
| 3. Deployment | v1.0 | 2/2 | Complete | 2026-02-27 |
| 4. Foundation | v1.1 | 4/4 | Complete | 2026-02-27 |
| 5. Data Layer and Caching | v1.1 | 3/3 | Complete | 2026-02-28 |
| 6. Auth, Validation, and Testing | v1.1 | 3/3 | Complete | 2026-02-28 |
| 6.1 Validation and URL Persistence Fixes | v1.1 | 1/1 | Complete | 2026-02-28 |
| 7. NLog and Polly | v1.2 | 3/3 | Complete | 2026-03-01 |
| 8. OpenAPI Documentation | v1.2 | 2/2 | Complete | 2026-03-01 |
| 9. Background Jobs | v1.2 | 2/2 | Complete | 2026-03-01 |
| 10. URL Serialization | v1.2 | 1/1 | Complete | 2026-03-01 |
| 11. Code Preview and Responsive Design | v1.2 | 2/2 | Complete | 2026-03-01 |
| 12. Tech Debt Cleanup | v1.2 | 1/1 | Complete | 2026-03-07 |
| 13. Template Foundation | 2/2 | Complete    | 2026-03-08 | - |
| 14. Core Parameter Model | 2/2 | Complete   | 2026-03-08 | - |
| 15. Architecture and Project Types | v1.3 | 0/? | Not started | - |
| 16. Data Access and Auth | v1.3 | 0/? | Not started | - |
| 17. Logging, Testing, and Quality | v1.3 | 0/? | Not started | - |
| 18. DevOps, Containers, and Background Jobs | v1.3 | 0/? | Not started | - |
| 19. Blazor CLI Panel | v1.3 | 0/? | Not started | - |
| 20. NuGet Packaging and Distribution | v1.3 | 0/? | Not started | - |
