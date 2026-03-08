using NetStarter.Models;

namespace NetStarter.Services;

public class FileTreeNode
{
    public string Name { get; set; } = "";
    public bool IsFolder { get; set; }
    public List<FileTreeNode> Children { get; set; } = new();
    public bool IsExpanded { get; set; } = true;
    public bool IsProject { get; set; }
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
        if (config.HasTestFramework)
        {
            var testsFolder = new FileTreeNode { Name = "tests", IsFolder = true };
            var unitTestProject = new FileTreeNode { Name = $"{config.ProjectName}.Tests", IsFolder = true, IsProject = true };
            unitTestProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Tests.csproj", IsFolder = false });
            unitTestProject.Children.Add(new FileTreeNode { Name = "UnitTest1.cs", IsFolder = false });
            testsFolder.Children.Add(unitTestProject);

            // Testcontainers integration tests (requires database)
            if (config.IncludeTestcontainers && config.Database.HasValue)
            {
                var integrationTestProject = new FileTreeNode { Name = $"{config.ProjectName}.IntegrationTests", IsFolder = true, IsProject = true };
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
        var domainProject = new FileTreeNode { Name = $"{config.ProjectName}.Domain", IsFolder = true, IsProject = true };
        domainProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Domain.csproj", IsFolder = false });
        srcFolder.Children.Add(domainProject);

        // Application project
        var appProject = new FileTreeNode { Name = $"{config.ProjectName}.Application", IsFolder = true, IsProject = true };
        appProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Application.csproj", IsFolder = false });

        // Mapster in Application
        if (config.Mapping == MappingOption.Mapster)
        {
            var mappingFolder = new FileTreeNode { Name = "Mapping", IsFolder = true };
            mappingFolder.Children.Add(new FileTreeNode { Name = "MappingConfig.cs", IsFolder = false });
            appProject.Children.Add(mappingFolder);
        }

        if (config.IncludeFluentValidation && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        {
            var validationFolder = new FileTreeNode { Name = "Validation", IsFolder = true };
            validationFolder.Children.Add(new FileTreeNode { Name = "SampleRequestValidator.cs", IsFolder = false });
            appProject.Children.Add(validationFolder);
        }

        if (config.Mapping == MappingOption.Mapster || config.IncludeFluentValidation)
            appProject.Children.Add(new FileTreeNode { Name = "ApplicationServiceCollectionExtensions.cs", IsFolder = false });

        srcFolder.Children.Add(appProject);

        // Infrastructure project
        var infraProject = new FileTreeNode { Name = $"{config.ProjectName}.Infrastructure", IsFolder = true, IsProject = true };
        infraProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.Infrastructure.csproj", IsFolder = false });
        infraProject.Children.Add(new FileTreeNode { Name = "InfrastructureServiceCollectionExtensions.cs", IsFolder = false });

        // EF Core in Infrastructure
        if (config.Orm == OrmOption.EfCore)
        {
            var dataFolder = new FileTreeNode { Name = "Data", IsFolder = true };
            dataFolder.Children.Add(new FileTreeNode { Name = "AppDbContext.cs", IsFolder = false });
            var migrationsFolder = new FileTreeNode { Name = "Migrations", IsFolder = true };
            dataFolder.Children.Add(migrationsFolder);
            infraProject.Children.Add(dataFolder);
        }
        else if (config.Orm == OrmOption.Dapper)
        {
            var dataFolder = new FileTreeNode { Name = "Data", IsFolder = true };
            dataFolder.Children.Add(new FileTreeNode { Name = "DapperExtensions.cs", IsFolder = false });
            infraProject.Children.Add(dataFolder);
        }

        // OpenTelemetry in Infrastructure
        if (config.IncludeOpenTelemetry)
        {
            var telemetryFolder = new FileTreeNode { Name = "Telemetry", IsFolder = true };
            telemetryFolder.Children.Add(new FileTreeNode { Name = "OpenTelemetryExtensions.cs", IsFolder = false });
            infraProject.Children.Add(telemetryFolder);
        }

        srcFolder.Children.Add(infraProject);

        // Entry point project
        var apiProject = new FileTreeNode { Name = config.EntryPointProjectName, IsFolder = true, IsProject = true };
        apiProject.Children.Add(new FileTreeNode { Name = $"{config.EntryPointProjectName}.csproj", IsFolder = false });
        apiProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
        if (config.ProjectType == ProjectType.Console)
            apiProject.Children.Add(new FileTreeNode { Name = "ServiceCollectionExtensions.cs", IsFolder = false });
        apiProject.Children.Add(new FileTreeNode { Name = "appsettings.json", IsFolder = false });
        apiProject.Children.Add(new FileTreeNode { Name = "appsettings.Development.json", IsFolder = false });

        if (config.ProjectType == ProjectType.WebApi)
        {
            var controllersFolder = new FileTreeNode { Name = "Controllers", IsFolder = true };
            controllersFolder.Children.Add(new FileTreeNode { Name = "SampleController.cs", IsFolder = false });
            apiProject.Children.Add(controllersFolder);
        }
        else if (config.ProjectType == ProjectType.MinimalApi)
        {
            var endpointsFolder = new FileTreeNode { Name = "Endpoints", IsFolder = true };
            endpointsFolder.Children.Add(new FileTreeNode { Name = "SampleEndpoint.cs", IsFolder = false });
            apiProject.Children.Add(endpointsFolder);
        }

        // Auth in entry point
        if (config.Auth == AuthOption.Jwt)
        {
            var authFolder = new FileTreeNode { Name = "Auth", IsFolder = true };
            authFolder.Children.Add(new FileTreeNode { Name = "JwtSettings.cs", IsFolder = false });
            apiProject.Children.Add(authFolder);
        }

        // .NET Aspire projects
        if (config.IncludeDotNetAspire)
        {
            var appHostProject = new FileTreeNode { Name = $"{config.ProjectName}.AppHost", IsFolder = true, IsProject = true };
            appHostProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.AppHost.csproj", IsFolder = false });
            appHostProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
            srcFolder.Children.Add(appHostProject);

            var serviceDefaultsProject = new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults", IsFolder = true, IsProject = true };
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults.csproj", IsFolder = false });
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = "Extensions.cs", IsFolder = false });
            srcFolder.Children.Add(serviceDefaultsProject);
        }

        // Background Jobs folder (in Infrastructure for Clean Architecture)
        if (config.BackgroundJobs != BackgroundJobsOption.None
            && config.ProjectType != ProjectType.Console)
        {
            var jobsFolder = new FileTreeNode { Name = "Jobs", IsFolder = true };
            var sampleFileName = config.BackgroundJobs switch
            {
                BackgroundJobsOption.IHostedService => "SampleBackgroundService.cs",
                BackgroundJobsOption.Hangfire       => "SampleHangfireJob.cs",
                BackgroundJobsOption.Quartz         => "SampleQuartzJob.cs",
                _                                   => null
            };
            if (sampleFileName is not null)
                jobsFolder.Children.Add(new FileTreeNode { Name = sampleFileName, IsFolder = false });
            infraProject.Children.Add(jobsFolder);
        }

        srcFolder.Children.Add(apiProject);
    }

    private void BuildVerticalSliceStructure(ProjectConfiguration config, FileTreeNode srcFolder)
    {
        var mainProject = new FileTreeNode { Name = config.ProjectName, IsFolder = true, IsProject = true };
        mainProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.csproj", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
        if (config.ProjectType == ProjectType.Console)
            mainProject.Children.Add(new FileTreeNode { Name = "ServiceCollectionExtensions.cs", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "appsettings.json", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "appsettings.Development.json", IsFolder = false });

        // Features folder with Hello slice
        var featuresFolder = new FileTreeNode { Name = "Features", IsFolder = true };
        var helloFolder = new FileTreeNode { Name = "Sample", IsFolder = true };

        if (config.Orm == OrmOption.EfCore)
            helloFolder.Children.Add(new FileTreeNode { Name = "SampleEntity.cs", IsFolder = false });

        if (config.ProjectType == ProjectType.WebApi)
            helloFolder.Children.Add(new FileTreeNode { Name = "SampleController.cs", IsFolder = false });
        else if (config.ProjectType == ProjectType.MinimalApi)
            helloFolder.Children.Add(new FileTreeNode { Name = "SampleEndpoint.cs", IsFolder = false });

        if (config.Mapping == MappingOption.Mapster)
            helloFolder.Children.Add(new FileTreeNode { Name = "SampleMappingConfig.cs", IsFolder = false });

        if (config.IncludeFluentValidation && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
            helloFolder.Children.Add(new FileTreeNode { Name = "SampleRequestValidator.cs", IsFolder = false });

        featuresFolder.Children.Add(helloFolder);
        mainProject.Children.Add(featuresFolder);

        // EF Core — shared infrastructure (DbContext + Migrations)
        if (config.Orm == OrmOption.EfCore)
        {
            var dataFolder = new FileTreeNode { Name = "Data", IsFolder = true };
            dataFolder.Children.Add(new FileTreeNode { Name = "AppDbContext.cs", IsFolder = false });
            var migrationsFolder = new FileTreeNode { Name = "Migrations", IsFolder = true };
            dataFolder.Children.Add(migrationsFolder);
            mainProject.Children.Add(dataFolder);
        }
        else if (config.Orm == OrmOption.Dapper)
        {
            var dataFolder = new FileTreeNode { Name = "Data", IsFolder = true };
            dataFolder.Children.Add(new FileTreeNode { Name = "DapperExtensions.cs", IsFolder = false });
            mainProject.Children.Add(dataFolder);
        }

        // Auth
        if (config.Auth == AuthOption.Jwt)
        {
            var authFolder = new FileTreeNode { Name = "Auth", IsFolder = true };
            authFolder.Children.Add(new FileTreeNode { Name = "JwtSettings.cs", IsFolder = false });
            mainProject.Children.Add(authFolder);
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
            var appHostProject = new FileTreeNode { Name = $"{config.ProjectName}.AppHost", IsFolder = true, IsProject = true };
            appHostProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.AppHost.csproj", IsFolder = false });
            appHostProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
            srcFolder.Children.Add(appHostProject);

            var serviceDefaultsProject = new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults", IsFolder = true, IsProject = true };
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults.csproj", IsFolder = false });
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = "Extensions.cs", IsFolder = false });
            srcFolder.Children.Add(serviceDefaultsProject);
        }

        // Background Jobs folder
        if (config.BackgroundJobs != BackgroundJobsOption.None
            && config.ProjectType != ProjectType.Console)
        {
            var jobsFolder = new FileTreeNode { Name = "Jobs", IsFolder = true };
            var sampleFileName = config.BackgroundJobs switch
            {
                BackgroundJobsOption.IHostedService => "SampleBackgroundService.cs",
                BackgroundJobsOption.Hangfire       => "SampleHangfireJob.cs",
                BackgroundJobsOption.Quartz         => "SampleQuartzJob.cs",
                _                                   => null
            };
            if (sampleFileName is not null)
                jobsFolder.Children.Add(new FileTreeNode { Name = sampleFileName, IsFolder = false });
            mainProject.Children.Add(jobsFolder);
        }

        srcFolder.Children.Add(mainProject);
    }

    private void BuildSimpleLayeredStructure(ProjectConfiguration config, FileTreeNode srcFolder)
    {
        var mainProject = new FileTreeNode { Name = config.ProjectName, IsFolder = true, IsProject = true };
        mainProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.csproj", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
        if (config.ProjectType == ProjectType.Console)
            mainProject.Children.Add(new FileTreeNode { Name = "ServiceCollectionExtensions.cs", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "appsettings.json", IsFolder = false });
        mainProject.Children.Add(new FileTreeNode { Name = "appsettings.Development.json", IsFolder = false });

        // Controllers or Endpoints folder depending on project type
        if (config.ProjectType == ProjectType.MinimalApi)
        {
            var endpointsFolder = new FileTreeNode { Name = "Endpoints", IsFolder = true };
            endpointsFolder.Children.Add(new FileTreeNode { Name = "SampleEndpoint.cs", IsFolder = false });
            mainProject.Children.Add(endpointsFolder);
        }
        else if (config.ProjectType == ProjectType.WebApi)
        {
            var controllersFolder = new FileTreeNode { Name = "Controllers", IsFolder = true };
            controllersFolder.Children.Add(new FileTreeNode { Name = "SampleController.cs", IsFolder = false });
            mainProject.Children.Add(controllersFolder);
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
        else if (config.Orm == OrmOption.Dapper)
        {
            var dataFolder = new FileTreeNode { Name = "Data", IsFolder = true };
            dataFolder.Children.Add(new FileTreeNode { Name = "DapperExtensions.cs", IsFolder = false });
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

        if (config.IncludeFluentValidation && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        {
            var validationFolder = new FileTreeNode { Name = "Validation", IsFolder = true };
            validationFolder.Children.Add(new FileTreeNode { Name = "SampleRequestValidator.cs", IsFolder = false });
            mainProject.Children.Add(validationFolder);
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
            var appHostProject = new FileTreeNode { Name = $"{config.ProjectName}.AppHost", IsFolder = true, IsProject = true };
            appHostProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.AppHost.csproj", IsFolder = false });
            appHostProject.Children.Add(new FileTreeNode { Name = "Program.cs", IsFolder = false });
            srcFolder.Children.Add(appHostProject);

            var serviceDefaultsProject = new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults", IsFolder = true, IsProject = true };
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = $"{config.ProjectName}.ServiceDefaults.csproj", IsFolder = false });
            serviceDefaultsProject.Children.Add(new FileTreeNode { Name = "Extensions.cs", IsFolder = false });
            srcFolder.Children.Add(serviceDefaultsProject);
        }

        // Background Jobs folder
        if (config.BackgroundJobs != BackgroundJobsOption.None
            && config.ProjectType != ProjectType.Console)
        {
            var jobsFolder = new FileTreeNode { Name = "Jobs", IsFolder = true };
            var sampleFileName = config.BackgroundJobs switch
            {
                BackgroundJobsOption.IHostedService => "SampleBackgroundService.cs",
                BackgroundJobsOption.Hangfire       => "SampleHangfireJob.cs",
                BackgroundJobsOption.Quartz         => "SampleQuartzJob.cs",
                _                                   => null
            };
            if (sampleFileName is not null)
                jobsFolder.Children.Add(new FileTreeNode { Name = sampleFileName, IsFolder = false });
            mainProject.Children.Add(jobsFolder);
        }

        srcFolder.Children.Add(mainProject);
    }
}
