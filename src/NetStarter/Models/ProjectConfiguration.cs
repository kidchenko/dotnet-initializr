namespace NetStarter.Models;

public enum DotNetSdkVersion { Net8, Net9, Net10 }
public enum ProjectType { WebApi, MinimalApi, Console, WorkerService }
public enum ArchitecturePattern { CleanArchitecture, VerticalSlice, SimpleLayered }
public enum OrmOption { None, EfCore }
public enum DatabaseOption { PostgreSql, SqlServer }
public enum AuthOption { None, Jwt }
public enum MappingOption { None, Mapster }
public enum TestFrameworkOption { None, XUnit, NUnit }
public enum AssertLibraryOption { None, Shouldly }

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
    public bool IncludeSerilog { get; set; }
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

    public bool HasTestFramework => TestFramework != TestFrameworkOption.None;

    public string EntryPointSuffix => ProjectType == ProjectType.Console ? "Cli" : "Api";
    public string EntryPointProjectName => $"{ProjectName}.{EntryPointSuffix}";
}
