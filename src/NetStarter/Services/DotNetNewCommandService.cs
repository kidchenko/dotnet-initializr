using NetStarter.Models;

namespace NetStarter.Services;

public class DotNetNewCommandService
{
    public List<string> BuildCommands(ProjectConfiguration config)
    {
        var commands = new List<string>();

        // Line 1: Install the template package
        commands.Add("dotnet new install Initializr.Templates");

        // Line 2: Generate the project with flags (only non-default flags per template.json defaults)
        var flagParts = new List<string>();

        // --projectType: template.json default is MinimalApi
        if (config.ProjectType != ProjectType.MinimalApi)
            flagParts.Add($"--projectType {config.ProjectType}");

        // --arch: template.json default is CleanArchitecture
        if (config.Architecture != ArchitecturePattern.CleanArchitecture)
            flagParts.Add($"--arch {config.Architecture}");

        // --framework: template.json default is net10.0
        if (config.SdkVersion != DotNetSdkVersion.Net10)
        {
            var framework = config.SdkVersion switch
            {
                DotNetSdkVersion.Net8 => "net8.0",
                DotNetSdkVersion.Net9 => "net9.0",
                DotNetSdkVersion.Net10 => "net10.0",
                _ => "net10.0",
            };
            flagParts.Add($"--framework {framework}");
        }

        // --orm: template.json default is EfCore
        if (config.Orm != OrmOption.EfCore)
            flagParts.Add($"--orm {config.Orm}");

        // --db: omit when orm == None OR db == PostgreSql (template.json default)
        if (config.Orm != OrmOption.None && config.Database.HasValue && config.Database.Value != DatabaseOption.PostgreSql)
            flagParts.Add($"--db {config.Database.Value}");

        // --auth: template.json default is None
        if (config.Auth != AuthOption.None)
            flagParts.Add($"--auth {config.Auth}");

        // --logging: template.json default is Serilog
        if (config.Logging != LoggingOption.Serilog)
            flagParts.Add($"--logging {config.Logging}");

        // --backgroundJobs: template.json default is None
        if (config.BackgroundJobs != BackgroundJobsOption.None)
            flagParts.Add($"--backgroundJobs {config.BackgroundJobs}");

        // --apiDocs: template.json default is None
        if (config.ApiDocsUi != OpenApiUi.None)
            flagParts.Add($"--apiDocs {config.ApiDocsUi}");

        // --healthChecks: template.json default is true
        if (!config.IncludeHealthChecks)
            flagParts.Add("--healthChecks false");

        // --openTelemetry: template.json default is false
        if (config.IncludeOpenTelemetry)
            flagParts.Add("--openTelemetry true");

        // --caching (IncludeRedis): template.json default is false
        if (config.IncludeRedis)
            flagParts.Add("--caching true");

        // --validation (IncludeFluentValidation): template.json default is true
        if (!config.IncludeFluentValidation)
            flagParts.Add("--validation false");

        // --resilience: template.json default is false
        if (config.IncludeResilience)
            flagParts.Add("--resilience true");

        // --mapping (Mapping != None): template.json default is false
        if (config.Mapping != MappingOption.None)
            flagParts.Add("--mapping true");

        // --testing: multi-value, repeated flags
        if (config.TestFramework == TestFrameworkOption.XUnit)
            flagParts.Add("--testing xunit");
        if (config.IncludeNSubstitute)
            flagParts.Add("--testing nsubstitute");
        if (config.IncludeBogus)
            flagParts.Add("--testing bogus");
        if (config.IncludeTestcontainers)
            flagParts.Add("--testing testcontainers");
        // Note: NUnit is not mapped to --testing (no template.json equivalent for NUnit via testing multi-value)
        // Note: AssertLibraryOption.Shouldly is not mapped (no --testing fluentassertions Blazor equivalent)

        // --containers: single-choice, priority: Aspire > DockerCompose > Dockerfile
        if (config.IncludeDotNetAspire)
            flagParts.Add("--containers Aspire");
        else if (config.IncludeDockerCompose)
            flagParts.Add("--containers DockerCompose");
        else if (config.IncludeDockerfile)
            flagParts.Add("--containers Dockerfile");

        // --cicd: single-choice, priority: GitHubActions > AzureDevOps
        if (config.IncludeGitHubActions)
            flagParts.Add("--cicd GitHubActions");
        else if (config.IncludeAzureDevOps)
            flagParts.Add("--cicd AzureDevOps");

        // --namespace: only when namespace differs from project name
        if (config.Namespace != config.ProjectName)
            flagParts.Add($"--namespace {config.Namespace}");

        var flags = flagParts.Count > 0 ? " " + string.Join(" ", flagParts) : "";
        commands.Add($"dotnet new dotnet-initializr -n {config.ProjectName}{flags}");

        return commands;
    }
}
