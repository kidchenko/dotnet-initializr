# Architecture Research

**Domain:** Client-side .NET project generator (Blazor WASM)
**Researched:** 2026-02-26
**Confidence:** HIGH (architectural decisions are independent of library versions)

## Standard Architecture

### System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Blazor WASM UI Layer                     │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │ Config   │  │ Preview  │  │ Generate │  │ CLI      │    │
│  │ Form     │  │ Panel    │  │ Button   │  │ Output   │    │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘    │
│       │              │             │              │          │
├───────┴──────────────┴─────────────┴──────────────┴──────────┤
│                     State / Config Model                     │
│  ┌──────────────────────────────────────────────────────┐    │
│  │              ProjectConfig (record)                   │    │
│  │  SdkVersion, ProjectType, Architecture, Orm, Auth... │    │
│  └──────────────────────┬───────────────────────────────┘    │
├─────────────────────────┴────────────────────────────────────┤
│                     Generation Engine                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │ Solution │  │ Project  │  │ File     │  │ CLI Cmd  │    │
│  │ Generator│  │ Generator│  │ Templates│  │ Builder  │    │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘    │
│       │              │             │              │          │
├───────┴──────────────┴─────────────┴──────────────┴──────────┤
│                     Output Layer                             │
│  ┌──────────────────┐  ┌──────────────────┐                  │
│  │ ZipArchive       │  │ CLI Commands     │                  │
│  │ (BCL in-memory)  │  │ (string output)  │                  │
│  └────────┬─────────┘  └────────┬─────────┘                  │
│           │                     │                            │
│  ┌────────┴─────────────────────┴────────┐                   │
│  │  JS Interop (file download trigger)   │                   │
│  └───────────────────────────────────────┘                   │
└─────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Typical Implementation |
|-----------|----------------|------------------------|
| Config Form | Captures user selections for all project options | Blazor component with MudBlazor form controls, two-way binding to `ProjectConfig` |
| Preview Panel | Shows what will be generated (file tree, key files) | Read-only display driven by `ProjectConfig` changes |
| Generate Button | Triggers generation + download | Calls `ProjectGenerator.Generate(config)`, pipes to zip, triggers download |
| CLI Output | Shows equivalent `dotnet new` + `dotnet add` commands | String builder driven by `ProjectConfig` |
| ProjectConfig | Single source of truth for all user selections | C# record with validation; drives all generation logic |
| Solution Generator | Orchestrates full solution creation | Determines which projects to create based on architecture pattern |
| Project Generator | Creates individual .csproj files | Handles SDK version, package references, project properties |
| File Templates | Generate individual source files (Program.cs, etc.) | C# classes with `Generate(ProjectConfig)` methods returning strings |
| CLI Command Builder | Builds equivalent CLI commands | Maps config options to `dotnet new` / `dotnet add` arguments |
| ZipArchive | Packages all generated files into a downloadable zip | `System.IO.Compression.ZipArchive` + `MemoryStream` |
| JS Interop | Triggers browser file download from WASM bytes | Small JS function that creates a Blob and click-downloads it |

## Recommended Project Structure

```
NetStarter/NetStarter/
├── wwwroot/                    # Static assets
│   ├── index.html              # WASM host page
│   ├── css/                    # Custom styles (minimal — MudBlazor handles most)
│   └── js/
│       └── download.js         # File download JS interop
├── Models/                     # Configuration and option models
│   ├── ProjectConfig.cs        # Main config record (all user selections)
│   ├── Enums/                  # SdkVersion, ProjectType, Architecture, etc.
│   └── Validation/             # Config validation rules
├── Components/                 # Reusable Blazor components
│   ├── ConfigForm.razor        # Main configuration form
│   ├── ProjectPreview.razor    # File tree preview
│   ├── CliOutput.razor         # CLI equivalent display
│   └── Shared/                 # Shared UI pieces
├── Generation/                 # Template engine (the core)
│   ├── ProjectGenerator.cs     # Orchestrator — takes ProjectConfig, returns file list
│   ├── SolutionGenerator.cs    # Creates .sln structure
│   ├── Templates/              # Individual file generators
│   │   ├── Base/               # Shared across architectures
│   │   │   ├── CsprojTemplate.cs
│   │   │   ├── AppSettingsTemplate.cs
│   │   │   ├── DockerfileTemplate.cs
│   │   │   └── GitIgnoreTemplate.cs
│   │   ├── WebApi/             # Web API specific
│   │   │   ├── ProgramCsTemplate.cs
│   │   │   └── ControllerTemplate.cs
│   │   ├── MinimalApi/         # Minimal API specific
│   │   │   ├── ProgramCsTemplate.cs
│   │   │   └── EndpointTemplate.cs
│   │   ├── Console/            # Console / Worker specific
│   │   │   └── ProgramCsTemplate.cs
│   │   └── Architecture/       # Architecture-specific overlays
│   │       ├── CleanArch/
│   │       ├── VerticalSlice/
│   │       └── SimpleLayered/
│   ├── Cli/                    # CLI command generation
│   │   └── CliCommandBuilder.cs
│   └── Packaging/              # Zip creation
│       └── ZipPackager.cs
├── Services/                   # App services
│   └── DownloadService.cs      # JS interop for file download
├── Layout/                     # App layout (MudBlazor layout)
│   └── MainLayout.razor
├── Pages/                      # Page components
│   └── Home.razor              # Single page — the generator
├── Program.cs                  # WASM host setup
├── App.razor                   # Root component
└── _Imports.razor              # Global using directives
```

### Structure Rationale

- **Models/**: Separated from generation logic. `ProjectConfig` is the contract between UI and engine.
- **Components/**: Razor components that compose the UI. ConfigForm is the most complex.
- **Generation/**: The core engine. No Blazor dependencies — pure C# that takes a config and returns files. This is testable in isolation.
- **Generation/Templates/Base/**: Files shared across all project types (Dockerfile, .gitignore, appsettings.json).
- **Generation/Templates/{ProjectType}/**: Project-type-specific files. Each has its own `ProgramCsTemplate` because `Program.cs` differs significantly between Web API, Minimal API, and Console.
- **Generation/Templates/Architecture/**: Architecture pattern overlays. These define folder structure and cross-project references (e.g., Clean Architecture creates Domain, Application, Infrastructure, Presentation projects).
- **Services/**: Thin wrapper around JS interop for testability.

## Architectural Patterns

### Pattern 1: Composable File Generation (Config → Files)

**What:** Each template is a class with a `Generate(ProjectConfig) → GeneratedFile` method. The `ProjectGenerator` orchestrates which templates to run based on config.

**When to use:** Always — this is the core pattern.

**Trade-offs:** Simple, testable, no magic. Can get verbose with many templates, but explicitness beats cleverness for generated code.

**Example:**
```csharp
public record GeneratedFile(string Path, string Content);

public interface IFileTemplate
{
    bool AppliesTo(ProjectConfig config);
    IEnumerable<GeneratedFile> Generate(ProjectConfig config);
}

public class ProjectGenerator
{
    private readonly IEnumerable<IFileTemplate> _templates;

    public IReadOnlyList<GeneratedFile> Generate(ProjectConfig config)
    {
        return _templates
            .Where(t => t.AppliesTo(config))
            .SelectMany(t => t.Generate(config))
            .ToList();
    }
}
```

### Pattern 2: Architecture as Solution Structure

**What:** Architecture patterns (Clean, Vertical Slice, Simple) don't just change folder names — they change how many .csproj files exist and how they reference each other.

**When to use:** When generating any project with an architecture selection.

**Trade-offs:** More complex than simple folder renaming, but produces accurate output.

**Example:**
```csharp
// Clean Architecture generates 4+ projects:
// MyApp.Domain/          (class library, no dependencies)
// MyApp.Application/     (class library, depends on Domain)
// MyApp.Infrastructure/  (class library, depends on Application)
// MyApp.Api/             (web project, depends on Infrastructure)

// Vertical Slice generates 1-2 projects:
// MyApp.Api/             (web project with Features/ folder)
// MyApp.Tests/           (test project)

// Simple Layered generates 1 project:
// MyApp/                 (web project with Controllers/Services/Data folders)
```

### Pattern 3: Conditional Composition (Options as Additive Layers)

**What:** Cross-cutting options (EF Core, JWT, Serilog, Docker) are additive — they layer onto whatever base project exists without changing the base.

**When to use:** For all non-architecture options (ORM, auth, observability, containers, CI).

**Trade-offs:** Keeps base templates clean. Requires careful ordering (e.g., EF Core must add its NuGet refs AND modify `Program.cs`).

**Example:**
```csharp
public class EfCoreOverlay : IFileTemplate
{
    public bool AppliesTo(ProjectConfig config) => config.Orm == OrmOption.EfCore;

    public IEnumerable<GeneratedFile> Generate(ProjectConfig config)
    {
        // Adds: DbContext class, connection string in appsettings,
        // migration setup in Program.cs, NuGet packages in .csproj
        yield return GenerateDbContext(config);
        yield return GenerateEntityConfigs(config);
    }
}
```

**Key insight:** This means `ProgramCsTemplate` must be composed from segments, not monolithic. Each overlay contributes lines to Program.cs:

```csharp
public class ProgramCsComposer
{
    public string Compose(ProjectConfig config)
    {
        var sections = new List<ProgramSection>();
        sections.Add(new BaseProgramSection(config));

        if (config.Orm == OrmOption.EfCore)
            sections.Add(new EfCoreProgramSection(config));
        if (config.Auth == AuthOption.Jwt)
            sections.Add(new JwtProgramSection(config));
        if (config.Observability.HasFlag(ObservabilityOption.Serilog))
            sections.Add(new SerilogProgramSection(config));

        return MergeSections(sections);
    }
}
```

## Data Flow

### Generation Flow

```
[User selects options in ConfigForm]
    ↓ (two-way binding)
[ProjectConfig record updated]
    ↓ (on change)
[Preview updates: file tree + CLI commands]
    ↓ (on Generate click)
[ProjectGenerator.Generate(config)]
    ↓ (returns List<GeneratedFile>)
[ZipPackager.Package(files)]
    ↓ (returns byte[])
[DownloadService.DownloadZip(bytes, fileName)]
    ↓ (JS interop)
[Browser downloads file]
```

### State Management

```
[ProjectConfig] (single record, the ONLY state)
    ↓ (drives everything)
[ConfigForm] ←→ (two-way binding) ←→ [ProjectConfig]
[ProjectPreview] ← (read-only) ← [ProjectConfig]
[CliOutput] ← (read-only) ← [ProjectConfig]
[ProjectGenerator] ← (input) ← [ProjectConfig]
```

No state management library needed. `ProjectConfig` is a single record that drives the entire app. Blazor's built-in `StateHasChanged` handles re-rendering.

### Key Data Flows

1. **Config → Preview:** When any option changes, the preview panel recalculates the expected file tree. This is lightweight (just file paths, not content).
2. **Config → CLI:** CLI command builder maps config options to `dotnet` CLI arguments. Pure string operation, no heavy computation.
3. **Config → Generate → Zip → Download:** The heavy path. Generates all file contents, writes to in-memory zip, triggers browser download.

## Handling the Combination Matrix

The biggest architectural challenge: 3 project types x 3 architectures x optional EF Core x optional JWT x optional Serilog x optional Docker = many combinations.

### Strategy: Layered Composition (NOT Template Per Combination)

```
Base Layer:     Project type determines Program.cs skeleton + .csproj SDK
Architecture:   Determines solution structure (# of projects, references)
Additive:       EF Core, JWT, Serilog, Docker, etc. each contribute independently
```

This means ~15-20 template classes, NOT 36+ full templates:

| Layer | Templates | Drives |
|-------|-----------|--------|
| Base | 3 (WebApi, MinimalApi, Console) | Program.cs skeleton, base .csproj |
| Architecture | 3 (CleanArch, VerticalSlice, Simple) | Solution structure, project references |
| Additive | ~10 (EfCore, JWT, Serilog, HealthChecks, Docker, Compose, Aspire, GitHubActions, xUnit, Testcontainers) | Extra files + Program.cs segments + NuGet refs |

Total classes: ~16-20, not 36+. Each is independently testable.

## Build Order (Suggested Phases)

1. **Config model + UI shell:** `ProjectConfig` record, MudBlazor dark theme layout, form with all options
2. **Generation engine core:** `ProjectGenerator`, `GeneratedFile`, zip packaging, download interop
3. **Base project templates:** 3 project types with Simple Layered architecture (smallest matrix first)
4. **Architecture overlays:** Clean Architecture and Vertical Slice solution structures
5. **Additive options:** EF Core, JWT, Serilog, health checks, Docker, CI, testing
6. **CLI equivalent output:** `CliCommandBuilder` that maps config to `dotnet` commands
7. **Polish:** Preview panel, deployment pipeline, loading UX

## Anti-Patterns

### Anti-Pattern 1: Template Per Combination

**What people do:** Create a full template for every combination (WebApi+CleanArch+EfCore+JWT, WebApi+CleanArch+EfCore+NoAuth, ...).
**Why it's wrong:** Exponential growth. Adding one new option doubles the template count.
**Do this instead:** Composable layers where each option contributes independently.

### Anti-Pattern 2: Monolithic Program.cs Template

**What people do:** One giant `if/else` chain generating the entire `Program.cs`.
**Why it's wrong:** Becomes unmaintainable past 4-5 options. Each option interleaves with others.
**Do this instead:** Section-based composition where each option contributes ordered segments.

### Anti-Pattern 3: File System Abstraction in WASM

**What people do:** Try to create a virtual file system or use WASI-like file operations.
**Why it's wrong:** Unnecessary complexity. Generated files are just strings destined for a zip.
**Do this instead:** `List<GeneratedFile>` where `GeneratedFile` is a `(path, content)` tuple.

## Sources

- Architectural analysis of PROJECT.md requirements and constraints
- Training data on Spring Initializr architecture (well-documented open source)
- Training data on Blazor WASM application patterns
- Composable template engine patterns from code generation tools

---
*Architecture research for: .NET project generator (Blazor WASM)*
*Researched: 2026-02-26*
