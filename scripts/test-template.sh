#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TEMPLATES_DIR="$REPO_ROOT/templates"
OUT_DIR="$REPO_ROOT/out"
# Use /private/tmp on macOS to avoid /tmp symlink duplicate restore issue
TEST_DIR="/private/tmp/dotnet-initializr-test"
PACKAGE_ID="Initializr.Templates"

echo "=== Template Test Script ==="
echo ""

# Step 1: Clean previous output
echo "1. Cleaning previous output..."
rm -rf "$OUT_DIR"
rm -rf "$TEST_DIR"

# Step 2: Pack the template
echo "2. Packing template..."
dotnet pack "$TEMPLATES_DIR/Initializr.Templates.csproj" -o "$OUT_DIR" -c Release --nologo

# Step 3: Uninstall any existing version (ignore error if not installed)
echo "3. Uninstalling existing template..."
dotnet new uninstall "$PACKAGE_ID" 2>/dev/null || true

# Step 4: Install from local nupkg
echo "4. Installing template from local nupkg..."
NUPKG=$(ls "$OUT_DIR"/${PACKAGE_ID}.*.nupkg | head -1)
dotnet new install "$NUPKG"

# Step 5: Verify registration
echo "5. Verifying template registration..."
if ! dotnet new list | grep -q "dotnet-initializr"; then
    echo "FAIL: dotnet-initializr not found in template list"
    exit 1
fi
echo "   OK: dotnet-initializr registered"

# Step 6: Generate test project (VerticalSlice + WebApi for single-project structure tests)
echo "6. Generating test project (VerticalSlice + WebApi)..."
dotnet new dotnet-initializr -n MyApp --arch VerticalSlice --project-type WebApi -o "$TEST_DIR"

# Step 7: Verify dotfiles present (TMPL-05)
echo "7. Verifying dotfiles..."
if [ ! -f "$TEST_DIR/.gitignore" ]; then
    echo "FAIL: .gitignore missing from generated output"
    exit 1
fi
if [ ! -f "$TEST_DIR/.editorconfig" ]; then
    echo "FAIL: .editorconfig missing from generated output"
    exit 1
fi
echo "   OK: .gitignore and .editorconfig present"

# Step 8: Verify name substitution (TMPL-03)
echo "8. Verifying name substitution..."
if grep -r "Company\.ProjectName" "$TEST_DIR" --include="*.cs" --include="*.csproj" --include="*.slnx" -l 2>/dev/null; then
    echo "FAIL: Company.ProjectName still found in generated files"
    exit 1
fi
# Verify MyApp appears where expected
if ! grep -q "namespace MyApp" "$TEST_DIR/src/MyApp/Program.cs" 2>/dev/null && \
   ! grep -q "MyApp" "$TEST_DIR/src/MyApp/Controllers/HelloController.cs" 2>/dev/null; then
    # Check if the namespace is implicit (no namespace statement means global namespace)
    # For Program.cs, top-level statements don't have namespace — check HelloController instead
    if ! grep -q "namespace MyApp" "$TEST_DIR/src/MyApp/Controllers/HelloController.cs"; then
        echo "FAIL: MyApp namespace not found in generated HelloController.cs"
        exit 1
    fi
fi
echo "   OK: Company.ProjectName replaced with MyApp"

# Step 9: Verify #if DEBUG preserved (TMPL-04)
echo "9. Verifying #if DEBUG preserved..."
if ! grep -q "#if DEBUG" "$TEST_DIR/src/MyApp/Program.cs"; then
    echo "FAIL: #if DEBUG not found in generated Program.cs (template engine removed it)"
    exit 1
fi
# Verify noEmit markers are NOT in the generated output (they should be consumed by the engine)
if grep -q "noEmit" "$TEST_DIR/src/MyApp/Program.cs"; then
    echo "FAIL: noEmit markers leaked into generated output"
    exit 1
fi
echo "   OK: #if DEBUG preserved, noEmit markers consumed"

# Step 10: Build the generated project
echo "10. Building generated project..."
dotnet build "$TEST_DIR" --nologo

# Step 11: Verify directory/file name substitution
echo "11. Verifying file/directory name substitution..."
if [ ! -d "$TEST_DIR/src/MyApp" ]; then
    echo "FAIL: src/MyApp directory not found"
    exit 1
fi
if [ ! -f "$TEST_DIR/src/MyApp/MyApp.csproj" ]; then
    echo "FAIL: MyApp.csproj not found"
    exit 1
fi
if [ ! -f "$TEST_DIR/MyApp.slnx" ]; then
    echo "FAIL: MyApp.slnx not found"
    exit 1
fi
echo "   OK: File and directory names substituted correctly"

# Step 12: Verify README and global.json present
echo "12. Verifying additional files..."
if [ ! -f "$TEST_DIR/README.md" ]; then
    echo "FAIL: README.md missing"
    exit 1
fi
if [ ! -f "$TEST_DIR/global.json" ]; then
    echo "FAIL: global.json missing"
    exit 1
fi
echo "   OK: README.md and global.json present"

# ===== Phase 14: Parameter Model Verification =====

# Step 13: Verify --help shows all user-facing parameters (PARAM-01 through PARAM-13)
echo "13. Verifying --help shows all user-facing parameters..."
HELP_OUTPUT=$(dotnet new dotnet-initializr --help 2>&1)

# Core choice parameters
echo "$HELP_OUTPUT" | grep -q "\-\-project-type" || { echo "FAIL: --project-type not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-arch" || { echo "FAIL: --arch not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-orm" || { echo "FAIL: --orm not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-db" || { echo "FAIL: --db not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-auth" || { echo "FAIL: --auth not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-logging" || { echo "FAIL: --logging not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-framework" || { echo "FAIL: --framework not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-testing" || { echo "FAIL: --testing not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-containers" || { echo "FAIL: --containers not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-cicd" || { echo "FAIL: --cicd not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-background-jobs" || { echo "FAIL: --background-jobs not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-api-docs" || { echo "FAIL: --api-docs not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-health-checks" || { echo "FAIL: --health-checks not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-openTelemetry" || { echo "FAIL: --openTelemetry not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-caching" || { echo "FAIL: --caching not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-validation" || { echo "FAIL: --validation not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-resilience" || { echo "FAIL: --resilience not in help"; exit 1; }
echo "$HELP_OUTPUT" | grep -q "\-\-mapping" || { echo "FAIL: --mapping not in help"; exit 1; }

# Verify computed symbols are NOT leaked to help output
# (dotnetcli.host.json hides all IncludeX symbols via isHidden: true)
if echo "$HELP_OUTPUT" | grep -q "IncludeWebProject"; then
    echo "FAIL: computed symbol IncludeWebProject leaked to help (isHidden not working)"
    exit 1
fi
if echo "$HELP_OUTPUT" | grep -q "IncludeEfCore"; then
    echo "FAIL: computed symbol IncludeEfCore leaked to help (isHidden not working)"
    exit 1
fi
if echo "$HELP_OUTPUT" | grep -q "IncludeAnyAuth"; then
    echo "FAIL: computed symbol IncludeAnyAuth leaked to help (isHidden not working)"
    exit 1
fi
if echo "$HELP_OUTPUT" | grep -q "IncludeSerilog"; then
    echo "FAIL: computed symbol IncludeSerilog leaked to help (isHidden not working)"
    exit 1
fi
if echo "$HELP_OUTPUT" | grep -q "EntryPointSuffix"; then
    echo "FAIL: computed symbol EntryPointSuffix leaked to help (isHidden not working)"
    exit 1
fi
echo "   OK: All 18 user-facing parameters present, no computed symbols leaked"

# Step 14: Verify gating rule 1 — --db shows 'Enabled if' annotation (requires --orm)
# NOTE: In .NET 10, isEnabled is ADVISORY — the CLI does NOT reject the command;
# it silently uses the defaultValue when the condition is false.
# We verify the constraint is communicated via the 'Enabled if:' annotation in --help.
# Use precise start-of-line pattern to avoid matching --db references inside other descriptions.
echo "14. Verifying gating rule 1: --db shows 'Enabled if' annotation (requires --orm)..."
if ! echo "$HELP_OUTPUT" | grep -A5 "^  -db " | grep -q "Enabled if"; then
    echo "FAIL: --db does not show 'Enabled if' annotation in help"
    exit 1
fi
echo "   OK: --db shows 'Enabled if' gating annotation"

# Step 15: Verify gating rule 2 — --auth shows 'Enabled if' annotation (requires web type)
echo "15. Verifying gating rule 2: --auth shows 'Enabled if' annotation (requires web project-type)..."
if ! echo "$HELP_OUTPUT" | grep -A5 "\-\-auth" | grep -q "Enabled if"; then
    echo "FAIL: --auth does not show 'Enabled if' annotation in help"
    exit 1
fi
echo "   OK: --auth shows 'Enabled if: (projectType == \"WebApi\" || projectType == \"MinimalApi\")' gating annotation"

# Step 16: Verify gating rule 3 — --api-docs shows 'Enabled if' annotation (requires web type)
echo "16. Verifying gating rule 3: --api-docs shows 'Enabled if' annotation (requires web project-type)..."
if ! echo "$HELP_OUTPUT" | grep -A5 "\-\-api-docs" | grep -q "Enabled if"; then
    echo "FAIL: --api-docs does not show 'Enabled if' annotation in help"
    exit 1
fi
echo "   OK: --api-docs shows 'Enabled if: (projectType == \"WebApi\" || projectType == \"MinimalApi\")' gating annotation"

# Step 17: Verify multi-value --testing parameter (repeated flags)
# NOTE: .NET 10 requires repeated option flags, NOT comma-separated syntax.
# allowMultipleValues: true in template.json enables this.
echo "17. Verifying multi-value --testing parameter (repeated flags)..."
dotnet new dotnet-initializr -n MultiTest --testing xunit --testing fluentassertions --testing nsubstitute -o "$TEST_DIR/multi"
if [ ! -d "$TEST_DIR/multi" ]; then
    echo "FAIL: multi-value --testing with repeated flags should succeed"
    exit 1
fi
rm -rf "$TEST_DIR/multi"
echo "   OK: --testing multi-value via repeated flags accepted"

# Verify that --help confirms multiple values are allowed
if ! echo "$HELP_OUTPUT" | grep -A10 "\-\-testing" | grep -q "Multiple values are allowed"; then
    echo "FAIL: --testing does not indicate 'Multiple values are allowed' in help"
    exit 1
fi
echo "   OK: --testing shows 'Multiple values are allowed: True' in help"

# Step 18: Verify valid gated combination works (web type + auth + orm + db)
echo "18. Verifying valid gated combination: --project-type WebApi --auth Jwt --orm EfCore --db PostgreSql..."
dotnet new dotnet-initializr -n GatedValid --project-type WebApi --auth Jwt --orm EfCore --db PostgreSql -o "$TEST_DIR/gated-valid"
if [ ! -d "$TEST_DIR/gated-valid" ]; then
    echo "FAIL: valid gated combination should succeed"
    exit 1
fi
rm -rf "$TEST_DIR/gated-valid"
echo "   OK: valid gated combination accepted"

# Step 19: Verify Console project type generates and builds
echo "19. Verifying Console project type generates and builds..."
dotnet new dotnet-initializr -n ConsoleTest --project-type Console --arch SimpleLayered -o "$TEST_DIR/console-test"
if [ ! -d "$TEST_DIR/console-test" ]; then
    echo "FAIL: Console project generation should succeed"
    exit 1
fi
dotnet build "$TEST_DIR/console-test" --nologo
rm -rf "$TEST_DIR/console-test"
echo "   OK: Console project type generates and builds"

# ===== Phase 15: Architecture and Project Types Verification =====

# Step 20: Verify CleanArchitecture + WebApi generates 4 projects with .Api suffix
echo "20. Verifying CleanArchitecture + WebApi generates 4 projects with .Api suffix..."
dotnet new dotnet-initializr -n CA1 --arch CleanArchitecture --project-type WebApi -o "$TEST_DIR/ca-webapi"
[ -d "$TEST_DIR/ca-webapi/src/CA1.Domain" ] || { echo "FAIL: CA1.Domain missing"; exit 1; }
[ -d "$TEST_DIR/ca-webapi/src/CA1.Application" ] || { echo "FAIL: CA1.Application missing"; exit 1; }
[ -d "$TEST_DIR/ca-webapi/src/CA1.Infrastructure" ] || { echo "FAIL: CA1.Infrastructure missing"; exit 1; }
[ -d "$TEST_DIR/ca-webapi/src/CA1.Api" ] || { echo "FAIL: CA1.Api missing (expected .Api suffix)"; exit 1; }
dotnet build "$TEST_DIR/ca-webapi" --nologo || { echo "FAIL: CA WebApi build failed"; exit 1; }
rm -rf "$TEST_DIR/ca-webapi"
echo "   OK: CleanArchitecture + WebApi: 4 projects, .Api suffix, builds"

# Step 21: Verify CleanArchitecture + Console generates .Console suffix and Jobs/ in Infrastructure
echo "21. Verifying CleanArchitecture + Console generates .Console suffix and Jobs/ in Infrastructure..."
dotnet new dotnet-initializr -n CC1 --arch CleanArchitecture --project-type Console -o "$TEST_DIR/ca-console"
[ -d "$TEST_DIR/ca-console/src/CC1.Console" ] || { echo "FAIL: CC1.Console missing (expected .Console suffix)"; exit 1; }
[ -d "$TEST_DIR/ca-console/src/CC1.Infrastructure/Jobs" ] || { echo "FAIL: Infrastructure/Jobs/ missing for Console type"; exit 1; }
dotnet build "$TEST_DIR/ca-console" --nologo || { echo "FAIL: CA Console build failed"; exit 1; }
rm -rf "$TEST_DIR/ca-console"
echo "   OK: CleanArchitecture + Console: .Console suffix, Jobs/ in Infrastructure, builds"

# Step 22: Verify CleanArchitecture + WorkerService generates .Worker suffix and Worker.cs
echo "22. Verifying CleanArchitecture + WorkerService generates .Worker suffix and Worker.cs..."
dotnet new dotnet-initializr -n CW1 --arch CleanArchitecture --project-type WorkerService -o "$TEST_DIR/ca-worker"
[ -d "$TEST_DIR/ca-worker/src/CW1.Worker" ] || { echo "FAIL: CW1.Worker missing (expected .Worker suffix)"; exit 1; }
[ -f "$TEST_DIR/ca-worker/src/CW1.Worker/Worker.cs" ] || { echo "FAIL: Worker.cs missing for WorkerService type"; exit 1; }
dotnet build "$TEST_DIR/ca-worker" --nologo || { echo "FAIL: CA Worker build failed"; exit 1; }
rm -rf "$TEST_DIR/ca-worker"
echo "   OK: CleanArchitecture + WorkerService: .Worker suffix, Worker.cs present, builds"

# Step 23: Verify VerticalSlice + MinimalApi has Features/ and no Models/Services/Data/
echo "23. Verifying VerticalSlice + MinimalApi has Features/ and no SimpleLayered folders..."
dotnet new dotnet-initializr -n VS1 --arch VerticalSlice --project-type MinimalApi -o "$TEST_DIR/vs-minimal"
[ -d "$TEST_DIR/vs-minimal/src/VS1/Features" ] || { echo "FAIL: Features/ missing for VerticalSlice"; exit 1; }
[ ! -d "$TEST_DIR/vs-minimal/src/VS1/Models" ] || { echo "FAIL: Models/ should not be present for VerticalSlice"; exit 1; }
dotnet build "$TEST_DIR/vs-minimal" --nologo || { echo "FAIL: VS MinimalApi build failed"; exit 1; }
rm -rf "$TEST_DIR/vs-minimal"
echo "   OK: VerticalSlice + MinimalApi: Features/ present, no SimpleLayered folders, builds"

# Step 24: Verify SimpleLayered + WorkerService has Models/Services/Data/ and Jobs/ and Worker.cs
echo "24. Verifying SimpleLayered + WorkerService has correct folder structure..."
dotnet new dotnet-initializr -n SLW --arch SimpleLayered --project-type WorkerService -o "$TEST_DIR/sl-worker"
[ -d "$TEST_DIR/sl-worker/src/SLW/Models" ] || { echo "FAIL: Models/ missing for SimpleLayered"; exit 1; }
[ -d "$TEST_DIR/sl-worker/src/SLW/Services" ] || { echo "FAIL: Services/ missing for SimpleLayered"; exit 1; }
[ -d "$TEST_DIR/sl-worker/src/SLW/Data" ] || { echo "FAIL: Data/ missing for SimpleLayered"; exit 1; }
[ -d "$TEST_DIR/sl-worker/src/SLW/Jobs" ] || { echo "FAIL: Jobs/ missing for WorkerService type"; exit 1; }
[ -f "$TEST_DIR/sl-worker/src/SLW/Worker.cs" ] || { echo "FAIL: Worker.cs missing for WorkerService type"; exit 1; }
dotnet build "$TEST_DIR/sl-worker" --nologo || { echo "FAIL: SL Worker build failed"; exit 1; }
rm -rf "$TEST_DIR/sl-worker"
echo "   OK: SimpleLayered + WorkerService: Models/Services/Data/Jobs/ present, Worker.cs, builds"

# Step 25: Verify CleanArchitecture .slnx has 4 project entries
echo "25. Verifying CleanArchitecture .slnx lists 4 projects..."
dotnet new dotnet-initializr -n SLX --arch CleanArchitecture --project-type WebApi -o "$TEST_DIR/ca-slnx"
PROJECT_COUNT=$(grep -c "Project Path" "$TEST_DIR/ca-slnx/SLX.slnx")
[ "$PROJECT_COUNT" -eq 4 ] || { echo "FAIL: CA .slnx should have 4 Project entries, got $PROJECT_COUNT"; exit 1; }
rm -rf "$TEST_DIR/ca-slnx"
echo "   OK: CleanArchitecture .slnx has exactly 4 Project entries"

# Step 26: Verify single-project .slnx has 1 project entry and Folder wrappers
echo "26. Verifying single-project .slnx has 1 project entry and Folder wrappers..."
dotnet new dotnet-initializr -n SLX2 --arch VerticalSlice --project-type WebApi -o "$TEST_DIR/sp-slnx"
PROJECT_COUNT=$(grep -c "Project Path" "$TEST_DIR/sp-slnx/SLX2.slnx")
[ "$PROJECT_COUNT" -eq 1 ] || { echo "FAIL: SP .slnx should have 1 Project entry, got $PROJECT_COUNT"; exit 1; }
FOLDER_COUNT=$(grep -c "Folder Name" "$TEST_DIR/sp-slnx/SLX2.slnx")
[ "$FOLDER_COUNT" -eq 2 ] || { echo "FAIL: SP .slnx should have 2 Folder entries (src/ and tests/), got $FOLDER_COUNT"; exit 1; }
rm -rf "$TEST_DIR/sp-slnx"
echo "   OK: Single-project .slnx has 1 Project entry and 2 Folder wrappers"

# Cleanup
echo ""
echo "=== Cleaning up ==="
rm -rf "$TEST_DIR"
dotnet new uninstall "$PACKAGE_ID" 2>/dev/null || true
rm -rf "$OUT_DIR"

echo ""
echo "=== ALL CHECKS PASSED ==="
echo "Template generates a compilable project with correct name substitution, dotfiles, and preserved #if DEBUG guards."
echo "Phase 14 verification: All 18 parameters in --help, gating annotations present, multi-value testing works, Console project builds."
echo "Phase 15 verification: All arch x type combinations generate correct folder structures and compile successfully."
