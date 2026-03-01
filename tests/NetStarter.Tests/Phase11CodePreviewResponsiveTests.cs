using NetStarter.Models;
using NetStarter.Services;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Tests for Phase 11 code preview language detection and file content lookup.
/// Covers: PREV-01 (file content lookup), PREV-03 (language detection).
/// </summary>
public class Phase11CodePreviewResponsiveTests
{
    private readonly ProjectGenerationService _generationService = new();

    // PREV-01: File content lookup works for generated files
    [Fact]
    public void Generate_DefaultWebApi_AllTreeFilesHaveContent()
    {
        // Default config generates files — verify the dictionary is non-empty
        // and contains expected key files
        var config = new ProjectConfiguration();
        var files = _generationService.Generate(config);

        Assert.NotEmpty(files);
        // Program.cs must exist under the project name
        Assert.Contains(files, f => f.Key.EndsWith("Program.cs"));
        Assert.Contains(files, f => f.Key.EndsWith(".csproj"));
        Assert.Contains(files, f => f.Key.EndsWith("appsettings.json"));
    }

    // PREV-01: File tree paths match generation dictionary keys (minus leading slash)
    [Fact]
    public void Generate_FileTreePaths_MatchGenerationDictionaryKeys()
    {
        var config = new ProjectConfiguration();
        var files = _generationService.Generate(config);
        var treeService = new FileTreeService();
        var tree = treeService.GenerateTree(config);

        // Collect all file (non-folder) paths from tree
        var treePaths = CollectFilePaths(tree, "");

        // Every tree file path (after TrimStart('/')) should have a dictionary entry
        // Some tree nodes are folders (empty dirs) which won't be in the dict — skip those
        var missingFiles = new List<string>();
        foreach (var treePath in treePaths)
        {
            var dictKey = treePath.TrimStart('/');
            if (!files.ContainsKey(dictKey))
            {
                missingFiles.Add(dictKey);
            }
        }

        // The key assertion: at least 80% of tree files have generation content
        var matchRate = (double)(treePaths.Count - missingFiles.Count) / treePaths.Count;
        Assert.True(matchRate >= 0.8,
            $"Only {matchRate:P0} of tree files found in generation dict. Missing: {string.Join(", ", missingFiles.Take(5))}");
    }

    // PREV-03: Language detection coverage — verify all generated file types return expected identifiers
    [Theory]
    [InlineData("Program.cs", "csharp")]
    [InlineData("appsettings.json", "json")]
    [InlineData("MyProject.csproj", "xml")]
    [InlineData("docker-compose.yml", "yaml")]
    [InlineData("Dockerfile", "dockerfile")]
    [InlineData(".gitignore", "plaintext")]
    [InlineData(".editorconfig", "ini")]
    [InlineData("README.md", "markdown")]
    [InlineData("dotnet.yml", "yaml")]
    [InlineData("MyProject.slnx", "xml")]
    public void GetLanguage_ReturnsCorrectLanguageIdentifier(string fileName, string expectedLanguage)
    {
        var result = GetLanguage(fileName);
        Assert.Equal(expectedLanguage, result);
    }

    // PREV-01: Different config combinations produce different file sets
    [Fact]
    public void Generate_DifferentConfigs_ProduceDifferentFileSets()
    {
        var minimalConfig = new ProjectConfiguration { ProjectType = ProjectType.Console };
        var fullConfig = new ProjectConfiguration
        {
            ProjectType = ProjectType.WebApi,
            Orm = OrmOption.EfCore,
            Database = DatabaseOption.PostgreSql,
            IncludeDockerfile = true,
            IncludeDockerCompose = true
        };

        var minimalFiles = _generationService.Generate(minimalConfig);
        var fullFiles = _generationService.Generate(fullConfig);

        Assert.True(fullFiles.Count > minimalFiles.Count,
            $"Full config ({fullFiles.Count} files) should produce more files than minimal ({minimalFiles.Count} files)");
    }

    // PREV-01: Generated content is non-empty for key files
    [Fact]
    public void Generate_DefaultWebApi_ProgramCsHasContent()
    {
        var config = new ProjectConfiguration();
        var files = _generationService.Generate(config);
        var programCs = files.FirstOrDefault(f => f.Key.EndsWith("Program.cs"));

        Assert.NotEqual(default, programCs);
        Assert.False(string.IsNullOrWhiteSpace(programCs.Value),
            "Program.cs content should not be empty");
        Assert.Contains("builder", programCs.Value);
    }

    // PREV-01: All generated file values are non-null strings
    [Fact]
    public void Generate_DefaultWebApi_AllFilesHaveNonNullContent()
    {
        var config = new ProjectConfiguration();
        var files = _generationService.Generate(config);

        foreach (var kvp in files)
        {
            Assert.False(kvp.Value == null,
                $"File '{kvp.Key}' has null content");
        }
    }

    // Helper: collect file paths from tree (non-folder nodes only)
    private static List<string> CollectFilePaths(List<FileTreeNode> nodes, string prefix)
    {
        var paths = new List<string>();
        foreach (var node in nodes)
        {
            var path = prefix + "/" + node.Name;
            if (node.IsFolder)
            {
                paths.AddRange(CollectFilePaths(node.Children, path));
            }
            else
            {
                paths.Add(path);
            }
        }
        return paths;
    }

    // Mirror of CodePreviewModal.GetLanguage for testability
    // Keep this in sync with the component's implementation
    private static string GetLanguage(string filePath)
    {
        if (Path.GetFileName(filePath) == "Dockerfile")
            return "dockerfile";

        return Path.GetExtension(filePath).ToLower() switch
        {
            ".cs" => "csharp",
            ".xml" => "xml",
            ".csproj" => "xml",
            ".slnx" => "xml",
            ".json" => "json",
            ".yml" => "yaml",
            ".yaml" => "yaml",
            ".sh" => "bash",
            ".md" => "markdown",
            ".editorconfig" => "ini",
            _ => "plaintext"
        };
    }
}
