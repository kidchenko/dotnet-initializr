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

# Cleanup
echo ""
echo "=== Cleaning up ==="
rm -rf "$TEST_DIR"
dotnet new uninstall "$PACKAGE_ID" 2>/dev/null || true
rm -rf "$OUT_DIR"

echo ""
echo "=== ALL CHECKS PASSED ==="
echo "Template generates a compilable project with correct name substitution, dotfiles, and preserved #if DEBUG guards."
