---
phase: 11-code-preview-and-responsive-design
plan: 01
subsystem: ui
tags: [blazor, highlight.js, code-preview, modal, file-tree, javascript-modules, lazy-loading]

# Dependency graph
requires:
  - phase: 09-background-jobs
    provides: Home.razor structure and FileTreePreview component with ProjectGenerationService wiring
  - phase: 10-url-serialization
    provides: OnConfigChanged pattern for config mutation events
provides:
  - CodePreviewModal.razor component with lazy highlight.js ESM loading and VS2015 dark theme
  - CodePreviewModal.razor.js collocated JS module with line number injection
  - FileTreePreview.razor with OnFileSelected EventCallback and folder toggle click handlers
  - Home.razor with generated files cache, file selection handler, and modal wiring
affects: [phase-12, responsive-design, code-preview, file-tree-interaction]

# Tech tracking
tech-stack:
  added:
    - highlight.js 11.11.1 (ESM CDN import via dynamic import(), not bundled)
    - VS2015 CSS theme from cdnjs (injected once on first modal open)
  patterns:
    - Collocated Blazor JS module (.razor.js) served at /Components/ComponentName.razor.js
    - Lazy JS module loading via IJSObjectReference with IAsyncDisposable cleanup
    - Generated files dictionary caching with null invalidation on config change
    - Two-way bind-IsOpen pattern for modal open/close state

key-files:
  created:
    - src/NetStarter/Components/CodePreviewModal.razor
    - src/NetStarter/Components/CodePreviewModal.razor.js
  modified:
    - src/NetStarter/Components/FileTreePreview.razor
    - src/NetStarter/Components/FileTreePreview.razor.css
    - src/NetStarter/Pages/Home.razor

key-decisions:
  - "Collocated JS file served at /Components/CodePreviewModal.razor.js (not _content/AssemblyName/...) in standalone Blazor WASM"
  - "Modal is always in DOM (opacity/pointer-events toggle) not conditionally rendered, to preserve JS module reference across opens"
  - "Generated files cached in _generatedFiles nullable dict, invalidated by setting to null in OnConfigChanged before PushStateToUrl"
  - "Line numbers injected by JS module post-highlight via span.line/span.line-number wrapping — CSS counter approach not used"

patterns-established:
  - "Lazy JS module load pattern: EnsureHighlightJsLoaded() called on OnAfterRenderAsync when IsOpen transitions from false to true"
  - "File icon resolution: filename-first check (Dockerfile), then extension switch with colored inline style"

requirements-completed: [PREV-01, PREV-02, PREV-03, PREV-04, RESP-03]

# Metrics
duration: 4min
completed: 2026-03-01
---

# Phase 11 Plan 01: Code Preview Modal Summary

**Code preview modal with lazy-loaded highlight.js ESM, VS2015 dark theme, line numbers, and clickable file tree nodes wired to Home.razor**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-01T18:58:16Z
- **Completed:** 2026-03-01T19:02:21Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- CodePreviewModal.razor component with IAsyncDisposable, lazy JS loading, Escape/backdrop/X close, desktop 80% centered + mobile full-screen (RESP-03)
- CodePreviewModal.razor.js collocated module dynamically imports highlight.js 11.11.1 from cdnjs CDN, injects VS2015 CSS once, adds line numbers
- FileTreePreview.razor updated with OnFileSelected EventCallback, folder toggle (open/closed icon), file-type icons by extension (Docker, C#, JSON, XML, YAML, etc.)
- Home.razor: generated files dictionary cached and invalidated on config change, modal wired with bind-IsOpen/FilePath/FileContent
- All 235 tests pass (220 prior + 15 Phase 11 language detection/content lookup tests)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create CodePreviewModal component and JS module** - `90db1e5` (feat)
2. **Task 2: Wire file tree click handlers and connect modal in Home.razor** - `1e32c89` (feat)

## Files Created/Modified
- `src/NetStarter/Components/CodePreviewModal.razor` - Modal component with IAsyncDisposable, lazy module loading, language detection, CSS (desktop + mobile)
- `src/NetStarter/Components/CodePreviewModal.razor.js` - JS module: ESM import of highlight.js, theme CSS injection, line number wrapping
- `src/NetStarter/Components/FileTreePreview.razor` - Added OnFileSelected parameter, folder toggle, file-type icons with colors
- `src/NetStarter/Components/FileTreePreview.razor.css` - Added .file-clickable and .folder-toggle cursor pointer styles
- `src/NetStarter/Pages/Home.razor` - Added modal state fields, GetGeneratedFiles() cache, OnFileSelected handler, CodePreviewModal wiring

## Decisions Made
- Collocated JS file path: standalone Blazor WASM serves `.razor.js` files at `/Components/ComponentName.razor.js`, not at `_content/AssemblyName/` (confirmed via staticwebassets.build.json)
- Modal always rendered in DOM (not conditionally rendered with `@if`) so that the IJSObjectReference does not get disposed/remounted on close
- Generated files cache (`_generatedFiles`) invalidated by setting to null before `PushStateToUrl()` in `OnConfigChanged()` to ensure next preview shows updated content

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Needed to verify the correct import path for the collocated JS module. Static web assets build JSON confirmed it's served at `/Components/CodePreviewModal.razor.js` (relative: `./Components/CodePreviewModal.razor.js`), not `_content/NetStarter/...` which is only for Razor Class Libraries.

## User Setup Required
None - no external service configuration required. highlight.js loads from cdnjs CDN on first modal open.

## Next Phase Readiness
- Code preview modal complete and wired — users can click any file in the tree to view syntax-highlighted content
- All PREV-01 through PREV-04 requirements satisfied
- RESP-03 (mobile full-screen modal) satisfied
- Phase 11 Plan 02 (responsive design final checks) can proceed

## Self-Check: PASSED

- CodePreviewModal.razor: FOUND
- CodePreviewModal.razor.js: FOUND
- FileTreePreview.razor: FOUND
- Home.razor: FOUND
- 11-01-SUMMARY.md: FOUND
- Commit 90db1e5 (Task 1): FOUND
- Commit 1e32c89 (Task 2): FOUND

---
*Phase: 11-code-preview-and-responsive-design*
*Completed: 2026-03-01*
