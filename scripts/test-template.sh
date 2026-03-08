#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TEMPLATES_DIR="$REPO_ROOT/templates"
OUT_DIR="$REPO_ROOT/out"
TEST_DIR="/tmp/dotnet-initializr-test"
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

# Step 6: Generate test project
echo "6. Generating test project..."
dotnet new dotnet-initializr -n MyApp -o "$TEST_DIR"

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
dotnet new dotnet-initializr -n ConsoleTest --project-type Console -o "$TEST_DIR/console-test"
if [ ! -d "$TEST_DIR/console-test" ]; then
    echo "FAIL: Console project generation should succeed"
    exit 1
fi
dotnet build "$TEST_DIR/console-test" --nologo
rm -rf "$TEST_DIR/console-test"
echo "   OK: Console project type generates and builds"

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
