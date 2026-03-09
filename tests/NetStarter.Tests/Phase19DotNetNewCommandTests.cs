using NetStarter.Models;
using NetStarter.Services;

namespace NetStarter.Tests;

/// <summary>
/// Comprehensive unit tests for DotNetNewCommandService flag mapping.
/// Verifies correct translation from ProjectConfiguration to template.json parameter names.
///
/// CRITICAL PITFALL: Flag emission compares against template.json defaults, NOT Blazor defaults.
/// Examples:
///   - orm: template default is EfCore (Blazor default is None) — so --orm None IS emitted
///   - healthChecks: template default is true (Blazor default is false) — so --healthChecks false IS emitted
///   - validation: template default is true (Blazor default is false) — so --validation false IS emitted
///   - logging: template default is Serilog (Blazor default is None) — so --logging None IS emitted
///
/// Covers: CLI-01, CLI-02, CLI-03.
/// </summary>
public class Phase19DotNetNewCommandTests
{
    private static ProjectConfiguration CreateConfig(
        string projectName = "MyProject",
        string? @namespace = null,
        ProjectType projectType = ProjectType.WebApi,
        ArchitecturePattern architecture = ArchitecturePattern.CleanArchitecture,
        DotNetSdkVersion sdkVersion = DotNetSdkVersion.Net10,
        OrmOption orm = OrmOption.None,
        DatabaseOption? database = null,
        AuthOption auth = AuthOption.None,
        LoggingOption logging = LoggingOption.None,
        BackgroundJobsOption backgroundJobs = BackgroundJobsOption.None,
        OpenApiUi apiDocsUi = OpenApiUi.None,
        bool includeHealthChecks = false,
        bool includeOpenTelemetry = false,
        bool includeRedis = false,
        bool includeFluentValidation = false,
        bool includeResilience = false,
        MappingOption mapping = MappingOption.None,
        TestFrameworkOption testFramework = TestFrameworkOption.None,
        bool includeNSubstitute = false,
        bool includeBogus = false,
        bool includeTestcontainers = false,
        bool includeDockerfile = false,
        bool includeDockerCompose = false,
        bool includeDotNetAspire = false,
        bool includeGitHubActions = false,
        bool includeAzureDevOps = false)
    {
        return new ProjectConfiguration
        {
            ProjectName = projectName,
            Namespace = @namespace ?? projectName,
            ProjectType = projectType,
            Architecture = architecture,
            SdkVersion = sdkVersion,
            Orm = orm,
            Database = database,
            Auth = auth,
            Logging = logging,
            BackgroundJobs = backgroundJobs,
            ApiDocsUi = apiDocsUi,
            IncludeHealthChecks = includeHealthChecks,
            IncludeOpenTelemetry = includeOpenTelemetry,
            IncludeRedis = includeRedis,
            IncludeFluentValidation = includeFluentValidation,
            IncludeResilience = includeResilience,
            Mapping = mapping,
            TestFramework = testFramework,
            IncludeNSubstitute = includeNSubstitute,
            IncludeBogus = includeBogus,
            IncludeTestcontainers = includeTestcontainers,
            IncludeDockerfile = includeDockerfile,
            IncludeDockerCompose = includeDockerCompose,
            IncludeDotNetAspire = includeDotNetAspire,
            IncludeGitHubActions = includeGitHubActions,
            IncludeAzureDevOps = includeAzureDevOps,
        };
    }

    /// <summary>
    /// Returns the full second command string (the dotnet new dotnet-initializr line)
    /// for easier flag assertions.
    /// </summary>
    private static string GetFlagString(List<string> commands) => commands[1];

    // ========== CLI-01: Command structure ==========

    [Fact] // CLI-01: BuildCommands returns exactly 2 commands
    public void BuildCommands_ReturnsExactlyTwoCommands()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig());
        Assert.Equal(2, result.Count);
    }

    [Fact] // CLI-01: First command is exactly the install command
    public void BuildCommands_FirstCommandIsInstall()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig());
        Assert.Equal("dotnet new install Initializr.Templates", result[0]);
    }

    [Fact] // CLI-01: Second command starts with dotnet new dotnet-initializr -n
    public void BuildCommands_SecondCommandStartsWithDotnetNew()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig());
        Assert.StartsWith("dotnet new dotnet-initializr -n", result[1]);
    }

    [Fact] // CLI-01: Project name appears in -n flag of the second command
    public void BuildCommands_ProjectNameAppearsInCommand()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(projectName: "MyProject"));
        Assert.Contains("-n MyProject", GetFlagString(result));
    }

    // ========== CLI-01: Default omission (template.json defaults vs Blazor defaults) ==========

    [Fact] // CLI-01: Default Blazor config (WebApi) differs from template.json default (MinimalApi) — must emit --projectType WebApi
    public void BuildCommands_DefaultBlazerConfig_IncludesProjectTypeWebApi()
    {
        var svc = new DotNetNewCommandService();
        // Blazor defaults: ProjectType = WebApi. Template.json default = MinimalApi.
        var result = svc.BuildCommands(CreateConfig(projectType: ProjectType.WebApi));
        Assert.Contains("--projectType WebApi", GetFlagString(result));
    }

    [Fact] // CLI-01: Default Blazor config (Orm=None) differs from template.json default (EfCore) — must emit --orm None
    public void BuildCommands_DefaultBlazerConfig_IncludesOrmNone()
    {
        var svc = new DotNetNewCommandService();
        // Blazor defaults: Orm = None. Template.json default = EfCore.
        var result = svc.BuildCommands(CreateConfig(orm: OrmOption.None));
        Assert.Contains("--orm None", GetFlagString(result));
    }

    [Fact] // CLI-01: Default Blazor config (Logging=None) differs from template.json default (Serilog) — must emit --logging None
    public void BuildCommands_DefaultBlazerConfig_IncludesLoggingNone()
    {
        var svc = new DotNetNewCommandService();
        // Blazor defaults: Logging = None. Template.json default = Serilog.
        var result = svc.BuildCommands(CreateConfig(logging: LoggingOption.None));
        Assert.Contains("--logging None", GetFlagString(result));
    }

    [Fact] // CLI-01: Default Blazor config (IncludeHealthChecks=false) differs from template.json default (true) — must emit --healthChecks false
    public void BuildCommands_DefaultBlazerConfig_IncludesHealthChecksFalse()
    {
        var svc = new DotNetNewCommandService();
        // Blazor defaults: IncludeHealthChecks = false. Template.json default = true.
        var result = svc.BuildCommands(CreateConfig(includeHealthChecks: false));
        Assert.Contains("--healthChecks false", GetFlagString(result));
    }

    [Fact] // CLI-01: Default Blazor config (IncludeFluentValidation=false) differs from template.json default (true) — must emit --validation false
    public void BuildCommands_DefaultBlazerConfig_IncludesValidationFalse()
    {
        var svc = new DotNetNewCommandService();
        // Blazor defaults: IncludeFluentValidation = false. Template.json default = true.
        var result = svc.BuildCommands(CreateConfig(includeFluentValidation: false));
        Assert.Contains("--validation false", GetFlagString(result));
    }

    [Fact] // CLI-01: Architecture == CleanArchitecture matches template.json default — --arch NOT emitted
    public void BuildCommands_TemplateDefaults_OmitsArch()
    {
        var svc = new DotNetNewCommandService();
        // Both template.json and Blazor default: CleanArchitecture
        var result = svc.BuildCommands(CreateConfig(architecture: ArchitecturePattern.CleanArchitecture));
        Assert.DoesNotContain("--arch", GetFlagString(result));
    }

    [Fact] // CLI-01: SdkVersion == Net10 matches template.json default — --framework NOT emitted
    public void BuildCommands_TemplateDefaults_OmitsFramework()
    {
        var svc = new DotNetNewCommandService();
        // Both template.json and Blazor default: Net10
        var result = svc.BuildCommands(CreateConfig(sdkVersion: DotNetSdkVersion.Net10));
        Assert.DoesNotContain("--framework", GetFlagString(result));
    }

    // ========== CLI-01: Choice parameters ==========

    [Fact] // CLI-01: MinimalApi is template.json default — --projectType NOT emitted
    public void BuildCommands_MinimalApi_OmitsProjectType()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(projectType: ProjectType.MinimalApi));
        Assert.DoesNotContain("--projectType", GetFlagString(result));
    }

    [Fact] // CLI-01: Console is not the template default — --projectType Console IS emitted
    public void BuildCommands_Console_IncludesProjectType()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(projectType: ProjectType.Console));
        Assert.Contains("--projectType Console", GetFlagString(result));
    }

    [Fact] // CLI-01: VerticalSlice is not the template default — --arch VerticalSlice IS emitted
    public void BuildCommands_VerticalSlice_IncludesArch()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(architecture: ArchitecturePattern.VerticalSlice));
        Assert.Contains("--arch VerticalSlice", GetFlagString(result));
    }

    [Fact] // CLI-01: EfCore is template.json default — --orm NOT emitted
    public void BuildCommands_EfCore_OmitsOrm()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(orm: OrmOption.EfCore));
        Assert.DoesNotContain("--orm", GetFlagString(result));
    }

    [Fact] // CLI-01: Dapper is not template default — --orm Dapper IS emitted
    public void BuildCommands_Dapper_IncludesOrm()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(orm: OrmOption.Dapper));
        Assert.Contains("--orm Dapper", GetFlagString(result));
    }

    [Fact] // CLI-01: EfCore + PostgreSql — PostgreSql is template.json default db — --db NOT emitted
    public void BuildCommands_EfCore_PostgreSql_OmitsDb()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(orm: OrmOption.EfCore, database: DatabaseOption.PostgreSql));
        Assert.DoesNotContain("--db", GetFlagString(result));
    }

    [Fact] // CLI-01: EfCore + SqlServer — not template default — --db SqlServer IS emitted
    public void BuildCommands_EfCore_SqlServer_IncludesDb()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(orm: OrmOption.EfCore, database: DatabaseOption.SqlServer));
        Assert.Contains("--db SqlServer", GetFlagString(result));
    }

    [Fact] // CLI-01: Orm=None — no --db flag regardless of Database value
    public void BuildCommands_OrmNone_OmitsDb()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(orm: OrmOption.None, database: DatabaseOption.SqlServer));
        Assert.DoesNotContain("--db", GetFlagString(result));
    }

    [Fact] // CLI-01: Auth=Jwt — --auth Jwt IS emitted
    public void BuildCommands_AuthJwt_IncludesAuth()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(auth: AuthOption.Jwt));
        Assert.Contains("--auth Jwt", GetFlagString(result));
    }

    [Fact] // CLI-01: Logging=Serilog is template.json default — --logging NOT emitted
    public void BuildCommands_Serilog_OmitsLogging()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(logging: LoggingOption.Serilog));
        Assert.DoesNotContain("--logging", GetFlagString(result));
    }

    [Fact] // CLI-01: Logging=NLog is not template default — --logging NLog IS emitted
    public void BuildCommands_NLog_IncludesLogging()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(logging: LoggingOption.NLog));
        Assert.Contains("--logging NLog", GetFlagString(result));
    }

    [Fact] // CLI-01: SdkVersion=Net8 — --framework net8.0 IS emitted
    public void BuildCommands_Net8_IncludesFramework()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(sdkVersion: DotNetSdkVersion.Net8));
        Assert.Contains("--framework net8.0", GetFlagString(result));
    }

    [Fact] // CLI-01: SdkVersion=Net9 — --framework net9.0 IS emitted
    public void BuildCommands_Net9_IncludesFramework()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(sdkVersion: DotNetSdkVersion.Net9));
        Assert.Contains("--framework net9.0", GetFlagString(result));
    }

    // ========== CLI-01: Testing multi-value ==========

    [Fact] // CLI-01: XUnit — --testing xunit IS emitted
    public void BuildCommands_XUnit_IncludesTestingXunit()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(testFramework: TestFrameworkOption.XUnit));
        Assert.Contains("--testing xunit", GetFlagString(result));
    }

    [Fact] // CLI-01: XUnit + NSubstitute + Bogus — three separate --testing flags emitted
    public void BuildCommands_MultipleTestingFlags_EmitsSeparateFlags()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(
            testFramework: TestFrameworkOption.XUnit,
            includeNSubstitute: true,
            includeBogus: true));
        var flags = GetFlagString(result);
        Assert.Contains("--testing xunit", flags);
        Assert.Contains("--testing nsubstitute", flags);
        Assert.Contains("--testing bogus", flags);
    }

    [Fact] // CLI-01: No testing selected — --testing NOT emitted
    public void BuildCommands_NoTestingSelected_OmitsTestingFlag()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(
            testFramework: TestFrameworkOption.None,
            includeNSubstitute: false,
            includeBogus: false,
            includeTestcontainers: false));
        Assert.DoesNotContain("--testing", GetFlagString(result));
    }

    // ========== CLI-01: Containers and CI/CD ==========

    [Fact] // CLI-01: Dockerfile selected — --containers Dockerfile IS emitted
    public void BuildCommands_Dockerfile_IncludesContainers()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeDockerfile: true));
        Assert.Contains("--containers Dockerfile", GetFlagString(result));
    }

    [Fact] // CLI-01: DockerCompose selected — --containers DockerCompose IS emitted
    public void BuildCommands_DockerCompose_IncludesContainers()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeDockerCompose: true));
        Assert.Contains("--containers DockerCompose", GetFlagString(result));
    }

    [Fact] // CLI-01: Aspire selected — --containers Aspire IS emitted
    public void BuildCommands_Aspire_IncludesContainers()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeDotNetAspire: true));
        Assert.Contains("--containers Aspire", GetFlagString(result));
    }

    [Fact] // CLI-01: GitHubActions selected — --cicd GitHubActions IS emitted
    public void BuildCommands_GitHubActions_IncludesCicd()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeGitHubActions: true));
        Assert.Contains("--cicd GitHubActions", GetFlagString(result));
    }

    [Fact] // CLI-01: AzureDevOps selected — --cicd AzureDevOps IS emitted
    public void BuildCommands_AzureDevOps_IncludesCicd()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeAzureDevOps: true));
        Assert.Contains("--cicd AzureDevOps", GetFlagString(result));
    }

    // ========== CLI-01: Boolean flags ==========

    [Fact] // CLI-01: IncludeHealthChecks=true matches template.json default — --healthChecks NOT emitted
    public void BuildCommands_HealthChecksTrue_OmitsHealthChecks()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeHealthChecks: true));
        Assert.DoesNotContain("--healthChecks", GetFlagString(result));
    }

    [Fact] // CLI-01: IncludeOpenTelemetry=true differs from template.json default (false) — --openTelemetry true IS emitted
    public void BuildCommands_OpenTelemetryTrue_IncludesFlag()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeOpenTelemetry: true));
        Assert.Contains("--openTelemetry true", GetFlagString(result));
    }

    [Fact] // CLI-01: IncludeRedis=true (caching) differs from template.json default (false) — --caching true IS emitted
    public void BuildCommands_CachingTrue_IncludesFlag()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeRedis: true));
        Assert.Contains("--caching true", GetFlagString(result));
    }

    [Fact] // CLI-01: IncludeFluentValidation=true matches template.json default — --validation NOT emitted
    public void BuildCommands_ValidationTrue_OmitsFlag()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeFluentValidation: true));
        Assert.DoesNotContain("--validation", GetFlagString(result));
    }

    [Fact] // CLI-01: IncludeResilience=true differs from template.json default (false) — --resilience true IS emitted
    public void BuildCommands_ResilienceTrue_IncludesFlag()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(includeResilience: true));
        Assert.Contains("--resilience true", GetFlagString(result));
    }

    [Fact] // CLI-01: Mapping=Mapster differs from template.json default (None/false) — --mapping true IS emitted
    public void BuildCommands_MappingMapster_IncludesFlag()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(mapping: MappingOption.Mapster));
        Assert.Contains("--mapping true", GetFlagString(result));
    }

    // ========== CLI-01: Namespace ==========

    [Fact] // CLI-01: Namespace differs from project name — --namespace IS emitted
    public void BuildCommands_NamespaceDiffersFromProjectName_IncludesNamespace()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(projectName: "MyProject", @namespace: "MyApp.Core"));
        Assert.Contains("--namespace MyApp.Core", GetFlagString(result));
    }

    [Fact] // CLI-01: Namespace same as project name — --namespace NOT emitted
    public void BuildCommands_NamespaceSameAsProjectName_OmitsNamespace()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(projectName: "MyProject", @namespace: "MyProject"));
        Assert.DoesNotContain("--namespace", GetFlagString(result));
    }

    // ========== CLI-01: Background jobs and API docs ==========

    [Fact] // CLI-01: BackgroundJobs=Hangfire — --backgroundJobs Hangfire IS emitted
    public void BuildCommands_Hangfire_IncludesBackgroundJobs()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(backgroundJobs: BackgroundJobsOption.Hangfire));
        Assert.Contains("--backgroundJobs Hangfire", GetFlagString(result));
    }

    [Fact] // CLI-01: ApiDocsUi=Scalar — --apiDocs Scalar IS emitted
    public void BuildCommands_Scalar_IncludesApiDocs()
    {
        var svc = new DotNetNewCommandService();
        var result = svc.BuildCommands(CreateConfig(apiDocsUi: OpenApiUi.Scalar));
        Assert.Contains("--apiDocs Scalar", GetFlagString(result));
    }

    // ========== CLI-02: Existing service unchanged ==========

    [Fact] // CLI-02: CliCommandService still works — sanity check that the existing service is unmodified
    public void CliCommandService_StillWorks_AfterPhase19()
    {
        var svc = new CliCommandService();
        var config = new ProjectConfiguration
        {
            ProjectName = "SanityCheck",
            Namespace = "SanityCheck",
            SdkVersion = DotNetSdkVersion.Net10,
            ProjectType = ProjectType.WebApi,
            Architecture = ArchitecturePattern.CleanArchitecture,
        };
        var commands = svc.BuildCommands(config);
        Assert.NotNull(commands);
        Assert.NotEmpty(commands);
        // CliCommandService returns many commands for CleanArchitecture (sln, classlibs, refs, etc.)
        Assert.True(commands.Count > 2, $"Expected more than 2 commands but got {commands.Count}");
        Assert.Contains(commands, c => c.StartsWith("dotnet new sln"));
    }
}
