# Roadmap: NetStarter

## Milestones

- ✅ **v1.0 MVP** — Phases 1-3 (shipped 2026-02-27)
- ✅ **v1.1 Expanded Generator Options** — Phases 4-6.1 (shipped 2026-02-28)
- 📋 **v1.2 Infrastructure, UX & Polish** — Phases 7-11 (in progress)

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

### 📋 v1.2 Infrastructure, UX & Polish (In Progress)

**Milestone Goal:** Add NLog, Polly, OpenAPI UI picker, and background jobs to the generator; expose all new options in shareable URLs with backward-compatible parsing; add code preview with syntax highlighting; make the UI fully responsive on phones and tablets.

- [x] **Phase 7: NLog and Polly** - Wire NLog logging option and Polly resilience scaffold into the generator
- [x] **Phase 8: OpenAPI Documentation** - Add the OpenAPI document layer and Scalar/SwaggerUI/Redoc UI picker with SDK-version-aware generated code
- [ ] **Phase 9: Background Jobs** - Add IHostedService, Hangfire, and Quartz.NET scaffolding with file tree and Jobs/Workers folder support
- [ ] **Phase 10: URL Serialization** - Serialize all new v1.2 options as query parameters with full backward-compatible parsing
- [ ] **Phase 11: Code Preview and Responsive Design** - Add code preview modal with highlight.js syntax highlighting and make the UI fully responsive

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
**Plans:** 2 plans
Plans:
- [ ] 09-01-PLAN.md — Backend generators: BackgroundJobsGenerator.cs, CsprojGenerator packages, ProgramCsGenerator fragments, FileTreeService Jobs/ folder, ProjectGenerationService wiring
- [ ] 09-02-PLAN.md — UI wiring (ConfigurationForm.razor Background Jobs radio group, reset logic) + comprehensive automated tests

### Phase 10: URL Serialization
**Goal**: All v1.2 generator options are serialized in shareable URLs and all existing v1.0/v1.1 URLs continue to resolve correctly
**Depends on**: Phase 9
**Requirements**: URL-01, URL-02, URL-03
**Success Criteria** (what must be TRUE):
  1. A URL containing `logging`, `docs`, `jobs`, and `resilience` query parameters fully restores those configuration values when visited; a round-trip test asserts serialize → deserialize → equality for all non-default config fields
  2. All existing v1.0 and v1.1 URLs (including `obs=serilog` and all v1.1 params) continue to work without producing a broken or default-reset configuration
  3. The pre-existing v1.1 gap where `IncludeResilience` was not serialized in `PushStateToUrl()` is closed and covered by the round-trip test
**Plans**: TBD

### Phase 11: Code Preview and Responsive Design
**Goal**: Users can preview the content of any generated file with syntax highlighting directly in the browser, and the generator UI works correctly on phones and tablets
**Depends on**: Phase 7
**Requirements**: PREV-01, PREV-02, PREV-03, PREV-04, RESP-01, RESP-02, RESP-03, RESP-04
**Success Criteria** (what must be TRUE):
  1. Clicking any file node in the file tree opens a modal displaying the generated file content with syntax highlighting for C#, XML, JSON, YAML, Dockerfile, and Bash
  2. highlight.js is loaded as a local JS module via dynamic `import()` on first modal open — it is not loaded in `index.html` on page load; the component implements `IAsyncDisposable`
  3. The generator UI displays in a single-column stacked layout on viewports below 768px, and all form controls have a minimum 44px touch target
  4. The code preview modal is scrollable and correctly sized on mobile viewports; the responsive layout is verified against a production build on GitHub Pages (not localhost)
**Plans**: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 7 → 8 → 9 → 10 → 11
(Phase 11 depends only on Phase 7; it can start after Phase 7 completes if needed)

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1. UI and Configuration Form | v1.0 | 4/4 | Complete | 2026-02-26 |
| 2. Generation Engine and Output | v1.0 | 7/7 | Complete | 2026-02-26 |
| 3. Deployment | v1.0 | 2/2 | Complete | 2026-02-27 |
| 4. Foundation | v1.1 | 4/4 | Complete | 2026-02-27 |
| 5. Data Layer and Caching | v1.1 | 3/3 | Complete | 2026-02-28 |
| 6. Auth, Validation, and Testing | v1.1 | 3/3 | Complete | 2026-02-28 |
| 6.1 Validation and URL Persistence Fixes | v1.1 | 1/1 | Complete | 2026-02-28 |
| 7. NLog and Polly | 3/3 | Complete    | 2026-03-01 | 2026-02-28 |
| 8. OpenAPI Documentation | v1.2 | Complete    | 2026-03-01 | 2026-03-01 |
| 9. Background Jobs | v1.2 | 0/2 | Planned | - |
| 10. URL Serialization | v1.2 | 0/? | Not started | - |
| 11. Code Preview and Responsive Design | v1.2 | 0/? | Not started | - |
