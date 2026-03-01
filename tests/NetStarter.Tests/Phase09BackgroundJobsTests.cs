using NetStarter.Models;
using NetStarter.Services;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Comprehensive tests covering all Phase 9 JOBS requirements.
/// Background Jobs radio group picker, NuGet packages, Program.cs code generation,
/// sample class generation, file tree, and project generation service wiring.
/// Covers: JOBS-01 through JOBS-06.
/// </summary>
public class Phase09BackgroundJobsTests
{
    private static ProjectConfiguration CreateConfig(
        BackgroundJobsOption backgroundJobs = BackgroundJobsOption.None,
        ProjectType projectType = ProjectType.WebApi,
        DotNetSdkVersion sdk = DotNetSdkVersion.Net9,
        DatabaseOption? database = DatabaseOption.PostgreSql,
        ArchitecturePattern architecture = ArchitecturePattern.SimpleLayered)
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = sdk,
            ProjectType = projectType,
            Architecture = architecture,
            BackgroundJobs = backgroundJobs,
        };

        if (database.HasValue)
        {
            config.Orm = OrmOption.EfCore;
            config.Database = database;
        }

        return config;
    }

    // ---- JOBS-01: Enum and defaults ----

    [Fact] // JOBS-01: BackgroundJobsOption enum has four expected values
    public void JOBS_01_BackgroundJobsOptionEnumExists()
    {
        var values = Enum.GetValues<BackgroundJobsOption>();
        Assert.Contains(BackgroundJobsOption.None, values);
        Assert.Contains(BackgroundJobsOption.IHostedService, values);
        Assert.Contains(BackgroundJobsOption.Hangfire, values);
        Assert.Contains(BackgroundJobsOption.Quartz, values);
        Assert.Equal(4, values.Length);
    }

    [Fact] // JOBS-01: New ProjectConfiguration defaults BackgroundJobs to None
    public void JOBS_01_DefaultIsNone()
    {
        var config = new ProjectConfiguration();
        Assert.Equal(BackgroundJobsOption.None, config.BackgroundJobs);
    }

    [Fact] // JOBS-01: Console project type should not generate background jobs code
    public void JOBS_01_HiddenForConsole()
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService, ProjectType.Console);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("AddHostedService", result);
        Assert.DoesNotContain("AddHangfire", result);
        Assert.DoesNotContain("AddQuartz", result);
    }

    [Fact] // JOBS-01: Worker Service with IHostedService generates AddHostedService
    public void JOBS_01_AvailableForWorkerService()
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService, ProjectType.WorkerService, database: null);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddHostedService<SampleBackgroundService>()", result);
    }

    // ---- JOBS-02: IHostedService ----

    [Fact] // JOBS-02: IHostedService selects no NuGet packages (already in hosting abstractions)
    public void JOBS_02_IHostedService_NoNuGetPackages()
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService, ProjectType.WebApi);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("Hangfire", result);
        Assert.DoesNotContain("Quartz", result);
    }

    [Theory] // JOBS-02: IHostedService emits AddHostedService<SampleBackgroundService>() for web and worker project types
    [InlineData(ProjectType.WebApi)]
    [InlineData(ProjectType.MinimalApi)]
    [InlineData(ProjectType.WorkerService)]
    public void JOBS_02_IHostedService_ProgramCs_AddHostedService(ProjectType projectType)
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService, projectType, database: null);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddHostedService<SampleBackgroundService>()", result);
    }

    [Fact] // JOBS-02: BackgroundJobsGenerator produces SampleBackgroundService with BackgroundService base class
    public void JOBS_02_IHostedService_SampleClass()
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService);
        var result = BackgroundJobsGenerator.GenerateSampleBackgroundService(config);
        Assert.Contains("BackgroundService", result);
        Assert.Contains("ExecuteAsync", result);
        Assert.Contains("ILogger<SampleBackgroundService>", result);
    }

    // ---- JOBS-03: Hangfire packages ----

    [Fact] // JOBS-03: Hangfire + PostgreSql emits Hangfire, Hangfire.AspNetCore, Hangfire.PostgreSql
    public void JOBS_03_Hangfire_PostgreSql_Packages()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WebApi, database: DatabaseOption.PostgreSql);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Hangfire", result);
        Assert.Contains("Hangfire.AspNetCore", result);
        Assert.Contains("Hangfire.PostgreSql", result);
    }

    [Fact] // JOBS-03: Hangfire + SqlServer emits Hangfire, Hangfire.AspNetCore, Hangfire.SqlServer
    public void JOBS_03_Hangfire_SqlServer_Packages()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WebApi, database: DatabaseOption.SqlServer);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Hangfire", result);
        Assert.Contains("Hangfire.AspNetCore", result);
        Assert.Contains("Hangfire.SqlServer", result);
    }

    [Fact] // JOBS-03: Hangfire + MySql emits Hangfire, Hangfire.AspNetCore, Hangfire.MySqlStorage
    public void JOBS_03_Hangfire_MySql_Packages()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WebApi, database: DatabaseOption.MySql);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Hangfire", result);
        Assert.Contains("Hangfire.AspNetCore", result);
        Assert.Contains("Hangfire.MySqlStorage", result);
    }

    [Theory] // JOBS-03: Hangfire emits correct storage method in Program.cs services
    [InlineData(DatabaseOption.PostgreSql, "UsePostgreSqlStorage")]
    [InlineData(DatabaseOption.SqlServer, "UseSqlServerStorage")]
    [InlineData(DatabaseOption.MySql, "UseMySqlStorage")]
    public void JOBS_03_Hangfire_ProgramCs_Services(DatabaseOption database, string storageMethod)
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WebApi, database: database);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddHangfire", result);
        Assert.Contains("AddHangfireServer", result);
        Assert.Contains(storageMethod, result);
    }

    [Fact] // JOBS-03: Hangfire without database triggers HANGFIRE_REQUIRES_DATABASE validation error
    public void JOBS_03_Hangfire_Validation_RequiresDatabase()
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            BackgroundJobs = BackgroundJobsOption.Hangfire,
            Database = null,
        };
        var errors = config.Validate();
        Assert.Contains(errors, e => e.Code == "HANGFIRE_REQUIRES_DATABASE");
    }

    [Fact] // JOBS-03: Hangfire with database does NOT trigger HANGFIRE_REQUIRES_DATABASE
    public void JOBS_03_Hangfire_WithDatabase_NoValidationError()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WebApi, database: DatabaseOption.PostgreSql);
        var errors = config.Validate();
        Assert.DoesNotContain(errors, e => e.Code == "HANGFIRE_REQUIRES_DATABASE");
    }

    [Fact] // JOBS-03: BackgroundJobsGenerator produces SampleHangfireJob with ILogger and Execute method
    public void JOBS_03_Hangfire_SampleClass()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, database: DatabaseOption.PostgreSql);
        var result = BackgroundJobsGenerator.GenerateSampleHangfireJob(config);
        Assert.Contains("ILogger<SampleHangfireJob>", result);
        Assert.Contains("Execute()", result);
    }

    // ---- JOBS-04: Hangfire dashboard ----

    [Theory] // JOBS-04: Hangfire dashboard (UseHangfireDashboard) is emitted for WebApi and MinimalApi in IsDevelopment block
    [InlineData(ProjectType.WebApi)]
    [InlineData(ProjectType.MinimalApi)]
    public void JOBS_04_Hangfire_Dashboard_WebOnly(ProjectType projectType)
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, projectType, database: DatabaseOption.PostgreSql);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("UseHangfireDashboard(\"/hangfire\")", result);
    }

    [Fact] // JOBS-04: Hangfire dashboard NOT emitted for Worker Service
    public void JOBS_04_Hangfire_Dashboard_NotForWorkerService()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WorkerService, database: DatabaseOption.PostgreSql);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("UseHangfireDashboard", result);
    }

    [Fact] // JOBS-04: Hangfire dashboard is guarded by IsDevelopment()
    public void JOBS_04_Hangfire_Dashboard_IsDevelopmentGuard()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WebApi, database: DatabaseOption.PostgreSql);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("IsDevelopment()", result);
        Assert.Contains("UseHangfireDashboard", result);

        // Verify IsDevelopment appears before UseHangfireDashboard
        var isDevelopmentIndex = result.IndexOf("IsDevelopment()", StringComparison.Ordinal);
        var dashboardIndex = result.IndexOf("UseHangfireDashboard", StringComparison.Ordinal);
        Assert.True(isDevelopmentIndex < dashboardIndex, "IsDevelopment() should appear before UseHangfireDashboard");
    }

    [Fact] // JOBS-04: Hangfire in WebApi emits using Hangfire; in Program.cs
    public void JOBS_04_Hangfire_UsingHangfire()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WebApi, database: DatabaseOption.PostgreSql);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("using Hangfire;", result);
    }

    // ---- JOBS-05: Quartz packages ----

    [Fact] // JOBS-05: Quartz emits all three required packages
    public void JOBS_05_Quartz_AllThreePackages()
    {
        var config = CreateConfig(BackgroundJobsOption.Quartz, ProjectType.WebApi);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Quartz", result);
        Assert.Contains("Quartz.Extensions.Hosting", result);
        Assert.Contains("Quartz.Extensions.DependencyInjection", result);
    }

    [Theory] // JOBS-05: Quartz emits AddQuartz, UseMicrosoftDependencyInjectionJobFactory, and AddQuartzHostedService in Program.cs
    [InlineData(ProjectType.WebApi)]
    [InlineData(ProjectType.WorkerService)]
    public void JOBS_05_Quartz_ProgramCs_Services(ProjectType projectType)
    {
        var config = CreateConfig(BackgroundJobsOption.Quartz, projectType, database: null);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddQuartz", result);
        Assert.Contains("UseMicrosoftDependencyInjectionJobFactory", result);
        Assert.Contains("AddQuartzHostedService", result);
    }

    [Fact] // JOBS-05: BackgroundJobsGenerator produces SampleQuartzJob with correct usings, IJob, IJobExecutionContext, ILogger
    public void JOBS_05_Quartz_SampleClass()
    {
        var config = CreateConfig(BackgroundJobsOption.Quartz);
        var result = BackgroundJobsGenerator.GenerateSampleQuartzJob(config);
        Assert.Contains("using Quartz;", result);
        Assert.Contains("IJob", result);
        Assert.Contains("IJobExecutionContext", result);
        Assert.Contains("ILogger<SampleQuartzJob>", result);
    }

    [Fact] // JOBS-05: BackgroundJobs=None produces no Quartz packages in csproj
    public void JOBS_05_Quartz_NoNuGetForNone()
    {
        var config = CreateConfig(BackgroundJobsOption.None, ProjectType.WebApi);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("Quartz", result);
    }

    // ---- JOBS-06: File tree ----

    private static FileTreeNode? FindNode(IEnumerable<FileTreeNode> nodes, string name)
    {
        foreach (var node in nodes)
        {
            if (node.Name == name) return node;
            var found = FindNode(node.Children, name);
            if (found is not null) return found;
        }
        return null;
    }

    [Theory] // JOBS-06: Jobs folder appears in file tree for all architectures and non-None BackgroundJobs options
    [InlineData(ArchitecturePattern.CleanArchitecture, BackgroundJobsOption.IHostedService)]
    [InlineData(ArchitecturePattern.CleanArchitecture, BackgroundJobsOption.Hangfire)]
    [InlineData(ArchitecturePattern.CleanArchitecture, BackgroundJobsOption.Quartz)]
    [InlineData(ArchitecturePattern.VerticalSlice, BackgroundJobsOption.IHostedService)]
    [InlineData(ArchitecturePattern.VerticalSlice, BackgroundJobsOption.Hangfire)]
    [InlineData(ArchitecturePattern.VerticalSlice, BackgroundJobsOption.Quartz)]
    [InlineData(ArchitecturePattern.SimpleLayered, BackgroundJobsOption.IHostedService)]
    [InlineData(ArchitecturePattern.SimpleLayered, BackgroundJobsOption.Hangfire)]
    [InlineData(ArchitecturePattern.SimpleLayered, BackgroundJobsOption.Quartz)]
    public void JOBS_06_FileTree_Jobs_Folder(ArchitecturePattern architecture, BackgroundJobsOption jobs)
    {
        var database = jobs == BackgroundJobsOption.Hangfire ? DatabaseOption.PostgreSql : (DatabaseOption?)null;
        var config = CreateConfig(jobs, ProjectType.WebApi, database: database, architecture: architecture);
        var service = new FileTreeService();
        var tree = service.GenerateTree(config);
        var jobsNode = FindNode(tree, "Jobs");
        Assert.NotNull(jobsNode);
        Assert.True(jobsNode.IsFolder);
        Assert.NotEmpty(jobsNode.Children);

        var expectedFileName = jobs switch
        {
            BackgroundJobsOption.IHostedService => "SampleBackgroundService.cs",
            BackgroundJobsOption.Hangfire => "SampleHangfireJob.cs",
            BackgroundJobsOption.Quartz => "SampleQuartzJob.cs",
            _ => null
        };
        Assert.Contains(jobsNode.Children, c => c.Name == expectedFileName);
    }

    [Fact] // JOBS-06: No Jobs folder in file tree when BackgroundJobs == None
    public void JOBS_06_FileTree_NoJobsFolder_WhenNone()
    {
        var config = CreateConfig(BackgroundJobsOption.None, ProjectType.WebApi);
        var service = new FileTreeService();
        var tree = service.GenerateTree(config);
        var jobsNode = FindNode(tree, "Jobs");
        Assert.Null(jobsNode);
    }

    [Fact] // JOBS-06: No Jobs folder in file tree for Console even when BackgroundJobs is set
    public void JOBS_06_FileTree_NoJobsFolder_ForConsole()
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService, ProjectType.Console, database: null);
        var service = new FileTreeService();
        var tree = service.GenerateTree(config);
        var jobsNode = FindNode(tree, "Jobs");
        Assert.Null(jobsNode);
    }

    [Theory] // JOBS-06: ProjectGenerationService output contains sample job file for all non-None BackgroundJobs options
    [InlineData(BackgroundJobsOption.IHostedService)]
    [InlineData(BackgroundJobsOption.Hangfire)]
    [InlineData(BackgroundJobsOption.Quartz)]
    public void JOBS_06_ProjectGenerationService_IncludesJobFile(BackgroundJobsOption jobs)
    {
        var database = jobs == BackgroundJobsOption.Hangfire ? DatabaseOption.PostgreSql : (DatabaseOption?)null;
        var config = CreateConfig(jobs, ProjectType.WebApi, database: database);
        var service = new ProjectGenerationService();
        var files = service.Generate(config);

        var expectedFileName = jobs switch
        {
            BackgroundJobsOption.IHostedService => "SampleBackgroundService.cs",
            BackgroundJobsOption.Hangfire => "SampleHangfireJob.cs",
            BackgroundJobsOption.Quartz => "SampleQuartzJob.cs",
            _ => null
        };

        Assert.Contains(files.Keys, k => k.EndsWith(expectedFileName!));
    }

    // ---- Additional coverage: Worker project packages ----

    [Fact] // JOBS-02: Worker project with IHostedService emits no Hangfire or Quartz packages
    public void JOBS_02_WorkerProject_IHostedService_NoExtraPackages()
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService, ProjectType.WorkerService, database: null);
        var result = CsprojGenerator.GenerateWorkerProject(config);
        Assert.DoesNotContain("Hangfire", result);
        Assert.DoesNotContain("Quartz", result);
    }

    [Fact] // JOBS-05: Worker project with Quartz emits Quartz packages
    public void JOBS_05_WorkerProject_Quartz_Packages()
    {
        var config = CreateConfig(BackgroundJobsOption.Quartz, ProjectType.WorkerService, database: null);
        var result = CsprojGenerator.GenerateWorkerProject(config);
        Assert.Contains("Quartz", result);
        Assert.Contains("Quartz.Extensions.Hosting", result);
        Assert.Contains("Quartz.Extensions.DependencyInjection", result);
    }

    // ---- Negative: Console hides background jobs from csproj ----

    [Fact] // JOBS-01: Console project does not emit Hangfire packages even when BackgroundJobs = Hangfire
    public void JOBS_01_Console_NoHangfirePackages()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.Console, database: DatabaseOption.PostgreSql);
        var result = CsprojGenerator.GenerateConsoleProject(config);
        Assert.DoesNotContain("Hangfire", result);
    }

    [Fact] // JOBS-01: Console project does not emit Quartz packages even when BackgroundJobs = Quartz
    public void JOBS_01_Console_NoQuartzPackages()
    {
        var config = CreateConfig(BackgroundJobsOption.Quartz, ProjectType.Console, database: null);
        var result = CsprojGenerator.GenerateConsoleProject(config);
        Assert.DoesNotContain("Quartz", result);
    }

    // ---- Additional: MinimalApi background jobs ----

    [Fact] // JOBS-02: MinimalApi with IHostedService emits AddHostedService
    public void JOBS_02_MinimalApi_IHostedService_ProgramCs()
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService, ProjectType.MinimalApi, database: null);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddHostedService<SampleBackgroundService>()", result);
    }

    [Fact] // JOBS-05: MinimalApi with Quartz emits AddQuartz and AddQuartzHostedService
    public void JOBS_05_MinimalApi_Quartz_ProgramCs()
    {
        var config = CreateConfig(BackgroundJobsOption.Quartz, ProjectType.MinimalApi, database: null);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddQuartz", result);
        Assert.Contains("AddQuartzHostedService", result);
    }

    // ---- BackgroundJobsGenerator path helpers ----

    [Fact] // JOBS-06: GetFilePath returns SampleBackgroundService.cs path for IHostedService
    public void JOBS_06_GetFilePath_IHostedService()
    {
        var config = CreateConfig(BackgroundJobsOption.IHostedService, ProjectType.WebApi);
        var path = BackgroundJobsGenerator.GetFilePath(config);
        Assert.Contains("SampleBackgroundService.cs", path);
        Assert.Contains("Jobs", path);
    }

    [Fact] // JOBS-06: GetFilePath returns SampleHangfireJob.cs path for Hangfire
    public void JOBS_06_GetFilePath_Hangfire()
    {
        var config = CreateConfig(BackgroundJobsOption.Hangfire, ProjectType.WebApi, database: DatabaseOption.PostgreSql);
        var path = BackgroundJobsGenerator.GetFilePath(config);
        Assert.Contains("SampleHangfireJob.cs", path);
        Assert.Contains("Jobs", path);
    }

    [Fact] // JOBS-06: GetFilePath returns SampleQuartzJob.cs path for Quartz
    public void JOBS_06_GetFilePath_Quartz()
    {
        var config = CreateConfig(BackgroundJobsOption.Quartz, ProjectType.WebApi);
        var path = BackgroundJobsGenerator.GetFilePath(config);
        Assert.Contains("SampleQuartzJob.cs", path);
        Assert.Contains("Jobs", path);
    }

    // ---- CleanArchitecture path uses EntryPointProjectName ----

    [Fact] // JOBS-06: CleanArchitecture IHostedService uses EntryPointProjectName in Jobs path
    public void JOBS_06_CleanArchitecture_IHostedService_FilePath()
    {
        var config = CreateConfig(
            BackgroundJobsOption.IHostedService,
            ProjectType.WebApi,
            architecture: ArchitecturePattern.CleanArchitecture);
        var path = BackgroundJobsGenerator.GetFilePath(config);
        Assert.Contains("TestApp.Api", path);
        Assert.Contains("Jobs/SampleBackgroundService.cs", path);
    }
}
