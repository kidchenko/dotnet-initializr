# Technology Stack

**Project:** NetStarter — Blazor WASM .NET Project Generator
**Researched:** 2026-02-26
**Confidence note:** Web search and WebFetch tools were unavailable during this research session. Findings are based on training data current to August 2025 plus the existing project files. All version numbers should be verified against NuGet before locking.

---

## Recommended Stack

### Core Framework

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| .NET 10 | 10.0 (RC/Preview as of research date) | Runtime for the Blazor WASM host app | Already in use per `NetStarter.csproj`; targets the latest LTS-adjacent release; generates projects for .NET 8/9/10 |
| Blazor WebAssembly | 10.0.x | SPA framework running in browser | Matches project constraint: zero backend, static hosting, browser-only execution; already scaffolded |
| Microsoft.AspNetCore.Components.WebAssembly | 10.0.2 | Blazor WASM host runtime | Already referenced in the csproj; keep at parity with .NET SDK |

**Confidence: MEDIUM** — .NET 10 was in RC at training cutoff; the existing csproj already pins 10.0.2, so this is established, not speculative.

---

### UI Component Library

**Recommendation: MudBlazor**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| MudBlazor | 7.x (latest stable) | Full UI component library | Best dark-theme support out of the box; Material Design system with first-class dark mode palette; large community (35k+ GitHub stars); actively maintained; designed for Blazor from the ground up — not a JS wrapper |

**Why MudBlazor over alternatives:**

- **vs Radzen Blazor:** Radzen has a free tier but its component depth and dark theme are weaker than MudBlazor. Radzen's data-grid focus doesn't add value here; NetStarter needs forms, chips, toggles, and a clean layout — all MudBlazor strengths.
- **vs Ant Design Blazor:** AntD Blazor follows Ant's light-first design language; dark mode is secondary and the theming API is more complex. Community size is smaller.
- **vs Fluent UI Blazor (Microsoft):** Microsoft-backed and solid, but the Fluent design aesthetic is Windows-app-flavored. Spring Initializr-style requires a clean web aesthetic with strong dark defaults; MudBlazor achieves this.
- **vs Blazorise:** Capable but requires an explicit CSS framework underneath (Bootstrap, Bulma). Adds complexity without benefit for a single-page UI.
- **vs raw Tailwind CSS + headless:** Feasible but requires significant hand-rolled component work. Not worth it for a utility app — MudBlazor ships all the form widgets needed.

**Confidence: MEDIUM** — MudBlazor 7.x was the current stable series at training cutoff (mid-2025). Verify the exact version on NuGet before locking.

---

### Client-Side Zip Generation

**Recommendation: System.IO.Compression (built-in BCL)**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| System.IO.Compression | Built into .NET 10 BCL | Create `.zip` in memory | Available in Blazor WASM since .NET 5; no additional NuGet dependency; `ZipArchive` + `MemoryStream` produces a byte array that can be pushed to browser download via JS interop |

**How it works in WASM:**

```csharp
// Create zip in memory
using var ms = new MemoryStream();
using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
{
    var entry = zip.CreateEntry("src/MyProject/Program.cs");
    using var writer = new StreamWriter(entry.Open());
    await writer.WriteAsync(generatedProgramCs);
}

// Trigger download via JS interop
var bytes = ms.ToArray();
await JSRuntime.InvokeVoidAsync("downloadFile", "MyProject.zip", "application/zip", bytes);
```

**JS interop shim needed (in `wwwroot/index.html` or a `.js` file):**

```javascript
window.downloadFile = (fileName, contentType, bytes) => {
    const blob = new Blob([new Uint8Array(bytes)], { type: contentType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
};
```

**Why not a third-party library:**

- `SharpZipLib`, `DotNetZip`, and `ICSharpCode.SharpZipLib` all work in WASM but introduce dependency overhead.
- `System.IO.Compression.ZipArchive` covers everything needed: nested folder entries, text file entries, binary entries. No external library required.

**Confidence: HIGH** — `System.IO.Compression` has been available and tested in Blazor WASM since .NET 5. The JS interop pattern for file download is well-established. No verification needed beyond confirming the existing .NET 10 target.

---

### Template / String Generation Engine

**Recommendation: Plain C# string interpolation + a lightweight builder pattern (no external engine)**

| Approach | Version | Purpose | Why |
|----------|---------|---------|-----|
| C# string interpolation + `StringBuilder` | N/A (language feature) | Generate `.csproj`, `Program.cs`, `appsettings.json`, etc. | No runtime dependency; full C# control; easy unit-testing; no WASM-incompatible reflection or file I/O required |

**Pattern to use — typed template classes:**

```csharp
public class ProgramCsTemplate
{
    public string Generate(ProjectConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");

        if (config.Auth == AuthOption.Jwt)
        {
            sb.AppendLine();
            sb.AppendLine("builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)");
            sb.AppendLine("    .AddJwtBearer(options => { /* ... */ });");
        }

        sb.AppendLine();
        sb.AppendLine("var app = builder.Build();");
        sb.AppendLine("app.Run();");
        return sb.ToString();
    }
}
```

**Why not a template engine (Scriban, Liquid, Handlebars):**

- **Scriban / Fluid / Liquid:** These work in WASM and are solid choices, BUT the templates for this app are C# code — using a text template language to generate C# produces a confusing double-layer of escaping (`{` and `}` need escaping in both C# and the template syntax). This creates maintainability pain.
- **T4 templates:** T4 is a build-time code generation tool. It does not run at runtime in WASM at all.
- **Razor-based generation:** Blazor's Razor is for UI rendering, not code output. Using `RenderTreeBuilder` to generate source text is a misuse of the abstraction.
- **Scriban is acceptable if complexity grows:** If the number of templates grows beyond ~15-20 files and branching logic becomes hard to manage in C# classes, Scriban (with its simple `{{ }}` syntax) is the best alternative engine — it supports WASM, has no reflection-heavy dependencies, and is actively maintained. Add it in v2 if needed.

**Confidence: HIGH** — This is an architectural decision not dependent on an external library. The rationale is sound and does not require version verification.

---

### Dark Theme / Styling

**Recommendation: MudBlazor built-in dark theme (no separate CSS framework)**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| MudBlazor ThemeProvider + dark palette | 7.x | Dark theme application | MudBlazor ships `MudThemeProvider` with a `IsDarkMode` toggle and full palette customization; no additional CSS framework needed; Spring Initializr uses a simple dark background with accent colors — achievable with MudBlazor's default dark theme |

**Theme setup:**

```razor
<MudThemeProvider @bind-IsDarkMode="@_isDarkMode" Theme="_theme" />

@code {
    bool _isDarkMode = true; // Default to dark
    MudTheme _theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#6366f1",   // indigo accent
            Background = "#0f172a", // slate-900 equivalent
            Surface = "#1e293b",    // slate-800
        }
    };
}
```

**Why not Tailwind CSS:**

- Adding Tailwind requires a build pipeline (Node.js, PostCSS). For a pure .NET WASM project deployed as static files, this adds toolchain complexity and a non-.NET CI step.
- MudBlazor covers all UI needs (form inputs, chips, toggles, layout, responsive grid) without Tailwind.
- If a designer-driven CSS system becomes important in v2, Tailwind can be layered in, but it's not justified for v1.

**Confidence: MEDIUM** — MudBlazor's ThemeProvider API was stable at training cutoff. Verify palette property names (`PaletteDark`) against the MudBlazor 7.x docs, as the theming API has evolved across major versions.

---

### Static Site Deployment

**Recommendation: GitHub Pages (primary) + Azure Static Web Apps (secondary)**

| Platform | Purpose | Why |
|----------|---------|-----|
| GitHub Pages | Primary deployment target | Zero cost; direct integration from the repository; Blazor WASM deploys as static files with a `404.html` redirect workaround for client-side routing; widely used for FOSS tools |
| Azure Static Web Apps | Secondary / enterprise deployment | Free tier available; native support for Blazor WASM (handles `_framework/` path correctly); built-in GitHub Actions integration; global CDN |

**GitHub Pages configuration note:**

Blazor WASM on GitHub Pages requires a routing workaround because GitHub Pages serves 404 for deep-link routes. Standard approach:

1. Copy `index.html` to `404.html` in the publish output.
2. Add a small script to `index.html` that restores the path from `?/` redirect format.

This is a well-documented pattern and should be part of the CI/CD pipeline.

**GitHub Actions workflow:**

```yaml
- name: Publish Blazor WASM
  run: dotnet publish NetStarter/NetStarter/NetStarter.csproj -c Release -o publish

- name: Deploy to GitHub Pages
  uses: peaceiris/actions-gh-pages@v4
  with:
    github_token: ${{ secrets.GITHUB_TOKEN }}
    publish_dir: ./publish/wwwroot
```

**Confidence: HIGH** — GitHub Pages + Blazor WASM is a well-established pattern with documented solutions for the routing problem. No version-sensitive dependencies.

---

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| MudBlazor | 7.x | All UI components, layout, theming | Required from Day 1 |
| System.IO.Compression | BCL (no NuGet) | In-browser zip creation | Required for the Generate button |
| Microsoft.AspNetCore.Components.WebAssembly | 10.0.x | WASM runtime, JS interop | Already in project |
| Scriban | 5.x | Template engine (optional upgrade) | Only if C# template classes become unwieldy in v2 |

---

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| UI Library | MudBlazor | Radzen Blazor | Weaker dark theme; data-grid focus not needed |
| UI Library | MudBlazor | Ant Design Blazor | Light-first design system; dark mode secondary |
| UI Library | MudBlazor | Fluent UI Blazor | Windows-app aesthetic doesn't fit web tool |
| UI Library | MudBlazor | Blazorise | Requires additional CSS framework underneath |
| UI Library | MudBlazor | Tailwind + headless | Too much manual component work for a utility app |
| Zip generation | System.IO.Compression | SharpZipLib | Unnecessary external dependency; BCL covers everything |
| Zip generation | System.IO.Compression | JSZip (JS) | Would require shipping a JS library and crossing the JS/WASM boundary for the entire operation; defeats purpose |
| Template engine | C# StringBuilder | Scriban | Viable alternative for v2; adds dependency for v1 with no benefit |
| Template engine | C# StringBuilder | T4 | Build-time only; cannot run at WASM runtime |
| Template engine | C# StringBuilder | Liquid/Fluid | Same escaping confusion issue as Scriban for v1 |
| Hosting | GitHub Pages | Netlify | GitHub Pages is free and directly tied to the repo; no additional service needed |
| Hosting | GitHub Pages | Vercel | Same reasoning; GitHub Pages is simpler for an open-source .NET tool |

---

## Installation

Add MudBlazor to the existing project:

```bash
# Add MudBlazor (run from NetStarter/NetStarter/)
dotnet add package MudBlazor

# No other NuGet packages needed for core functionality
# System.IO.Compression is part of the BCL
```

**In `_Imports.razor` add:**

```razor
@using MudBlazor
```

**In `Program.cs` add:**

```csharp
builder.Services.AddMudServices();
```

**In `wwwroot/index.html` add in `<head>`:**

```html
<link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
```

**And before `</body>`:**

```html
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

---

## Version Verification Checklist

Before development begins, verify these on NuGet.org:

- [ ] `MudBlazor` latest stable version and .NET 10 compatibility
- [ ] `Microsoft.AspNetCore.Components.WebAssembly` 10.0.x release status (.NET 10 GA date)
- [ ] Confirm `System.IO.Compression.ZipArchive` entry naming behavior in WASM (nested paths with `/` separator)
- [ ] MudBlazor `PaletteDark` property names match 7.x API (check https://mudblazor.com/customization/theming)

---

## Sources

- Existing project file: `NetStarter/NetStarter/NetStarter.csproj` (confirmed .NET 10, 10.0.2 package versions)
- Training data: MudBlazor 7.x ecosystem knowledge (confidence: MEDIUM — verify versions)
- Training data: `System.IO.Compression` in Blazor WASM (confidence: HIGH — stable since .NET 5)
- Training data: GitHub Pages + Blazor WASM routing workaround (confidence: HIGH — widely documented)
- Training data: String template approach rationale (confidence: HIGH — architectural decision)

**Note:** WebSearch and WebFetch tools were unavailable during this research session. All version numbers should be cross-checked against NuGet.org before implementation. The architectural decisions (no backend, BCL zip, C# template classes) are independent of version concerns and carry HIGH confidence.
