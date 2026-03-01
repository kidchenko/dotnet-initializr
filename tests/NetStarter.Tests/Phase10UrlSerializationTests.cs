using NetStarter.Models;

namespace NetStarter.Tests;

/// <summary>
/// Round-trip and backward-compatibility tests for URL serialization.
/// Mirrors the exact serialize/deserialize logic from Home.razor's
/// PushStateToUrl() and OnParametersSet() methods.
/// Covers: URL-01 (docs/jobs params), URL-02 (backward compat), URL-03 (resil round-trip).
/// </summary>
public class Phase10UrlSerializationTests
{
    // ---- Helpers that mirror Home.razor logic ----

    /// <summary>
    /// Mirrors PushStateToUrl() from Home.razor.
    /// Returns a dictionary of URL query parameters (null values are omitted from URL).
    /// </summary>
    private static Dictionary<string, string?> SimulateSerialize(ProjectConfiguration config)
    {
        var obsValues = new List<string>();
        if (config.IncludeHealthChecks) obsValues.Add("healthchecks");
        if (config.IncludeOpenTelemetry) obsValues.Add("otel");

        var contValues = new List<string>();
        if (config.IncludeDockerfile) contValues.Add("dockerfile");
        if (config.IncludeDockerCompose) contValues.Add("compose");
        if (config.IncludeDotNetAspire) contValues.Add("aspire");

        var ciValues = new List<string>();
        if (config.IncludeGitHubActions) ciValues.Add("github");
        if (config.IncludeAzureDevOps) ciValues.Add("azdo");

        return new Dictionary<string, string?>
        {
            ["name"] = config.ProjectName != "MyProject" ? config.ProjectName : null,
            ["ns"] = config.Namespace != "MyProject" ? config.Namespace : null,
            ["sdk"] = config.SdkVersion != DotNetSdkVersion.Net10 ? config.SdkVersion.ToString().ToLower() : null,
            ["type"] = config.ProjectType != ProjectType.WebApi ? config.ProjectType.ToString().ToLower() : null,
            ["arch"] = config.Architecture != ArchitecturePattern.CleanArchitecture ? config.Architecture.ToString().ToLower() : null,
            ["orm"] = config.Orm != OrmOption.None ? config.Orm.ToString().ToLower() : null,
            ["db"] = config.Orm != OrmOption.None && config.Database.HasValue ? config.Database.Value.ToString().ToLower() : null,
            ["auth"] = config.Auth != AuthOption.None ? config.Auth.ToString().ToLower() : null,
            ["map"] = config.Mapping != MappingOption.None ? config.Mapping.ToString().ToLower() : null,
            ["logging"] = config.Logging != LoggingOption.None ? config.Logging.ToString().ToLower() : null,
            ["obs"] = obsValues.Count > 0 ? string.Join(",", obsValues) : null,
            ["test"] = config.TestFramework != TestFrameworkOption.None ? config.TestFramework.ToString().ToLower() : null,
            ["assert"] = config.AssertLibrary != AssertLibraryOption.None ? config.AssertLibrary.ToString().ToLower() : null,
            ["tc"] = config.IncludeTestcontainers ? "true" : null,
            ["cont"] = contValues.Count > 0 ? string.Join(",", contValues) : null,
            ["ci"] = ciValues.Count > 0 ? string.Join(",", ciValues) : null,
            ["redis"] = config.IncludeRedis ? "true" : null,
            ["fv"] = config.IncludeFluentValidation ? "true" : null,
            ["mock"] = config.IncludeNSubstitute ? "true" : null,
            ["bogus"] = config.IncludeBogus ? "true" : null,
            ["resil"] = config.IncludeResilience ? "true" : null,
            ["docs"] = config.ApiDocsUi != OpenApiUi.None ? config.ApiDocsUi.ToString().ToLower() : null,
            ["jobs"] = config.BackgroundJobs != BackgroundJobsOption.None ? config.BackgroundJobs.ToString().ToLower() : null,
        };
    }

    /// <summary>
    /// Mirrors OnParametersSet() from Home.razor.
    /// Applies parse logic from a query parameter dictionary to a new ProjectConfiguration.
    /// </summary>
    private static ProjectConfiguration SimulateDeserialize(Dictionary<string, string?> p)
    {
        var config = new ProjectConfiguration();

        if (p.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name))
            config.ProjectName = name;
        if (p.TryGetValue("ns", out var ns) && !string.IsNullOrEmpty(ns))
            config.Namespace = ns;
        if (p.TryGetValue("sdk", out var sdk) && !string.IsNullOrEmpty(sdk) && Enum.TryParse<DotNetSdkVersion>(sdk, true, out var sdkVal))
            config.SdkVersion = sdkVal;
        if (p.TryGetValue("type", out var type) && !string.IsNullOrEmpty(type) && Enum.TryParse<ProjectType>(type, true, out var pt))
            config.ProjectType = pt;
        if (p.TryGetValue("arch", out var arch) && !string.IsNullOrEmpty(arch) && Enum.TryParse<ArchitecturePattern>(arch, true, out var archVal))
            config.Architecture = archVal;
        if (p.TryGetValue("orm", out var orm) && !string.IsNullOrEmpty(orm) && Enum.TryParse<OrmOption>(orm, true, out var ormVal))
            config.Orm = ormVal;
        if (p.TryGetValue("db", out var db) && !string.IsNullOrEmpty(db) && Enum.TryParse<DatabaseOption>(db, true, out var dbVal))
            config.Database = dbVal;
        if (p.TryGetValue("auth", out var auth) && !string.IsNullOrEmpty(auth) && Enum.TryParse<AuthOption>(auth, true, out var authVal))
            config.Auth = authVal;
        if (p.TryGetValue("map", out var map) && !string.IsNullOrEmpty(map) && Enum.TryParse<MappingOption>(map, true, out var mapVal))
            config.Mapping = mapVal;

        // Multi-select: comma-separated obs
        if (p.TryGetValue("obs", out var obs) && !string.IsNullOrEmpty(obs))
        {
            var obsArr = obs.Split(',', StringSplitOptions.RemoveEmptyEntries);
            // Legacy v1.0 compat: obs=serilog → Logging enum
            if (obsArr.Contains("serilog", StringComparer.OrdinalIgnoreCase))
                config.Logging = LoggingOption.Serilog;
            config.IncludeHealthChecks = obsArr.Contains("healthchecks", StringComparer.OrdinalIgnoreCase);
            config.IncludeOpenTelemetry = obsArr.Contains("otel", StringComparer.OrdinalIgnoreCase);
        }
        // New v1.1 param (takes priority over legacy obs=serilog if both present)
        if (p.TryGetValue("logging", out var logging) && !string.IsNullOrEmpty(logging) && Enum.TryParse<LoggingOption>(logging, true, out var loggingVal))
            config.Logging = loggingVal;

        if (p.TryGetValue("test", out var test) && !string.IsNullOrEmpty(test) && Enum.TryParse<TestFrameworkOption>(test, true, out var testFw))
            config.TestFramework = testFw;
        if (p.TryGetValue("assert", out var assert) && !string.IsNullOrEmpty(assert) && Enum.TryParse<AssertLibraryOption>(assert, true, out var assertLib))
            config.AssertLibrary = assertLib;
        if (p.TryGetValue("tc", out var tc) && !string.IsNullOrEmpty(tc) && bool.TryParse(tc, out var tcVal) && tcVal)
            config.IncludeTestcontainers = true;

        if (p.TryGetValue("cont", out var cont) && !string.IsNullOrEmpty(cont))
        {
            var contArr = cont.Split(',', StringSplitOptions.RemoveEmptyEntries);
            config.IncludeDockerfile = contArr.Contains("dockerfile", StringComparer.OrdinalIgnoreCase);
            config.IncludeDockerCompose = contArr.Contains("compose", StringComparer.OrdinalIgnoreCase);
            config.IncludeDotNetAspire = contArr.Contains("aspire", StringComparer.OrdinalIgnoreCase);
        }

        if (p.TryGetValue("ci", out var ci) && !string.IsNullOrEmpty(ci))
        {
            var ciArr = ci.Split(',', StringSplitOptions.RemoveEmptyEntries);
            config.IncludeGitHubActions = ciArr.Contains("github", StringComparer.OrdinalIgnoreCase);
            config.IncludeAzureDevOps = ciArr.Contains("azdo", StringComparer.OrdinalIgnoreCase);
        }

        if (p.TryGetValue("redis", out var redis) && !string.IsNullOrEmpty(redis) && bool.TryParse(redis, out var redisVal) && redisVal)
            config.IncludeRedis = true;
        if (p.TryGetValue("fv", out var fv) && !string.IsNullOrEmpty(fv) && bool.TryParse(fv, out var fvVal) && fvVal)
            config.IncludeFluentValidation = true;
        if (p.TryGetValue("mock", out var mock) && !string.IsNullOrEmpty(mock) && bool.TryParse(mock, out var mockVal) && mockVal)
            config.IncludeNSubstitute = true;
        if (p.TryGetValue("bogus", out var bogus) && !string.IsNullOrEmpty(bogus) && bool.TryParse(bogus, out var bogusVal) && bogusVal)
            config.IncludeBogus = true;
        if (p.TryGetValue("resil", out var resil) && !string.IsNullOrEmpty(resil) && bool.TryParse(resil, out var resilVal) && resilVal)
            config.IncludeResilience = true;

        // v1.2 new params
        if (p.TryGetValue("docs", out var docs) && !string.IsNullOrEmpty(docs) && Enum.TryParse<OpenApiUi>(docs, true, out var docsUi))
            config.ApiDocsUi = docsUi;
        if (p.TryGetValue("jobs", out var jobs) && !string.IsNullOrEmpty(jobs) && Enum.TryParse<BackgroundJobsOption>(jobs, true, out var jobsOpt))
            config.BackgroundJobs = jobsOpt;

        return config;
    }

    // ---- URL-01: docs and jobs params ----

    [Fact] // URL-01: Full round-trip — all non-default fields serialize then deserialize to identical config
    public void URL_01_RoundTrip_AllNonDefaultFields_SerializeDeserializeEquality()
    {
        var original = new ProjectConfiguration
        {
            ProjectName = "RoundTripApp",
            Namespace = "RoundTripApp",
            SdkVersion = DotNetSdkVersion.Net8,
            ProjectType = ProjectType.MinimalApi,
            Architecture = ArchitecturePattern.VerticalSlice,
            Orm = OrmOption.EfCore,
            Database = DatabaseOption.PostgreSql,
            Auth = AuthOption.Jwt,
            Mapping = MappingOption.Mapster,
            Logging = LoggingOption.Serilog,
            IncludeHealthChecks = true,
            IncludeOpenTelemetry = true,
            TestFramework = TestFrameworkOption.XUnit,
            AssertLibrary = AssertLibraryOption.Shouldly,
            IncludeTestcontainers = true,
            IncludeDockerfile = true,
            IncludeDockerCompose = true,
            IncludeDotNetAspire = true,
            IncludeGitHubActions = true,
            IncludeAzureDevOps = true,
            IncludeRedis = true,
            IncludeFluentValidation = true,
            IncludeNSubstitute = true,
            IncludeBogus = true,
            IncludeResilience = true,
            ApiDocsUi = OpenApiUi.Scalar,
            BackgroundJobs = BackgroundJobsOption.Quartz,
        };

        var serialized = SimulateSerialize(original);
        var deserialized = SimulateDeserialize(serialized);

        Assert.Equal(original.ProjectName, deserialized.ProjectName);
        Assert.Equal(original.Namespace, deserialized.Namespace);
        Assert.Equal(original.SdkVersion, deserialized.SdkVersion);
        Assert.Equal(original.ProjectType, deserialized.ProjectType);
        Assert.Equal(original.Architecture, deserialized.Architecture);
        Assert.Equal(original.Orm, deserialized.Orm);
        Assert.Equal(original.Database, deserialized.Database);
        Assert.Equal(original.Auth, deserialized.Auth);
        Assert.Equal(original.Mapping, deserialized.Mapping);
        Assert.Equal(original.Logging, deserialized.Logging);
        Assert.Equal(original.IncludeHealthChecks, deserialized.IncludeHealthChecks);
        Assert.Equal(original.IncludeOpenTelemetry, deserialized.IncludeOpenTelemetry);
        Assert.Equal(original.TestFramework, deserialized.TestFramework);
        Assert.Equal(original.AssertLibrary, deserialized.AssertLibrary);
        Assert.Equal(original.IncludeTestcontainers, deserialized.IncludeTestcontainers);
        Assert.Equal(original.IncludeDockerfile, deserialized.IncludeDockerfile);
        Assert.Equal(original.IncludeDockerCompose, deserialized.IncludeDockerCompose);
        Assert.Equal(original.IncludeDotNetAspire, deserialized.IncludeDotNetAspire);
        Assert.Equal(original.IncludeGitHubActions, deserialized.IncludeGitHubActions);
        Assert.Equal(original.IncludeAzureDevOps, deserialized.IncludeAzureDevOps);
        Assert.Equal(original.IncludeRedis, deserialized.IncludeRedis);
        Assert.Equal(original.IncludeFluentValidation, deserialized.IncludeFluentValidation);
        Assert.Equal(original.IncludeNSubstitute, deserialized.IncludeNSubstitute);
        Assert.Equal(original.IncludeBogus, deserialized.IncludeBogus);
        Assert.Equal(original.IncludeResilience, deserialized.IncludeResilience);
        Assert.Equal(original.ApiDocsUi, deserialized.ApiDocsUi);
        Assert.Equal(original.BackgroundJobs, deserialized.BackgroundJobs);
    }

    [Fact] // URL-01: docs=scalar deserializes to ApiDocsUi = OpenApiUi.Scalar
    public void URL_01_Docs_Scalar_Deserializes()
    {
        var p = new Dictionary<string, string?> { ["docs"] = "scalar" };
        var config = SimulateDeserialize(p);
        Assert.Equal(OpenApiUi.Scalar, config.ApiDocsUi);
    }

    [Fact] // URL-01: docs=swaggerui deserializes to ApiDocsUi = OpenApiUi.SwaggerUI
    public void URL_01_Docs_SwaggerUI_Deserializes()
    {
        var p = new Dictionary<string, string?> { ["docs"] = "swaggerui" };
        var config = SimulateDeserialize(p);
        Assert.Equal(OpenApiUi.SwaggerUI, config.ApiDocsUi);
    }

    [Fact] // URL-01: docs=redoc deserializes to ApiDocsUi = OpenApiUi.Redoc
    public void URL_01_Docs_Redoc_Deserializes()
    {
        var p = new Dictionary<string, string?> { ["docs"] = "redoc" };
        var config = SimulateDeserialize(p);
        Assert.Equal(OpenApiUi.Redoc, config.ApiDocsUi);
    }

    [Fact] // URL-01: No docs param results in ApiDocsUi = OpenApiUi.None (default)
    public void URL_01_Docs_Absent_DefaultsToNone()
    {
        var p = new Dictionary<string, string?>();
        var config = SimulateDeserialize(p);
        Assert.Equal(OpenApiUi.None, config.ApiDocsUi);
    }

    [Fact] // URL-01: jobs=ihostedservice deserializes to BackgroundJobs = BackgroundJobsOption.IHostedService
    public void URL_01_Jobs_IHostedService_Deserializes()
    {
        var p = new Dictionary<string, string?> { ["jobs"] = "ihostedservice" };
        var config = SimulateDeserialize(p);
        Assert.Equal(BackgroundJobsOption.IHostedService, config.BackgroundJobs);
    }

    [Fact] // URL-01: jobs=hangfire deserializes to BackgroundJobs = BackgroundJobsOption.Hangfire
    public void URL_01_Jobs_Hangfire_Deserializes()
    {
        var p = new Dictionary<string, string?> { ["jobs"] = "hangfire" };
        var config = SimulateDeserialize(p);
        Assert.Equal(BackgroundJobsOption.Hangfire, config.BackgroundJobs);
    }

    [Fact] // URL-01: jobs=quartz deserializes to BackgroundJobs = BackgroundJobsOption.Quartz
    public void URL_01_Jobs_Quartz_Deserializes()
    {
        var p = new Dictionary<string, string?> { ["jobs"] = "quartz" };
        var config = SimulateDeserialize(p);
        Assert.Equal(BackgroundJobsOption.Quartz, config.BackgroundJobs);
    }

    [Fact] // URL-01: No jobs param results in BackgroundJobs = BackgroundJobsOption.None (default)
    public void URL_01_Jobs_Absent_DefaultsToNone()
    {
        var p = new Dictionary<string, string?>();
        var config = SimulateDeserialize(p);
        Assert.Equal(BackgroundJobsOption.None, config.BackgroundJobs);
    }

    // ---- URL-02: Backward compatibility ----

    [Fact] // URL-02: Legacy obs=serilog sets Logging = LoggingOption.Serilog
    public void URL_02_LegacyObsSerilog_SetsLogging()
    {
        var p = new Dictionary<string, string?> { ["obs"] = "serilog" };
        var config = SimulateDeserialize(p);
        Assert.Equal(LoggingOption.Serilog, config.Logging);
    }

    [Fact] // URL-02: obs=serilog with logging=nlog — new logging param wins over legacy obs
    public void URL_02_LegacyObsSerilog_LoggingParamOverrides()
    {
        var p = new Dictionary<string, string?> { ["obs"] = "serilog", ["logging"] = "nlog" };
        var config = SimulateDeserialize(p);
        Assert.Equal(LoggingOption.NLog, config.Logging);
    }

    [Fact] // URL-02: Full v1.1 param set (no docs/jobs) parses correctly with ApiDocsUi=None and BackgroundJobs=None
    public void URL_02_AllV11Params_StillParse()
    {
        var p = new Dictionary<string, string?>
        {
            ["name"] = "LegacyApp",
            ["ns"] = "LegacyApp",
            ["sdk"] = "net8",
            ["type"] = "minimalapi",
            ["arch"] = "verticalslice",
            ["orm"] = "efcore",
            ["db"] = "postgresql",
            ["auth"] = "jwt",
            ["map"] = "mapster",
            ["logging"] = "serilog",
            ["obs"] = "healthchecks,otel",
            ["test"] = "xunit",
            ["assert"] = "shouldly",
            ["tc"] = "true",
            ["cont"] = "dockerfile,compose",
            ["ci"] = "github,azdo",
            ["redis"] = "true",
            ["fv"] = "true",
            ["mock"] = "true",
            ["bogus"] = "true",
            ["resil"] = "true",
            // no docs, no jobs
        };

        var config = SimulateDeserialize(p);

        Assert.Equal("LegacyApp", config.ProjectName);
        Assert.Equal("LegacyApp", config.Namespace);
        Assert.Equal(DotNetSdkVersion.Net8, config.SdkVersion);
        Assert.Equal(ProjectType.MinimalApi, config.ProjectType);
        Assert.Equal(ArchitecturePattern.VerticalSlice, config.Architecture);
        Assert.Equal(OrmOption.EfCore, config.Orm);
        Assert.Equal(DatabaseOption.PostgreSql, config.Database);
        Assert.Equal(AuthOption.Jwt, config.Auth);
        Assert.Equal(MappingOption.Mapster, config.Mapping);
        Assert.Equal(LoggingOption.Serilog, config.Logging);
        Assert.True(config.IncludeHealthChecks);
        Assert.True(config.IncludeOpenTelemetry);
        Assert.Equal(TestFrameworkOption.XUnit, config.TestFramework);
        Assert.Equal(AssertLibraryOption.Shouldly, config.AssertLibrary);
        Assert.True(config.IncludeTestcontainers);
        Assert.True(config.IncludeDockerfile);
        Assert.True(config.IncludeDockerCompose);
        Assert.True(config.IncludeGitHubActions);
        Assert.True(config.IncludeAzureDevOps);
        Assert.True(config.IncludeRedis);
        Assert.True(config.IncludeFluentValidation);
        Assert.True(config.IncludeNSubstitute);
        Assert.True(config.IncludeBogus);
        Assert.True(config.IncludeResilience);
        // v1.2 fields default to None when not present
        Assert.Equal(OpenApiUi.None, config.ApiDocsUi);
        Assert.Equal(BackgroundJobsOption.None, config.BackgroundJobs);
    }

    // ---- URL-03: IncludeResilience round-trip ----

    [Fact] // URL-03: resil=true deserializes to IncludeResilience = true
    public void URL_03_ResilTrue_SetsIncludeResilience()
    {
        var p = new Dictionary<string, string?> { ["resil"] = "true" };
        var config = SimulateDeserialize(p);
        Assert.True(config.IncludeResilience);
    }

    [Fact] // URL-03: Config with IncludeResilience=true round-trips correctly via resil=true
    public void URL_03_RoundTrip_IncludesResil()
    {
        var original = new ProjectConfiguration { IncludeResilience = true };
        var serialized = SimulateSerialize(original);
        Assert.Equal("true", serialized["resil"]);
        var deserialized = SimulateDeserialize(serialized);
        Assert.True(deserialized.IncludeResilience);
    }
}
