namespace NetStarter.Models;

public enum DotNetSdkVersion { Net8, Net9, Net10 }
public enum ProjectType { WebApi, MinimalApi, Console, WorkerService }
public enum ArchitecturePattern { CleanArchitecture, VerticalSlice, SimpleLayered }
public enum OrmOption { None, EfCore, Dapper }
public enum DatabaseOption { PostgreSql, SqlServer, MySql, Sqlite }
public enum AuthOption { None, Jwt, AspNetIdentity, Keycloak, ApiKey }
public enum MappingOption { None, Mapster }
public enum TestFrameworkOption { None, XUnit, NUnit }
public enum AssertLibraryOption { None, Shouldly }
public enum LoggingOption { None, Serilog, NLog }
public enum OpenApiUi { None, Scalar, SwaggerUI, Redoc }
public enum BackgroundJobsOption { None, IHostedService, Hangfire, Quartz }

public record ValidationError(string Code, string Message, string[] AffectedFields);

public class ProjectConfiguration
{
    public string ProjectName { get; set; } = "MyProject";
    public string Namespace { get; set; } = "MyProject";
    public DotNetSdkVersion SdkVersion { get; set; } = DotNetSdkVersion.Net10;
    public ProjectType ProjectType { get; set; } = ProjectType.WebApi;
    public ArchitecturePattern Architecture { get; set; } = ArchitecturePattern.CleanArchitecture;
    public OrmOption Orm { get; set; } = OrmOption.None;
    public DatabaseOption? Database { get; set; }
    public AuthOption Auth { get; set; } = AuthOption.None;
    public MappingOption Mapping { get; set; } = MappingOption.None;
    public bool IncludeHealthChecks { get; set; }
    public bool IncludeOpenTelemetry { get; set; }
    public TestFrameworkOption TestFramework { get; set; } = TestFrameworkOption.None;
    public AssertLibraryOption AssertLibrary { get; set; } = AssertLibraryOption.None;
    public bool IncludeTestcontainers { get; set; }
    public bool IncludeDockerfile { get; set; }
    public bool IncludeDockerCompose { get; set; }
    public bool IncludeDotNetAspire { get; set; }
    public bool IncludeGitHubActions { get; set; }
    public bool IncludeAzureDevOps { get; set; }

    // v1.1 enum properties (single-select, default None)
    public LoggingOption Logging { get; set; } = LoggingOption.None;
    public OpenApiUi ApiDocsUi { get; set; } = OpenApiUi.None;
    public BackgroundJobsOption BackgroundJobs { get; set; } = BackgroundJobsOption.None;

    // v1.1 bool properties (combinable options, default false)
    public bool IncludeNSubstitute { get; set; }
    public bool IncludeBogus { get; set; }
    public bool IncludeFluentValidation { get; set; }
    public bool IncludeResilience { get; set; }
    public bool IncludeRedis { get; set; }

    public bool HasTestFramework => TestFramework != TestFrameworkOption.None;

    public string EntryPointSuffix => ProjectType switch
    {
        ProjectType.Console => "Cli",
        ProjectType.WorkerService => "Worker",
        _ => "Api",
    };
    public string EntryPointProjectName => $"{ProjectName}.{EntryPointSuffix}";

    public List<ValidationError> Validate()
    {
        var errors = new List<ValidationError>();

        // Identity requires EF Core (AUTH-02)
        if (Auth == AuthOption.AspNetIdentity && Orm != OrmOption.EfCore)
            errors.Add(new ValidationError(
                "IDENTITY_REQUIRES_EFCORE",
                "ASP.NET Identity requires EF Core. Select EF Core as your ORM.",
                [nameof(Auth), nameof(Orm)]));

        // Authentication requires a web project type
        if (Auth != AuthOption.None && ProjectType is not (ProjectType.WebApi or ProjectType.MinimalApi))
            errors.Add(new ValidationError(
                "AUTH_REQUIRES_WEB",
                "Authentication requires a web project (Web API or Minimal API).",
                [nameof(Auth), nameof(ProjectType)]));

        // HTTP Resilience requires a web project type (RESIL-03)
        if (IncludeResilience && ProjectType is not (ProjectType.WebApi or ProjectType.MinimalApi))
            errors.Add(new ValidationError(
                "RESILIENCE_REQUIRES_WEB",
                "HTTP Resilience requires a web project (Web API or Minimal API).",
                [nameof(IncludeResilience), nameof(ProjectType)]));

        // OpenAPI documentation requires a web project type (DOCS-06)
        if (ApiDocsUi != OpenApiUi.None && ProjectType is not (ProjectType.WebApi or ProjectType.MinimalApi))
            errors.Add(new ValidationError(
                "OPENAPI_REQUIRES_WEB",
                "API documentation requires a web project (Web API or Minimal API).",
                [nameof(ApiDocsUi), nameof(ProjectType)]));

        // Hangfire requires a database (JOBS-03)
        if (BackgroundJobs == BackgroundJobsOption.Hangfire && Database is null)
            errors.Add(new ValidationError(
                "HANGFIRE_REQUIRES_DATABASE",
                "Hangfire requires a database. Select a database option.",
                [nameof(BackgroundJobs), nameof(Database)]));

        return errors;
    }
}
