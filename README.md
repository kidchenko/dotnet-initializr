<p align="center">
  <img src="src/NetStarter/wwwroot/img/dotnet-initialzr_128x128.png" alt=".NET initialzr" width="128" height="128" />
</p>

<h1 align="center">.NET initialzr</h1>

<p align="center">
  A browser-based project scaffolding tool for .NET — configure, preview, and download ready-to-go solutions.
</p>

<p align="center">
  <a href="https://starter.dotnetth.com">starter.dotnetth.com</a>
</p>

---

## What it does

Pick your options in the UI and download a fully structured .NET solution as a zip file. No CLI required.

**Project types:** Web API, Minimal API, Console, Worker Service

**Architecture patterns:** Clean Architecture, Vertical Slice, Simple Layered

**Data:** EF Core with PostgreSQL or SQL Server

**Auth:** JWT bearer authentication

**Observability:** Serilog, Health Checks, OpenTelemetry

**Testing:** xUnit or NUnit, Shouldly, Testcontainers

**DevOps:** Dockerfile, docker-compose, .NET Aspire, GitHub Actions, Azure DevOps pipelines

**Mapping:** Mapster

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run locally

```bash
dotnet watch --project src/NetStarter
```

The app opens at `https://localhost:5001` (or the port shown in the terminal).

### Shareable URLs

Every configuration option is encoded in the URL query string. Share a link and the recipient gets the exact same setup.

## Tech stack

- **Blazor WebAssembly** — runs entirely in the browser, no server needed
- **Blazorise + Tailwind CSS** — UI components and utility styling
- **FontAwesome** — icons
- **PWA** — installable, works offline after first load

## Deployment

Deployed automatically to GitHub Pages via the `deploy.yml` workflow on every push to `main`.

## License

MIT
