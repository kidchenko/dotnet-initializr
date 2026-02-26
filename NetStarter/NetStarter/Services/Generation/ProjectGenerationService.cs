using NetStarter.Models;

namespace NetStarter.Services.Generation;

public class ProjectGenerationService
{
    public Dictionary<string, string> Generate(ProjectConfiguration config)
    {
        var files = new Dictionary<string, string>();
        var root = config.ProjectName;

        // 1. Solution file
        files[$"{root}/{root}.slnx"] = SlnxGenerator.Generate(config);

        // 2. Architecture-specific project structure
        if (config.ProjectType is ProjectType.Console or ProjectType.WorkerService)
        {
            // Console and Worker use a single project regardless of architecture selection
            ArchitectureGenerator.GenerateConsoleOrWorker(config, files, root);
        }
        else
        {
            switch (config.Architecture)
            {
                case ArchitecturePattern.CleanArchitecture:
                    ArchitectureGenerator.GenerateCleanArchitecture(config, files, root);
                    break;
                case ArchitecturePattern.VerticalSlice:
                    ArchitectureGenerator.GenerateVerticalSlice(config, files, root);
                    break;
                case ArchitecturePattern.SimpleLayered:
                    ArchitectureGenerator.GenerateSimpleLayered(config, files, root);
                    break;
            }
        }

        // 3. Aspire projects (AppHost and ServiceDefaults)
        if (config.IncludeDotNetAspire)
        {
            files[$"{root}/src/{root}.AppHost/{root}.AppHost.csproj"] =
                CsprojGenerator.GenerateAspireAppHostProject(config);
            files[$"{root}/src/{root}.AppHost/Program.cs"] =
                AspireGenerator.GenerateAppHostProgram(config);
            files[$"{root}/src/{root}.ServiceDefaults/{root}.ServiceDefaults.csproj"] =
                CsprojGenerator.GenerateAspireServiceDefaultsProject(config);
            files[$"{root}/src/{root}.ServiceDefaults/Extensions.cs"] =
                AspireGenerator.GenerateServiceDefaultsExtensions(config);
        }

        // 4. Test projects
        if (config.IncludeXUnit)
        {
            var mainProjectRef = GetMainProjectPath(config);
            files[$"{root}/tests/{root}.Tests/{root}.Tests.csproj"] =
                CsprojGenerator.GenerateTestProject(config, mainProjectRef);
            files[$"{root}/tests/{root}.Tests/UnitTest1.cs"] =
                TestProjectGenerator.GenerateSampleUnitTest(config);

            if (config.IncludeTestcontainers && config.Database.HasValue)
            {
                files[$"{root}/tests/{root}.IntegrationTests/{root}.IntegrationTests.csproj"] =
                    CsprojGenerator.GenerateIntegrationTestProject(config, mainProjectRef);
                files[$"{root}/tests/{root}.IntegrationTests/IntegrationTest1.cs"] =
                    TestProjectGenerator.GenerateIntegrationTest(config);
            }
        }

        // 5. CI/CD files
        if (config.IncludeGitHubActions)
            files[$"{root}/.github/workflows/dotnet.yml"] = CiCdGenerator.GenerateGitHubActions(config);
        if (config.IncludeAzureDevOps)
            files[$"{root}/azure-pipelines.yml"] = CiCdGenerator.GenerateAzurePipelines(config);

        // 6. Docker files
        if (config.IncludeDockerfile)
            files[$"{root}/Dockerfile"] = DockerGenerator.GenerateDockerfile(config);
        if (config.IncludeDockerCompose)
            files[$"{root}/docker-compose.yml"] = DockerGenerator.GenerateDockerCompose(config);

        // 7. Always-present root files
        files[$"{root}/.gitignore"] = StaticFileGenerator.GenerateGitignore();
        files[$"{root}/.editorconfig"] = StaticFileGenerator.GenerateEditorconfig();
        files[$"{root}/README.md"] = ReadmeGenerator.Generate(config);

        return files;
    }

    /// <summary>
    /// Returns the relative path from the test project directory to the main project .csproj.
    /// Used as ProjectReference in test .csproj files.
    /// </summary>
    private static string GetMainProjectPath(ProjectConfiguration config)
    {
        return config.Architecture switch
        {
            ArchitecturePattern.CleanArchitecture =>
                $"../../src/{config.ProjectName}.Api/{config.ProjectName}.Api.csproj",
            _ =>
                $"../../src/{config.ProjectName}/{config.ProjectName}.csproj",
        };
    }
}
