using System.Text;
using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class SlnxGenerator
{
    public static string Generate(ProjectConfiguration config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<Solution>");

        var hasSrcProjects = false;
        var hasTestsProjects = false;

        // Collect project paths based on architecture
        var srcProjects = new List<string>();
        var testProjects = new List<string>();

        switch (config.Architecture)
        {
            case ArchitecturePattern.CleanArchitecture:
                srcProjects.Add($"src/{config.ProjectName}.Domain/{config.ProjectName}.Domain.csproj");
                srcProjects.Add($"src/{config.ProjectName}.Application/{config.ProjectName}.Application.csproj");
                srcProjects.Add($"src/{config.ProjectName}.Infrastructure/{config.ProjectName}.Infrastructure.csproj");
                srcProjects.Add($"src/{config.ProjectName}.Api/{config.ProjectName}.Api.csproj");
                break;
            case ArchitecturePattern.VerticalSlice:
                srcProjects.Add($"src/{config.ProjectName}/{config.ProjectName}.csproj");
                break;
            case ArchitecturePattern.SimpleLayered:
                srcProjects.Add($"src/{config.ProjectName}/{config.ProjectName}.csproj");
                break;
        }

        if (config.IncludeDotNetAspire)
        {
            srcProjects.Add($"src/{config.ProjectName}.AppHost/{config.ProjectName}.AppHost.csproj");
            srcProjects.Add($"src/{config.ProjectName}.ServiceDefaults/{config.ProjectName}.ServiceDefaults.csproj");
        }

        if (config.IncludeXUnit)
        {
            testProjects.Add($"tests/{config.ProjectName}.Tests/{config.ProjectName}.Tests.csproj");
        }

        if (config.IncludeTestcontainers && config.Database.HasValue)
        {
            testProjects.Add($"tests/{config.ProjectName}.IntegrationTests/{config.ProjectName}.IntegrationTests.csproj");
        }

        hasSrcProjects = srcProjects.Count > 0;
        hasTestsProjects = testProjects.Count > 0;

        // Add src folder entry
        if (hasSrcProjects)
        {
            sb.AppendLine("  <Folder Path=\"src/\" />");
        }

        // Add test folder entry
        if (hasTestsProjects)
        {
            sb.AppendLine("  <Folder Path=\"tests/\" />");
        }

        // Add src project entries
        foreach (var path in srcProjects)
        {
            sb.AppendLine($"  <Project Path=\"{path}\" />");
        }

        // Add test project entries
        foreach (var path in testProjects)
        {
            sb.AppendLine($"  <Project Path=\"{path}\" />");
        }

        sb.Append("</Solution>");

        return sb.ToString();
    }
}
