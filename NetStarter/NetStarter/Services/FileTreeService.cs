using NetStarter.Models;

namespace NetStarter.Services;

public class FileTreeNode
{
    public string Name { get; set; } = "";
    public bool IsFolder { get; set; }
    public List<FileTreeNode> Children { get; set; } = new();
    public bool IsExpanded { get; set; } = true;
}

public class FileTreeService
{
    public List<FileTreeNode> GenerateTree(ProjectConfiguration config)
    {
        var root = new FileTreeNode
        {
            Name = config.ProjectName,
            IsFolder = true,
            IsExpanded = true
        };

        // Always-present root files
        root.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.slnx", IsFolder = false });

        // src/ folder
        var srcFolder = new FileTreeNode { Name = "src", IsFolder = true };

        if (config.Architecture == ArchitecturePattern.CleanArchitecture)
        {
            BuildCleanArchitectureStructure(config, srcFolder);
        }
        else if (config.Architecture == ArchitecturePattern.VerticalSlice)
        {
            BuildVerticalSliceStructure(config, srcFolder);
        }
        else // SimpleLayered
        {
            BuildSimpleLayeredStructure(config, srcFolder);
        }

        root.Children.Add(srcFolder);

        // tests/ folder (xUnit)
        if (config.IncludeXUnit)
        {
            var testsFolder = new FileTreeNode { Name = "tests", IsFolder = true };
            var unitTestProject = new FileTreeNode { Name = $"{config.ProjectName}.Tests", IsFolder = true };
            unitTestProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Tests.csproj", IsFolder = false });
            unitTestProject.Children.Add(new FileTreeNode { Name = "UnitTest1.cs", IsFolder = false });
            testsFolder.Children.Add(unitTestProject);

            // Testcontainers integration tests (requires database)
            if (config.IncludeTestcontainers && config.Database.HasValue)
            {
                var integrationTestProject = new FileTreeNode { Name = $"{config.ProjectName}.IntegrationTests", IsFolder = true };
                integrationTestProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.IntegrationTests.csproj", IsFolder = false });
                integrationTestProject.Children.Add(new FileTreeNode { Name = "IntegrationTest1.cs", IsFolder = false });
                testsFolder.Children.Add(integrationTestProject);
            }

            root.Children.Add(testsFolder);
        }

        // .github/workflows/ (GitHub Actions)
        if (config.IncludeGitHubActions)
        {
            var githubFolder = new FileTreeNode { Name = ".github", IsFolder = true };
            var workflowsFolder = new FileTreeNode { Name = "workflows", IsFolder = true };
            workflowsFolder.Children.Add(new FileTreeNode { Name = "dotnet.yml", IsFolder = false });
            githubFolder.Children.Add(workflowsFolder);
            root.Children.Add(githubFolder);
        }

        // .azuredevops/ (Azure DevOps)
        if (config.IncludeAzureDevOps)
        {
            root.Children.Add(new FileTreeNode { Name = "azure-pipelines.yml", IsFolder = false });
        }

        // Dockerfile at root
        if (config.IncludeDockerfile)
        {
            root.Children.Add(new FileTreeNode { Name = "Dockerfile", IsFolder = false });
        }

        // docker-compose.yml at root
        if (config.IncludeDockerCompose)
        {
            root.Children.Add(new FileTreeNode { Name = "docker-compose.yml", IsFolder = false });
        }

        // Always-present root files
        root.Children.Add(new FileTreeNode { Name = ".gitignore", IsFolder = false });
        root.Children.Add(new FileTreeNode { Name = ".editorconfig", IsFolder = false });
        root.Children.Add(new FileTreeNode { Name = "README.md", IsFolder = false });

        return new List<FileTreeNode> { root };
    }

    private void BuildCleanArchitectureStructure(ProjectConfiguration config, FileTreeNode srcFolder)
    {
        // Domain project
        var domainProject = new FileTreeNode { Name = $"{config.ProjectName}.Domain", IsFolder = true };
        domainProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Domain.csproj", IsFolder = false });
        srcFolder.Children.Add(domainProject);

        // Application project
        var appProject = new FileTreeNode { Name = $"{config.ProjectName}.Application", IsFolder = true };
        appProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Application.csproj", IsFolder = false });

        // Mapster in Application
        if (config.Mapping == MappingOption.Mapster)
        {
            var mappingFolder = new FileTreeNode { Name = "Mapping", IsFolder = true };
            mappingFolder.Children.Add(new FileTreeNode { Name = "MappingConfig.cs", IsFolder = false });
            appProject.Children.Add(mappingFolder);
        }

        srcFolder.Children.Add(appProject);

        // Infrastructure project
        var infraProject = new FileTreeNode { Name = $"{config.ProjectName}.Infrastructure", IsFolder = true };
        infraProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Infrastructure.csproj", IsFolder = false });

        // EF Core in Infrastructure
        if (config.Orm == OrmOption.EfCore)
        {
            var dataFolder = new FileTreeNode { Name = "Data", IsFolder = true };
            dataFolder.Children.Add(new FileTreeNode { Name = "AppDbContext.cs", IsFolder = false });
            var migrationsFolder = new FileTreeNode { Name = "Migrations", IsFolder = true };
            dataFolder.Children.Add(migrationsFolder);
            infraProject.Children.Add(dataFolder);
        }

        srcFolder.Children.Add(infraProject);

        // API project
        var apiProject = new FileTreeNode { Name = $"{config.ProjectName}.Api", IsFolder = true };
        apiProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Api.csproj", IsFolder = false });
        apiProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
        apiProject.Children.Add(new FileTreeNode { Name = "appsettings.json", IsFolder = false });
        apiProject.Children.Add(new FileTreeNode { Name = "appsettings.Development.json", IsFolder = false });

        // Auth in API
        if (config.Auth == AuthOption.Jwt)
        {
            var authFolder = new FileTreeNode { Name = "Auth", IsFolder = true };
            authFolder.Children.Add(new FileTreeNode { Name = "JwtSettings.cs", IsFolder = false });
            apiProject.Children.Add(authFolder);
        }

        // OpenTelemetry in API
        if (config.IncludeOpenTelemetry)
        {
            var telemetryFolder = new FileTreeNode { Name = "Telemetry", IsFolder = true };
            telemetryFolder.Children.Add(new FileTreeNode { Name = "OpenTelemetryExtensions.cs", IsFolder = false });
            apiProject.Children.Add(telemetryFolder);
        }

        // .NET Aspire projects
        if (config.IncludeDotNetAspire)
        {
            var appHostProject = new FileTreeNode { Name = $"{config.ProjectName}.AppHost", IsFolder = true };
            appHostProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.AppHost.csproj", IsFolder = false });
            appHostProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
            srcFolder.Children.Add(appHostProject);

            var serviceDefaultsProject = new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults", IsFolder = true };
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults.csproj", IsFolder = false });
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = "Extensions.cs", IsFolder = false });
            srcFolder.Children.Add(serviceDefaultsProject);
        }

        srcFolder.Children.Add(apiProject);
    }

    private void BuildVerticalSliceStructure(ProjectConfiguration config, FileTreeNode srcFolder)
    {
        var mainProject = new FileTreeNode { Name = config.ProjectName, IsFolder = true };
        mainProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.csproj", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "appsettings.json", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "appsettings.Development.json", IsFolder = false });

        // Features folder
        var featuresFolder = new FileTreeNode { Name = "Features", IsFolder = true };
        mainProject.Children.Add(featuresFolder);

        // EF Core
        if (config.Orm == OrmOption.EfCore)
        {
            var dataFolder = new FileTreeNode { Name = "Data", IsFolder = true };
            dataFolder.Children.Add(new FileTreeNode { Name = "AppDbContext.cs", IsFolder = false });
            var migrationsFolder = new FileTreeNode { Name = "Migrations", IsFolder = true };
            dataFolder.Children.Add(migrationsFolder);
            mainProject.Children.Add(dataFolder);
        }

        // Auth
        if (config.Auth == AuthOption.Jwt)
        {
            var authFolder = new FileTreeNode { Name = "Auth", IsFolder = true };
            authFolder.Children.Add(new FileTreeNode { Name = "JwtSettings.cs", IsFolder = false });
            mainProject.Children.Add(authFolder);
        }

        // Mapster
        if (config.Mapping == MappingOption.Mapster)
        {
            var mappingFolder = new FileTreeNode { Name = "Mapping", IsFolder = true };
            mappingFolder.Children.Add(new FileTreeNode { Name = "MappingConfig.cs", IsFolder = false });
            mainProject.Children.Add(mappingFolder);
        }

        // OpenTelemetry
        if (config.IncludeOpenTelemetry)
        {
            var telemetryFolder = new FileTreeNode { Name = "Telemetry", IsFolder = true };
            telemetryFolder.Children.Add(new FileTreeNode { Name = "OpenTelemetryExtensions.cs", IsFolder = false });
            mainProject.Children.Add(telemetryFolder);
        }

        // .NET Aspire projects
        if (config.IncludeDotNetAspire)
        {
            var appHostProject = new FileTreeNode { Name = $"{config.ProjectName}.AppHost", IsFolder = true };
            appHostProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.AppHost.csproj", IsFolder = false });
            appHostProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
            srcFolder.Children.Add(appHostProject);

            var serviceDefaultsProject = new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults", IsFolder = true };
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults.csproj", IsFolder = false });
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = "Extensions.cs", IsFolder = false });
            srcFolder.Children.Add(serviceDefaultsProject);
        }

        srcFolder.Children.Add(mainProject);
    }

    private void BuildSimpleLayeredStructure(ProjectConfiguration config, FileTreeNode srcFolder)
    {
        var mainProject = new FileTreeNode { Name = config.ProjectName, IsFolder = true };
        mainProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.csproj", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "appsettings.json", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "appsettings.Development.json", IsFolder = false });

        // Controllers or Endpoints folder depending on project type
        if (config.ProjectType == ProjectType.MinimalApi)
        {
            mainProject.Children.Add(new FileTreeNode { Name = "Endpoints", IsFolder = true });
        }
        else
        {
            mainProject.Children.Add(new FileTreeNode { Name = "Controllers", IsFolder = true });
        }

        mainProject.Children.Add(new FileTreeNode { Name = "Services", IsFolder = true });

        // EF Core
        if (config.Orm == OrmOption.EfCore)
        {
            var dataFolder = new FileTreeNode { Name = "Data", IsFolder = true };
            dataFolder.Children.Add(new FileTreeNode { Name = "AppDbContext.cs", IsFolder = false });
            var migrationsFolder = new FileTreeNode { Name = "Migrations", IsFolder = true };
            dataFolder.Children.Add(migrationsFolder);
            mainProject.Children.Add(dataFolder);
        }
        else
        {
            mainProject.Children.Add(new FileTreeNode { Name = "Data", IsFolder = true });
        }

        // Auth
        if (config.Auth == AuthOption.Jwt)
        {
            var authFolder = new FileTreeNode { Name = "Auth", IsFolder = true };
            authFolder.Children.Add(new FileTreeNode { Name = "JwtSettings.cs", IsFolder = false });
            mainProject.Children.Add(authFolder);
        }

        // Mapster
        if (config.Mapping == MappingOption.Mapster)
        {
            var mappingFolder = new FileTreeNode { Name = "Mapping", IsFolder = true };
            mappingFolder.Children.Add(new FileTreeNode { Name = "MappingConfig.cs", IsFolder = false });
            mainProject.Children.Add(mappingFolder);
        }

        // OpenTelemetry
        if (config.IncludeOpenTelemetry)
        {
            var telemetryFolder = new FileTreeNode { Name = "Telemetry", IsFolder = true };
            telemetryFolder.Children.Add(new FileTreeNode { Name = "OpenTelemetryExtensions.cs", IsFolder = false });
            mainProject.Children.Add(telemetryFolder);
        }

        // .NET Aspire projects
        if (config.IncludeDotNetAspire)
        {
            var appHostProject = new FileTreeNode { Name = $"{config.ProjectName}.AppHost", IsFolder = true };
            appHostProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.AppHost.csproj", IsFolder = false });
            appHostProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
            srcFolder.Children.Add(appHostProject);

            var serviceDefaultsProject = new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults", IsFolder = true };
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults.csproj", IsFolder = false });
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = "Extensions.cs", IsFolder = false });
            srcFolder.Children.Add(serviceDefaultsProject);
        }

        srcFolder.Children.Add(mainProject);
    }
}
