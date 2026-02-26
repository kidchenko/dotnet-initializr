# Feature Landscape

**Domain:** .NET project initializer / scaffolding tool (browser-based, static hosting)
**Researched:** 2026-02-26
**Confidence:** MEDIUM — based on direct knowledge of Spring Initializr, JHipster, dotnet new, Yeoman, create-react-app, and NX. WebSearch/WebFetch unavailable during research session; findings from training data (cutoff Aug 2025) cross-referenced against PROJECT.md constraints.

---

## Reference Tools Surveyed

| Tool | Language/Platform | Key Trait |
|------|------------------|-----------|
| Spring Initializr (start.spring.io) | Java/Spring | Gold standard UX; dependency picker; zero backend waste |
| JHipster | Java + Angular/React | Opinionated full-stack; rich architecture choices |
| dotnet new | .NET | CLI-first; template NuGet ecosystem; limited architecture options |
| create-react-app / Vite create | JS/TS front-end | Minimal — project type + variant only |
| Yeoman generators | Any | Generator-per-stack; composable but fragmented |
| NX | JS monorepo | Workspace-level; plugin system; interactive presets |
| Cookiecutter | Python/Any | Template-file-based; powerful but CLI-only |

---

## Table Stakes

Features users expect from ANY project initializer. Missing = product feels incomplete or untrustworthy.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Project name / namespace input | Every tool has it; identifies the output | Low | Validated by dotnet conventions: `MyCompany.MyProduct` |
| SDK / runtime version selection | Users target specific .NET LTS/STS releases | Low | Must offer .NET 8 (LTS), .NET 9, .NET 10 |
| Project type selection | Core differentiator of what gets scaffolded | Low | Web API, Minimal API, Console, Worker Service per PROJECT.md |
| Immediate zip download | The entire value delivery mechanism | Medium | Browser zip via System.IO.Compression; must work offline |
| Working generated code | Generated project must compile on first `dotnet build` | High | Most trust-destroying failure is code that doesn't build |
| `.gitignore` included | Every real project needs it; absence is embarrassing | Low | Standard .NET .gitignore pattern |
| `.editorconfig` included | .NET ecosystem norm; ensures code style consistency | Low | Standard Microsoft defaults |
| Solution file (`.sln`) | .NET ecosystem standard; IDEs expect it | Low | Required for multi-project architectures |
| Correct `.csproj` structure | Must reflect chosen SDK, target framework, references | Medium | Template must be parametrized correctly |
| `README.md` with setup instructions | Users need to know how to run what they downloaded | Low | "How to run" + "how to test" minimum |
| Conditional option visibility | Database picker only when ORM is selected, etc. | Medium | Core UX expectation set by Spring Initializr |
| Responsive / usable on desktop | Tool used by developers on desktops | Low | Mobile not required |
| Dark theme | Developer tool expectation; Spring Initializr sets the bar | Low | Required per PROJECT.md |

---

## Differentiators

Features that set NetStarter apart from `dotnet new` and plain Blazor templates. Not universally expected, but meaningfully valued.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Architecture pattern selection | Clean Architecture, Vertical Slice not in `dotnet new` — biggest gap in .NET ecosystem | High | This is the #1 differentiator per PROJECT.md |
| ORM + database combo wiring | EF Core configured with PostgreSQL or SQL Server out of the box; correct connection string, DbContext, migration setup | High | JHipster does this well; dotnet new does not |
| Auth scaffolding (JWT) | JWT bearer auth wired into Program.cs + middleware; not just a placeholder | High | Adding auth post-hoc is error-prone and commonly forgotten |
| Observability setup (Serilog, Health Checks) | Production-ready from day one; not an afterthought | Medium | Serilog with console + file sink; /health endpoint |
| Testing project wired up | xUnit + FluentAssertions + Testcontainers configured and referencing the main project | High | Most scaffolders omit test projects entirely |
| Dockerfile + docker-compose | Container-ready output; multi-stage Dockerfile for .NET | Medium | Many .NET devs containerize immediately |
| .NET Aspire support option | Modern .NET-native orchestration; highly current (2024+) | High | Differentiates from any Java-centric tool; Aspire AppHost wired |
| GitHub Actions CI scaffold | `.github/workflows/dotnet.yml` with build + test steps | Low | Minimal but complete; saves 30 min of YAML authoring |
| "dotnet CLI equivalent" display | Shows the CLI commands that approximate the choices made | Low | Bridges users toward `dotnet new` fluency; unique to NetStarter |
| Browser-only, zero backend | No API key, no account, no data sent anywhere — privacy and trust | Low (arch) | Unique vs JHipster which requires a running generator server |
| Static hosting friendly | Deployable to GitHub Pages / Azure Static Web Apps for free | Low (arch) | Lowers barrier to self-hosting the tool itself |
| Shareable configuration URL | URL encodes current selections so users can share a preset | Medium | Spring Initializr does this; high DX value |
| Project preview panel | Show a tree of files that will be generated before download | Medium | Increases trust; users know what they're getting |

---

## Anti-Features

Features to deliberately NOT build in v1 — either scope creep, complexity traps, or dilution of focus.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| Server-side backend / API | Destroys zero-cost static hosting; adds ops burden | Keep all generation client-side in WASM |
| Blazor as a project type (v1) | Confuses users ("is this generating itself?"); Blazor template complexity is high | Mark as v2; focus on API/Console/Worker |
| OAuth / Keycloak auth | Auth provider integration is a deep rabbit hole; each provider has its own setup | Ship JWT in v1; expand in v2 |
| NuGet package `dotnet new` custom template | Separate distribution channel; different UX; doubles maintenance | Scope to v2; focus on web UI first |
| Push to GitHub | OAuth flow, token management, GitHub API integration; major scope | v2 feature; ship zip first |
| Full OpenTelemetry | OTEL SDK setup is nuanced (exporters, collectors); Serilog covers basic observability | Full OTEL in v2; Health Checks + Serilog in v1 |
| gRPC project type | Small audience; complex Protobuf scaffolding; distracts from core types | v2 if validated |
| NUnit / MSTest support | xUnit is the dominant .NET testing framework; multiple options increase template complexity | Ship xUnit only in v1 |
| SQLite / Dapper | SQLite is useful but Dapper requires very different template structure; adds matrix size | v2; focus on EF Core + PostgreSQL/SQL Server |
| Azure DevOps CI | GitHub Actions covers most users; Azure DevOps CI YAML is different enough to double effort | v2 if user demand exists |
| Live preview / code editor in browser | Monaco or CodeMirror integration is high complexity; not what users need | Tree preview (file names only) is sufficient |
| Account / login / saved projects | No backend; no auth for the tool itself; privacy is a feature | Use shareable URL instead |
| Monorepo / workspace generation | Different mental model entirely; scope explosion | Out of scope for v1 and likely v2 |
| Interactive CLI companion | Separate project; requires distribution and install | Let the web UI be the primary UX |

---

## Feature Dependencies

```
SDK version selection
  → Project type selection
      → Architecture pattern selection
          → ORM selection
              → Database selection (only if EF Core)
                  → Auth selection
                      → Observability options (Serilog, Health Checks)
                          → Testing setup
                              → Container support
                                  → CI/CD scaffold
                                      → ZIP generation
                                          → CLI equivalent display

Conditional visibility:
  - Database picker: visible only when ORM = EF Core
  - Testcontainers: visible only when Testing is enabled AND database is selected
  - .NET Aspire: visible only when Container support is enabled
  - docker-compose: visible only when Container support is enabled

Architecture constraints:
  - Clean Architecture: generates src/Domain, src/Application, src/Infrastructure, src/Presentation layers
  - Vertical Slice: generates Features/ folder structure with handler-per-feature
  - Simple Layered: generates Controllers/, Services/, Repositories/ (classic 3-tier)

Each architecture × project type combination is a distinct template:
  3 architectures × 3 project types (Web API, Minimal API, Worker) = 9 core template variants
  Console stays simple (1 template)
```

---

## MVP Recommendation

Prioritize (v1):

1. **Project metadata + SDK selection** — trivial, foundational
2. **Project type selection** (Web API, Minimal API, Console, Worker Service) — core identity
3. **Architecture pattern selection** (Clean, Vertical Slice, Simple Layered) — #1 differentiator
4. **Working generated code that compiles** — non-negotiable trust signal
5. **ZIP download** — the delivery mechanism
6. **ORM + database selection** (EF Core + PostgreSQL/SQL Server) — high DX value
7. **Auth selection** (None, JWT) — commonly forgotten boilerplate
8. **Observability** (Serilog, Health Checks) — production-readiness signal
9. **Testing project** (xUnit + FluentAssertions + Testcontainers) — differentiator from dotnet new
10. **Container support** (Dockerfile, docker-compose, Aspire) — modern .NET norm
11. **CI/CD scaffold** (GitHub Actions) — low complexity, high value
12. **CLI equivalent display** — unique to NetStarter; low complexity

Defer to v2:
- Push to GitHub repo
- Shareable configuration URL (nice-to-have; can ship later)
- Project file tree preview (nice-to-have; adds trust but not MVP)
- dotnet new NuGet template package
- Blazor, gRPC, OAuth, Azure DevOps, NUnit, Dapper, SQLite, full OTEL

---

## Competitive Gap Analysis

| Capability | Spring Initializr | JHipster | dotnet new | **NetStarter** |
|-----------|-------------------|----------|------------|----------------|
| Architecture patterns | No | Yes (DDD, hexagonal) | No | Yes (Clean, Vertical Slice) |
| ORM wired with connection string | Partial | Yes | No | Yes |
| Auth scaffolded | Partial | Yes (full OAuth) | No | Yes (JWT) |
| Testing project | No | Yes | No | Yes |
| Container support | No | Yes | Partial | Yes |
| CI/CD scaffold | No | Yes | No | Yes |
| Browser-only (no server) | No | No | N/A (CLI) | Yes |
| Static hosting | No | No | N/A | Yes |
| .NET Aspire | No | No | Partial | Yes |
| Open-source, self-hostable | Yes | Yes | N/A | Yes |

NetStarter's unique combination: .NET-specific + architecture patterns + browser-only + zero cost.

---

## Sources

- PROJECT.md (direct project constraints and scope decisions)
- Spring Initializr (start.spring.io) — gold-standard UI/UX reference; feature set from direct observation (training data, HIGH confidence)
- JHipster feature list — architecture options, full-stack generation, auth integration (HIGH confidence, mature tool)
- `dotnet new` official documentation — templates, options, limitations (HIGH confidence)
- create-react-app / Vite scaffolding — minimal scaffolding baseline (HIGH confidence)
- Yeoman generator ecosystem — plugin-based generation patterns (MEDIUM confidence)
- NX workspace generator — preset system, interactive configuration (MEDIUM confidence)
- .NET Aspire documentation — AppHost orchestration, service defaults (MEDIUM confidence, 2024+ feature)

**Confidence note:** WebSearch and WebFetch were unavailable. All findings based on training data through August 2025 and direct project context from PROJECT.md. Architecture pattern naming (Clean Architecture, Vertical Slice Architecture) is well-established in the .NET community and unlikely to have changed. Feature categorizations are based on direct comparison experience with the reference tools. LOW risk of staleness for this feature domain.
