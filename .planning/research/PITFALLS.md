# Domain Pitfalls

**Domain:** .NET project generator / scaffolding tool (Blazor WASM client-side)
**Researched:** 2026-02-26
**Confidence:** HIGH (GitHub Pages/hosting pitfalls from official MS docs, MEDIUM on template maintenance from training data + community patterns)

---

## Critical Pitfalls

Mistakes that cause rewrites or major user-visible failures.

---

### Pitfall 1: Template Combinatorial Explosion Without Abstraction

**What goes wrong:** Each template option (project type x architecture x ORM x auth x observability) multiplies the number of code paths that must be correct. For 3 project types x 3 architectures x 2 ORMs x 2 auth modes = 36 combinations minimum, and that's before testing setup and CI/CD. Without a composable, data-driven template model from day one, the generation logic becomes a nest of `if/else` blocks that is impossible to test exhaustively and even harder to maintain when .NET 11 ships.

**Why it happens:** Builders start with one combination ("Web API + Clean Architecture") and add branches rather than designing a composition model up front. The first few additions feel fast; by the fifth the logic is unmaintainable.

**Consequences:** Silent bugs in rarely-tested combinations (e.g., Worker Service + Vertical Slice + JWT), generated projects that compile but behave incorrectly, template authors afraid to add new options because every addition could silently break an existing path.

**Prevention:**
- Model the project as a configuration object (a plain C# record) rather than scattered boolean flags.
- Write a unit-testable pure function `Generate(config) -> Dictionary<string, string>` where the dictionary key is the file path and value is the file content.
- Test every combination in CI using xUnit theory data, not manual smoke testing.
- Build one combination end-to-end first, validate the generated project compiles and runs, then add the second option as a variation.

**Detection:** More than 3 nested `if` blocks in any template builder function. Generated code that compiles but has unreachable code or inconsistent namespace references. Any option added in "10 minutes" that was not followed by a full matrix test.

**Phase:** Address in Phase 1 (template engine design). Non-negotiable before adding second project type or second architecture.

---

### Pitfall 2: Generated Code That Compiles But Violates Its Own Architecture

**What goes wrong:** A "Clean Architecture" generated project that puts domain logic in the web layer, or a "Vertical Slice" project that contains horizontal service-layer abstractions. The generated project compiles and runs, but users who chose that architecture get scaffolding that defeats the purpose and teaches bad patterns.

**Why it happens:** Template authors are often less familiar with the architecture than with the technology stack. The template is built to "look right" rather than to enforce architectural constraints. Clean Architecture in particular has no standard .NET template, so authors invent their own (often incorrect) interpretation.

**Consequences:** Users trust the generator and follow the generated pattern. A bad Clean Architecture scaffold produces projects with wrong dependencies for months before someone notices. Reputation damage — "NetStarter generates wrong code."

**Prevention:**
- Before implementing each architecture template, build a hand-written reference project in that architecture that compiles, runs, and passes tests. Use that as the source of truth for template content.
- For Clean Architecture: Domain project must have zero references to Infrastructure or Application (enforce with ArchUnit or architecture tests in the generated project itself).
- For Vertical Slice: Generated project should have feature folders, not horizontal layers. Test this by checking the folder structure output.
- Include at minimum one integration test or architecture test in the generated project that validates the architecture at runtime.

**Detection:** Generate a project, run `dotnet build`, then grep for cross-layer references that violate the chosen architecture. If the generated tests pass a wrong dependency, the template is wrong.

**Phase:** Phase 1 for initial architecture templates. Each new architecture option needs this validation before release.

---

### Pitfall 3: WASM Bundle Size Bloat from Unnecessary NuGet Packages

**What goes wrong:** The NetStarter app itself (the generator UI) pulls in NuGet packages for zip generation, template building, or UI that are not trimmer-friendly. Blazor WASM performs IL trimming on publish, but packages that use reflection, dynamic invocation, or `System.Type` lookups at runtime can either (a) be broken by trimming or (b) force trimmer exclusions that balloon the published bundle.

**Why it happens:** Developers add NuGet packages without checking WASM compatibility or trimmer annotations. `System.IO.Compression` is WASM-safe and already in the BCL, but third-party zip libraries, logging frameworks (e.g., Serilog with its sink ecosystem), or full-fat DI containers can introduce 500KB–2MB of non-trimmable IL.

**Consequences:** Initial load time of 3–8 seconds on a good connection (Blazor WASM uncompressed is already 5–10MB for a minimal app). Users bounce before the generator loads. The irony of a project-generator that itself has a bad first experience is corrosive to trust.

**Prevention:**
- Use `System.IO.Compression.ZipArchive` from the BCL — it is already available in WASM and is trimmer-friendly. Do not add a third-party zip library.
- Prefer pure string manipulation for template generation (no Razor Engine, no Handlebars.NET, no T4). String interpolation and `StringBuilder` have zero overhead.
- Check every NuGet package against the `IsTrimmable` metadata before adding it. Packages that ship with `<IsTrimmable>true</IsTrimmable>` in their props are safe.
- Measure bundle size on every PR using `dotnet publish -c Release` and check the `wwwroot/_framework/` folder size. Set a size budget (target: under 8MB compressed).
- Use `<BlazorWebAssemblyEnableLinking>true</BlazorWebAssemblyEnableLinking>` (the default) and never disable it to work around trimmer errors — fix the trimmer errors instead.

**Detection:** `dotnet publish -c Release` output shows the `_framework/` folder. Watch for any DLL over 500KB that is not a BCL assembly. Check `blazor.boot.json` for unexpected entries.

**Phase:** Phase 1 (before adding any NuGet packages beyond the baseline). Revisit every time a new package is added.

---

### Pitfall 4: GitHub Pages Routing Breaks on Deep Links and Refresh

**What goes wrong:** GitHub Pages is a static host. When a user navigates directly to a URL like `https://user.github.io/netstarter/` (or any non-root path after the repo name), GitHub Pages returns 404 because there is no physical file at that path. The Blazor WASM router never gets a chance to handle it.

**Why it happens:** SPA routing requires all paths to serve `index.html`. GitHub Pages does not support URL rewriting natively. This is documented but easily missed because it works fine on localhost.

**Consequences:** Any user who bookmarks a URL, shares a link, or refreshes the browser after navigation gets a 404. For a tool used by developers, this is severely trust-damaging.

**Prevention:**
- Add a `wwwroot/404.html` file that contains a JavaScript redirect script (the `rafrex/spa-github-pages` pattern). The 404 page encodes the path into a query string and redirects to `index.html`, which then reads the query string and restores the original path using the History API.
- In `wwwroot/index.html`, add the corresponding script that reads the redirected query string and restores the route.
- Configure the `<base href>` tag correctly for the GitHub Pages sub-path (e.g., `/netstarter/` not `/`). Use the `ghaction-rewrite-base-href` GitHub Action to inject this automatically at deploy time.
- Add a `.nojekyll` file to the repository root so Jekyll does not skip the `_framework` folder (which starts with underscore).

**Detection:** Deploy to a non-root GitHub Pages URL and try refreshing the page or navigating directly to a path. 404 = the fix is missing.

**Phase:** Phase 1 (CI/CD and deployment setup). Must be correct before the first public deployment.

---

### Pitfall 5: Git Line-Ending Changes Break Blazor Integrity Checks

**What goes wrong:** Git normalizes JavaScript file line endings (CRLF to LF or vice versa) during checkout on Windows. Blazor WASM ships with integrity hashes for its JS files baked into `blazor.boot.json`. When Git changes `blazor.webassembly.js` line endings, the file hash no longer matches, and the browser refuses to load the app with an integrity check failure.

**Why it happens:** The standard `.gitattributes` for .NET projects does not mark JS files as binary. Git treats them as text and applies EOL normalization.

**Consequences:** The deployed app fails to load in the browser with a console error about integrity check failure. This is invisible to the developer on the machine that built and committed the files, and only appears in CI or on other machines.

**Prevention:**
- Add `*.js binary` to `.gitattributes` before committing any project files. This must be done before the first commit of the `wwwroot` folder.
- If the project already has committed JS files without this setting: remove them from the Git index (`git rm --cached`), add the `.gitattributes` line, and recommit.

**Detection:** Deploy the app and check the browser console for `Failed to find a valid digest` or `integrity` errors. If present, the `.gitattributes` fix is missing.

**Phase:** Phase 1 (project setup). Must be in place before the first CI deployment.

---

## Moderate Pitfalls

---

### Pitfall 6: Version Drift Between Generator Output and Actual .NET SDK Capabilities

**What goes wrong:** NetStarter generates code targeting .NET 8, 9, or 10. When .NET 11 ships (or when a new SDK patch changes a template API), the generated output may use patterns that are deprecated, broken, or subtly wrong. Example: `Program.cs` patterns for minimal API changed significantly between .NET 6 and .NET 8. EF Core migration patterns change between major versions. Health check middleware registration changed in .NET 8.

**Why it happens:** Template strings are hardcoded in the generator. There is no mechanism to pull from the actual SDK — the tool is intentionally disconnected from the CLI. Version drift is inevitable without a maintenance process.

**Consequences:** Users generate a project for .NET 10, get deprecation warnings or build errors, lose trust in the tool. The tool becomes "technically correct for the .NET version it was built for" which is fine at launch but degrades over 12–18 months.

**Prevention:**
- Version-stamp every template internally. Add a comment at the top of key generated files: `// Generated by NetStarter - template version 1.0 for .NET {version}`.
- Pin NuGet package versions in generated output to the specific SDK version selected (e.g., EF Core 8.x for .NET 8 projects, EF Core 9.x for .NET 9 projects).
- Create a smoke test matrix in CI: for each supported SDK version, generate a project, run `dotnet build`, and verify it succeeds. This catches version drift within hours of a new SDK patch.
- Document which SDK versions are supported and when templates were last validated.

**Detection:** New .NET SDK ships, no CI smoke test exists, users file bugs. The absence of a smoke test matrix is the warning sign.

**Phase:** Phase 2 (after core template engine is working). Smoke test matrix is a prerequisite for claiming multi-version support.

---

### Pitfall 7: Client-Side Code Exposure of Generated Template Logic

**What goes wrong:** All template generation logic ships to the browser as part of the WASM bundle. Users can inspect the `.dll` files loaded by Blazor, decompile them, and read the full template strings. This is inherent to client-side WASM — there is no server-side code to protect.

**Why it happens:** This is not a bug, it is a characteristic of the architecture. The pitfall is failing to acknowledge it and then making decisions based on false assumptions of secrecy.

**Consequences:** Template logic cannot be proprietary. Any "secret sauce" in template generation is inspectable. This is actually fine for an open-source tool — but it would be a mistake to add API keys, secrets, or business logic that should be server-side into the WASM bundle.

**Prevention:**
- Never put API keys, license keys, or secrets in the WASM app. All secrets must stay server-side (and there is no server for this project).
- Treat the WASM bundle as fully public source code even if it is not open-sourced. Design accordingly.
- Lean into open source: publish the template logic on GitHub. Transparency is a feature.

**Detection:** If you find yourself thinking "users won't be able to see this code because it's WASM" — that is the warning sign. They can.

**Phase:** Phase 1 (architectural decision). Not a bug to fix, a constraint to accept.

---

### Pitfall 8: Missing Conditional UI Validation Leads to Invalid Combinations

**What goes wrong:** The UI shows options in sequence (e.g., ORM selection, then database selection), but the generated output does not validate that the combination is coherent. Example: user selects "None" for ORM, then somehow a database is still included in the generated code. Or: user selects "JWT auth" for a Console project type where JWT auth makes no sense.

**Why it happens:** UI conditional logic (`@if (config.Orm == OrmType.EfCore)`) and template generation logic are separate code paths. A developer writes one but forgets to enforce the constraint in the other.

**Consequences:** Generated projects that have conflicting configuration (e.g., EF Core context wired up with no database selected, or auth middleware registered in a project with no HTTP pipeline). These compile but fail at runtime.

**Prevention:**
- Build a `ProjectConfig` record with a `Validate()` method that enforces all business rules (e.g., `Database must be specified when ORM is EfCore`, `JWT auth is only valid for Web API and Minimal API project types`).
- Run `Validate()` before generation, not just before displaying options. The UI showing correct options does not guarantee correct config if the user manipulates state programmatically.
- Write unit tests for `Validate()` covering all invalid combination rules.

**Detection:** Unit test the `Validate()` method. Add an integration test that passes an invalid config directly to the generator and asserts it throws or returns an error rather than generating output.

**Phase:** Phase 1 (template engine design). Validation logic should be built alongside the first conditional options.

---

### Pitfall 9: Blazor WASM Cold Start Time UX Neglect

**What goes wrong:** First load of a Blazor WASM app downloads the .NET runtime, all application DLLs, and ICU data (internationalization). Even with compression, this can be 2–6MB for a minimal app and takes 1–4 seconds on a fast connection, 5–15 seconds on a mobile connection. If the loading experience shows a blank page or a generic spinner, users assume the tool is broken.

**Why it happens:** Developers build and test on localhost where the WASM bundle is cached after the first load. The cold-start experience is never tested on a real connection.

**Consequences:** High bounce rate from first-time visitors who hit the tool cold. The tool's value is "instant project generation" but the experience contradicts that if loading takes 8 seconds with no progress indicator.

**Prevention:**
- Customize the Blazor loading UI in `wwwroot/index.html` to show meaningful progress with the Spring Initializr visual style (dark theme, progress bar, or spinner matching the app aesthetic).
- Enable Brotli compression (already on by default in `dotnet publish`). GitHub Pages requires explicit client-side Brotli decompression — use the `decode.js` approach documented by Microsoft.
- Consider using ICU data sharding: `<BlazorWebAssemblyLoadAllGlobalizationData>false</BlazorWebAssemblyLoadAllGlobalizationData>` to reduce the ICU data downloaded (since this tool is English-only).
- Set `EmccMaximumHeapSize` appropriately if targeting Safari on iOS (default 2GB heap can crash iOS Safari).

**Detection:** Test cold load on a 4G mobile connection using Chrome DevTools network throttling. If the spinner shows for more than 3 seconds without any visible progress feedback, the UX needs improvement.

**Phase:** Phase 1 (before first public deployment) for loading indicator. Phase 2 for compression optimization.

---

### Pitfall 10: Generated Namespace Collisions with Reserved Keywords or Common Names

**What goes wrong:** The user types a project name like "System", "Microsoft", "API", "Test", or "Default". The generated `Program.cs` and `.csproj` use this as the root namespace. This creates build errors (e.g., `namespace System` conflicts with BCL) or runtime errors that are extremely confusing to debug.

**Why it happens:** No input validation on the project name field. The generator trusts the user to provide a valid C# namespace identifier.

**Consequences:** Users get a generated project that does not compile, with cryptic errors like "ambiguous reference between 'System.Console' and 'System.Console'" or the csproj fails to resolve NuGet packages.

**Prevention:**
- Validate project name against a blocklist of reserved C# namespaces (`System`, `Microsoft`, `Windows`, `Newtonsoft`, `Azure`, `AWS`).
- Validate that the name is a valid C# identifier: starts with a letter or underscore, contains only letters, digits, dots, and underscores, no spaces.
- Show inline validation error in the UI before allowing generation.
- Sanitize the name in the generator even after UI validation as a defense-in-depth measure.

**Detection:** Try generating with project name "System" or "API" or a name with spaces. If generation proceeds without error, validation is missing.

**Phase:** Phase 1 (before first user-facing release).

---

## Minor Pitfalls

---

### Pitfall 11: Missing `.nojekyll` Causes `_framework` Assets to 404 on GitHub Pages

**What goes wrong:** GitHub Pages uses Jekyll by default, which skips directories starting with underscore. The Blazor WASM framework assets are in `wwwroot/_framework/`. Without a `.nojekyll` file, these assets return 404 and the app fails to load entirely.

**Prevention:** Add a `.nojekyll` file (empty file) to the GitHub Pages source root. Include this in the CI/CD deploy step, not as a manual step.

**Detection:** Deploy without `.nojekyll`, check the browser console for 404 errors on `_framework/blazor.webassembly.js`.

**Phase:** Phase 1 (CI/CD setup).

---

### Pitfall 12: Zip File Naming and Content Path Structure in Generated Archive

**What goes wrong:** `System.IO.Compression.ZipArchive` creates zip files, but the paths inside the archive matter. Users expect the zip to extract into a single folder named after their project (like Maven/Gradle archives from Spring Initializr). If files are added with paths like `Program.cs` instead of `MyProject/Program.cs`, extracting creates a mess of files in the current directory.

**Prevention:** Always prefix all file paths in the zip with `{ProjectName}/`. Test the generated zip by extracting it and verifying the folder structure.

**Detection:** Generate a zip, extract it. If files land in the current directory instead of a named subfolder, the path prefix is missing.

**Phase:** Phase 1 (zip generation implementation).

---

### Pitfall 13: `System.IO.Compression` ZipArchive Dispose Must Be Called Before Returning Stream

**What goes wrong:** `ZipArchive` buffers zip content and finalizes the central directory when `Dispose()` is called. If you return the underlying `MemoryStream` before calling `Dispose()` on the `ZipArchive`, the stream contains an incomplete (corrupt) zip file.

**Prevention:** Always use `using` blocks or call `Dispose()` before returning the byte array. Return a copy of the stream contents after dispose: `using var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true); ... archive entries ... // archive disposed here ms.ToArray()`.

**Detection:** Generate a zip and attempt to open it with a zip tool. "End of central directory record not found" = dispose was not called before returning.

**Phase:** Phase 1 (zip generation implementation).

---

### Pitfall 14: Service Worker Caching Breaks Deployed Updates on GitHub Pages

**What goes wrong:** The existing project scaffolding already includes a service worker (`service-worker.js` and `service-worker.published.js`). Service workers cache the app aggressively. When a new version is deployed, returning users continue to run the old version from the service worker cache and never see updates unless they manually clear the cache or the service worker is correctly versioned.

**Why it happens:** The default Blazor WASM service worker uses a `CACHE_NAME` that does not change between deployments. The browser happily serves the old cached version indefinitely.

**Prevention:**
- In `service-worker.published.js`, the `CACHE_NAME` must change on every deployment. The simplest approach: use the build timestamp or a semantic version string as part of the cache name.
- Test cache update behavior: deploy v1, open app, deploy v2, reload — verify v2 is served within one reload cycle.
- Consider whether a service worker is needed at all for a tool (vs. a progressive web app). If offline support is not required, removing the service worker simplifies the deployment story.

**Detection:** Deploy an update, reload the browser without clearing cache, check which version is running.

**Phase:** Phase 2 (deployment polish). Not blocking for MVP if the service worker is removed; critical if kept.

---

### Pitfall 15: Dark Theme Accessibility — Low Contrast Ratios

**What goes wrong:** Dark themes are easy to implement with low contrast ratios that pass a quick visual check but fail WCAG AA (4.5:1 for normal text). Low contrast on input labels, placeholder text, or error messages makes the tool unusable for users with visual impairments.

**Prevention:**
- Use a contrast checker during UI development. Minimum 4.5:1 for normal text, 3:1 for large text.
- Spring Initializr's dark theme uses high-contrast labels — match or exceed that baseline.

**Detection:** Run the Chrome Accessibility Audit. Any "Insufficient color contrast" failures are the warning sign.

**Phase:** Phase 1 (UI implementation), checked before first public release.

---

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| Template engine design | Combinatorial explosion without abstraction (Pitfall 1) | Pure function generator with config object and unit test matrix |
| Architecture templates | Generated code that violates its own architecture (Pitfall 2) | Hand-written reference project per architecture before templating |
| NuGet package selection | Bundle size bloat from non-trimmable packages (Pitfall 3) | BCL-only approach; measure publish size on every PR |
| GitHub Pages deployment | Deep-link routing 404 (Pitfall 4) | 404.html redirect + base href + .nojekyll before first deploy |
| First commit / git setup | Integrity check failures from EOL normalization (Pitfall 5) | `.gitattributes *.js binary` before first commit |
| Input handling | Reserved namespace / invalid C# identifier (Pitfall 10) | Blocklist + regex validation before generation |
| Zip generation | Corrupt zip or wrong folder structure (Pitfalls 12, 13) | Unit test zip extraction; always dispose before returning bytes |
| Multi-SDK support | Version drift in generated output (Pitfall 6) | CI smoke test: generate + `dotnet build` per SDK version |
| UX/Loading | Blank screen during cold start (Pitfall 9) | Custom loading UI in index.html; Brotli + ICU sharding |
| Auth in generated code | Auth on project types where it makes no sense (Pitfall 8) | Validate() method with unit tests for all invalid combinations |
| Service worker | Cached old version never updates (Pitfall 14) | Version the cache name or remove the service worker for MVP |

---

## Sources

- Microsoft Docs: Host and deploy Blazor WASM — GitHub Pages (updated 2026-02-24): https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/github-pages
- Microsoft Docs: Host and deploy Blazor WASM standalone (compression, webcil, routing): https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/
- Microsoft Docs: Secure ASP.NET Core Blazor WASM (client-side security constraints): https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/
- Microsoft Docs: Blazor performance best practices (AOT, bundle, metrics): https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/
- Microsoft Docs: Custom templates for dotnet new: https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates
- SteveSandersonMS/ghaction-rewrite-base-href GitHub Action (base href for GitHub Pages deployments)
- Training data: .NET project generator patterns, Spring Initializr design precedents, Blazor WASM community known issues (MEDIUM confidence — patterns corroborated by official docs where cited)
