using System.Text;
using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class CiCdGenerator
{
    public static string GenerateGitHubActions(ProjectConfiguration config)
    {
        var major = GetSdkMajorVersion(config.SdkVersion);
        var hasTests = config.HasTestFramework || config.IncludeTestcontainers;

        var sb = new StringBuilder();
        sb.AppendLine("name: .NET");
        sb.AppendLine();
        sb.AppendLine("on:");
        sb.AppendLine("  push:");
        sb.AppendLine("    branches: [ main ]");
        sb.AppendLine("  pull_request:");
        sb.AppendLine("    branches: [ main ]");
        sb.AppendLine();
        sb.AppendLine("jobs:");
        sb.AppendLine("  build:");
        sb.AppendLine("    runs-on: ubuntu-latest");
        sb.AppendLine();
        sb.AppendLine("    steps:");
        sb.AppendLine("      - uses: actions/checkout@v4");
        sb.AppendLine();
        sb.AppendLine("      - name: Setup .NET");
        sb.AppendLine("        uses: actions/setup-dotnet@v4");
        sb.AppendLine("        with:");
        sb.AppendLine($"          dotnet-version: {major}.0.x");
        sb.AppendLine();
        sb.AppendLine("      - name: Restore dependencies");
        sb.AppendLine("        run: dotnet restore");
        sb.AppendLine();
        sb.AppendLine("      - name: Build");
        sb.AppendLine("        run: dotnet build --no-restore");

        if (hasTests)
        {
            sb.AppendLine();
            sb.AppendLine("      - name: Test");
            sb.AppendLine("        run: dotnet test --no-build --verbosity normal");
        }

        return sb.ToString();
    }

    public static string GenerateAzurePipelines(ProjectConfiguration config)
    {
        var major = GetSdkMajorVersion(config.SdkVersion);
        var hasTests = config.HasTestFramework || config.IncludeTestcontainers;

        var sb = new StringBuilder();
        sb.AppendLine("trigger:");
        sb.AppendLine("- main");
        sb.AppendLine();
        sb.AppendLine("pool:");
        sb.AppendLine("  vmImage: ubuntu-latest");
        sb.AppendLine();
        sb.AppendLine("variables:");
        sb.AppendLine("  buildConfiguration: Release");
        sb.AppendLine();
        sb.AppendLine("steps:");
        sb.AppendLine("- task: UseDotNet@2");
        sb.AppendLine("  inputs:");
        sb.AppendLine($"    version: '{major}.x'");
        sb.AppendLine("    installationPath: $(Agent.ToolsDirectory)/dotnet");
        sb.AppendLine();
        sb.AppendLine("- task: DotNetCoreCLI@2");
        sb.AppendLine("  displayName: Restore");
        sb.AppendLine("  inputs:");
        sb.AppendLine("    command: restore");
        sb.AppendLine("    projects: '**/*.csproj'");
        sb.AppendLine();
        sb.AppendLine("- task: DotNetCoreCLI@2");
        sb.AppendLine("  displayName: Build");
        sb.AppendLine("  inputs:");
        sb.AppendLine("    command: build");
        sb.AppendLine("    projects: '**/*.csproj'");
        sb.AppendLine("    arguments: '--configuration $(buildConfiguration) --no-restore'");

        if (hasTests)
        {
            sb.AppendLine();
            sb.AppendLine("- task: DotNetCoreCLI@2");
            sb.AppendLine("  displayName: Test");
            sb.AppendLine("  inputs:");
            sb.AppendLine("    command: test");
            sb.AppendLine("    projects: '**/*Tests/*.csproj'");
            sb.AppendLine("    arguments: '--configuration $(buildConfiguration) --no-build'");
        }

        return sb.ToString();
    }

    private static int GetSdkMajorVersion(DotNetSdkVersion sdk) => sdk switch
    {
        DotNetSdkVersion.Net8 => 8,
        DotNetSdkVersion.Net9 => 9,
        DotNetSdkVersion.Net10 => 10,
        _ => 9,
    };
}
