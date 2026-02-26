# Roadmap: NetStarter

## Overview

Build a browser-only .NET project generator in three phases: first the full interactive form (Blazorise + Tailwind CSS, all options wired, conditional visibility, shareable URLs), then the generation engine that turns those selections into a downloadable zip of compilable .NET code, then deployment to static hosting with automated CI/CD. The UI is built and validated before any code generation is written — the form is the contract the engine implements.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: UI and Configuration Form** - Full Spring Initializr-style form with all options, conditional visibility, and shareable URLs (Blazorise + Tailwind CSS) (completed 2026-02-26)
- [ ] **Phase 2: Generation Engine and Output** - Client-side engine that generates compilable .NET projects as a downloadable zip with CLI equivalent display
- [ ] **Phase 3: Deployment** - Static hosting deployment with correct SPA routing and automated CI/CD

## Phase Details

### Phase 1: UI and Configuration Form
**Goal**: Users can configure every .NET project option through a complete, functional form and share their configuration via URL
**Depends on**: Nothing (first phase)
**Requirements**: UI-01, UI-02, UI-03, UI-04, UI-05, UI-06, CFG-01, CFG-02, CFG-03, CFG-04, CFG-05, CFG-06, CFG-07, CFG-08, CFG-09, CFG-10, CFG-11
**Success Criteria** (what must be TRUE):
  1. User sees a dark-themed single-page layout styled with Blazorise + Tailwind CSS that matches the Spring Initializr visual feel
  2. User can fill in all configuration fields (project name, namespace, SDK version, project type, architecture, ORM, database, auth, observability, testing, containers, CI/CD) and the database picker only appears when EF Core is selected
  3. User can preview a file tree showing what would be generated based on their current selections
  4. User can copy a URL that encodes all their current selections, paste it in a new tab, and see the same configuration restored
  5. User sees a loading indicator during WASM cold start before the form is interactive
**Plans:** 4/4 plans complete

Plans:
- [ ] 01-01-PLAN.md — Foundation: Blazorise + Tailwind setup, loading screen, two-panel layout, ProjectConfiguration model
- [ ] 01-02-PLAN.md — Configuration form with all sections and input controls
- [ ] 01-03-PLAN.md — File tree preview service and component with live updates
- [ ] 01-04-PLAN.md — Home page composition, URL state sharing, and visual verification

### Phase 2: Generation Engine and Output
**Goal**: Users can generate and download a compilable .NET project zip that reflects all their configuration choices
**Depends on**: Phase 1
**Requirements**: GEN-01, GEN-02, GEN-03, GEN-04, GEN-05, GEN-06, GEN-07, GEN-08, GEN-09, GEN-10, GEN-11, GEN-12, GEN-13, GEN-14, GEN-15, GEN-16, GEN-17, GEN-18, GEN-19, GEN-20, GEN-21, GEN-22, OUT-01, OUT-02, OUT-03
**Success Criteria** (what must be TRUE):
  1. User clicks Generate and receives a zip download containing a .NET solution in .slnx format with all files under a root folder named after their project
  2. The downloaded zip passes `dotnet build` without errors for every supported SDK version and every supported project type and architecture combination
  3. Generated code correctly reflects all selected options: EF Core DbContext and connection string when ORM is selected, JWT middleware when auth is selected, Serilog/health checks when observability is selected, test project when testing is selected, Dockerfile/docker-compose/.NET Aspire files when container support is selected, GitHub Actions workflow when CI/CD is selected
  4. User can see and copy the equivalent dotnet CLI commands that approximate their generated project alongside the download button
**Plans:** 5/7 plans executed

Plans:
- [ ] 02-01-PLAN.md — Foundation services: NuGet version map, ZipService, CliCommandService
- [ ] 02-02-PLAN.md — Core templates: .slnx, .csproj, .gitignore, .editorconfig, appsettings.json generators
- [ ] 02-03-PLAN.md — Architecture + Program.cs: architecture-specific structure and conditional feature wiring
- [ ] 02-04-PLAN.md — Feature templates: EF Core, JWT auth, observability, Mapster generators
- [ ] 02-05-PLAN.md — Infra + test templates: Docker, Aspire, CI/CD, xUnit, Testcontainers, README generators
- [ ] 02-06-PLAN.md — Orchestrator: ProjectGenerationService composing all generators into file dictionary
- [ ] 02-07-PLAN.md — UI wiring: Generate button, zip download, CLI command panel, human verification

### Phase 3: Deployment
**Goal**: The app is live on static hosting, reachable at a public URL, with automated deploys on every push to main
**Depends on**: Phase 2
**Requirements**: DEP-01, DEP-02
**Success Criteria** (what must be TRUE):
  1. App loads correctly at its public URL (GitHub Pages, Cloudflare Pages, or Azure Static Web Apps) including deep-linked shareable configuration URLs without 404 errors
  2. A push to the main branch automatically triggers a CI/CD pipeline that builds, tests, and deploys the app to the static host without manual steps
**Plans**: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. UI and Configuration Form | 4/4 | Complete    | 2026-02-26 |
| 2. Generation Engine and Output | 5/7 | In Progress|  |
| 3. Deployment | 0/TBD | Not started | - |
