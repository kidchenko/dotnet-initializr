using NetStarter.Models;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Comprehensive tests covering all Phase 7 LOG and RESIL requirements.
/// NLog logging integration and Polly/Resilience (Microsoft.Extensions.Http.Resilience).
/// </summary>
public class Phase07NLogPollyTests
{
    private static ProjectConfiguration CreateWebConfig(Action<ProjectConfiguration>? configure = null)
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = DotNetSdkVersion.Net8,
            ProjectType = ProjectType.WebApi,
        };
        configure?.Invoke(config);
        return config;
    }

    private static ProjectConfiguration CreateWorkerConfig(Action<ProjectConfiguration>? configure = null)
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = DotNetSdkVersion.Net8,
            ProjectType = ProjectType.WorkerService,
        };
        configure?.Invoke(config);
        return config;
    }

    private static ProjectConfiguration CreateConsoleConfig(Action<ProjectConfiguration>? configure = null)
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = DotNetSdkVersion.Net8,
            ProjectType = ProjectType.Console,
        };
        configure?.Invoke(config);
        return config;
    }

    // ---- LOG-01: Logging picker model ----

    [Fact] // LOG-01: NLog enum value exists in LoggingOption
    public void LoggingOption_NLog_EnumValueExists()
    {
        var nlogValue = LoggingOption.NLog;
        Assert.Equal(LoggingOption.NLog, nlogValue);
    }

    // ---- LOG-02: NLog Web package (WebApi / MinimalApi) ----

    [Fact] // LOG-02: WebApi + NLog -> NLog.Web.AspNetCore package included
    public void GenerateWebProject_NLog_IncludesNLogWebAspNetCore()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("NLog.Web.AspNetCore", result);
    }

    [Fact] // LOG-02: WebApi + NLog -> NLog.Extensions.Hosting NOT included (wrong package for web)
    public void GenerateWebProject_NLog_DoesNotIncludeNLogExtensionsHosting()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("NLog.Extensions.Hosting", result);
    }

    [Fact] // LOG-02: MinimalApi + NLog -> NLog.Web.AspNetCore package included
    public void GenerateWebProject_MinimalApi_NLog_IncludesNLogWebAspNetCore()
    {
        var config = CreateWebConfig(c =>
        {
            c.ProjectType = ProjectType.MinimalApi;
            c.Logging = LoggingOption.NLog;
        });
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("NLog.Web.AspNetCore", result);
    }

    // ---- LOG-03: NLog Worker/Console package (NLog.Extensions.Hosting) ----

    [Fact] // LOG-03: WorkerService + NLog -> NLog.Extensions.Hosting included
    public void GenerateWorkerProject_NLog_IncludesNLogExtensionsHosting()
    {
        var config = CreateWorkerConfig(c => c.Logging = LoggingOption.NLog);
        var result = CsprojGenerator.GenerateWorkerProject(config);
        Assert.Contains("NLog.Extensions.Hosting", result);
    }

    [Fact] // LOG-03: WorkerService + NLog -> NLog.Web.AspNetCore NOT included (wrong package for worker)
    public void GenerateWorkerProject_NLog_DoesNotIncludeNLogWebAspNetCore()
    {
        var config = CreateWorkerConfig(c => c.Logging = LoggingOption.NLog);
        var result = CsprojGenerator.GenerateWorkerProject(config);
        Assert.DoesNotContain("NLog.Web.AspNetCore", result);
    }

    [Fact] // LOG-03: Console + NLog -> NLog.Extensions.Hosting included
    public void GenerateConsoleProject_NLog_IncludesNLogExtensionsHosting()
    {
        var config = CreateConsoleConfig(c => c.Logging = LoggingOption.NLog);
        var result = CsprojGenerator.GenerateConsoleProject(config);
        Assert.Contains("NLog.Extensions.Hosting", result);
    }

    // ---- LOG-04: NLog Program.cs UseNLog ----

    [Fact] // LOG-04: WebApi + NLog -> Program.cs contains builder.Host.UseNLog()
    public void GenerateProgram_WebApi_NLog_IncludesUseNLog()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("builder.Host.UseNLog()", result);
    }

    [Fact] // LOG-04: WebApi + NLog -> Program.cs contains builder.Logging.ClearProviders()
    public void GenerateProgram_WebApi_NLog_IncludesClearProviders()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("builder.Logging.ClearProviders()", result);
    }

    [Fact] // LOG-04: WebApi + NLog -> Program.cs contains using NLog.Web;
    public void GenerateProgram_WebApi_NLog_IncludesUsingNLogWeb()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("using NLog.Web;", result);
    }

    [Fact] // LOG-04: WorkerService + NLog -> Program.cs contains builder.UseNLog() (NOT builder.Host.UseNLog())
    public void GenerateProgram_WorkerService_NLog_IncludesUseNLog()
    {
        var config = CreateWorkerConfig(c => c.Logging = LoggingOption.NLog);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("builder.UseNLog()", result);
    }

    [Fact] // LOG-04: WorkerService + NLog -> Program.cs does NOT contain builder.Host.UseNLog()
    public void GenerateProgram_WorkerService_NLog_DoesNotIncludeHostUseNLog()
    {
        var config = CreateWorkerConfig(c => c.Logging = LoggingOption.NLog);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("builder.Host.UseNLog()", result);
    }

    [Fact] // LOG-04: WorkerService + NLog -> Program.cs contains using NLog.Extensions.Hosting;
    public void GenerateProgram_WorkerService_NLog_IncludesUsingExtensionsHosting()
    {
        var config = CreateWorkerConfig(c => c.Logging = LoggingOption.NLog);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("using NLog.Extensions.Hosting;", result);
    }

    // ---- LOG-05: NLog appsettings.json ----

    [Fact] // LOG-05: NLog config -> appsettings.json contains "NLog" section key
    public void GenerateAppSettings_NLog_IncludesNLogSection()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = AppSettingsGenerator.GenerateAppSettings(config);
        Assert.Contains("\"NLog\"", result);
    }

    [Fact] // LOG-05: NLog config -> appsettings.json contains throwConfigExceptions
    public void GenerateAppSettings_NLog_IncludesThrowConfigExceptions()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = AppSettingsGenerator.GenerateAppSettings(config);
        Assert.Contains("throwConfigExceptions", result);
    }

    [Fact] // LOG-05: NLog config -> appsettings.json contains logconsole target
    public void GenerateAppSettings_NLog_IncludesConsoleTarget()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = AppSettingsGenerator.GenerateAppSettings(config);
        Assert.Contains("console", result);
    }

    [Fact] // LOG-05: NLog config -> appsettings.json contains logfile target
    public void GenerateAppSettings_NLog_IncludesFileTarget()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = AppSettingsGenerator.GenerateAppSettings(config);
        Assert.Contains("logfile", result);
    }

    // ---- Negative/mutual exclusion: Serilog and None do not include NLog ----

    [Fact] // Negative: Serilog config -> csproj does NOT contain NLog packages
    public void GenerateWebProject_Serilog_DoesNotIncludeNLog()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.Serilog);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("NLog", result);
    }

    [Fact] // Negative: No logging config -> csproj does NOT contain NLog packages
    public void GenerateWebProject_NoLogging_DoesNotIncludeNLog()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.None);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("NLog", result);
    }

    // ---- RESIL-01: Resilience UI model (IncludeResilience property) ----

    [Fact] // RESIL-01: New ProjectConfiguration has IncludeResilience defaulting to false
    public void IncludeResilience_DefaultIsFalse()
    {
        var config = new ProjectConfiguration();
        Assert.False(config.IncludeResilience);
    }

    // ---- RESIL-02: Resilience package (Microsoft.Extensions.Http.Resilience) ----

    [Fact] // RESIL-02: WebApi + IncludeResilience -> csproj contains Microsoft.Extensions.Http.Resilience
    public void GenerateWebProject_Resilience_IncludesHttpResilience()
    {
        var config = CreateWebConfig(c => c.IncludeResilience = true);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.Extensions.Http.Resilience", result);
    }

    [Fact] // RESIL-02: MinimalApi + IncludeResilience -> csproj contains Microsoft.Extensions.Http.Resilience
    public void GenerateWebProject_MinimalApi_Resilience_IncludesHttpResilience()
    {
        var config = CreateWebConfig(c =>
        {
            c.ProjectType = ProjectType.MinimalApi;
            c.IncludeResilience = true;
        });
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.Extensions.Http.Resilience", result);
    }

    [Fact] // RESIL-02: SDK-aligned versioning: .NET 8 + Resilience -> version contains "8."
    public void GenerateWebProject_Resilience_VersionIsSdkAligned_Net8()
    {
        var config = CreateWebConfig(c =>
        {
            c.SdkVersion = DotNetSdkVersion.Net8;
            c.IncludeResilience = true;
        });
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("8.", result);
    }

    [Fact] // RESIL-02: SDK-aligned versioning: .NET 10 + Resilience -> version contains "10."
    public void GenerateWebProject_Resilience_VersionIsSdkAligned_Net10()
    {
        var config = CreateWebConfig(c =>
        {
            c.SdkVersion = DotNetSdkVersion.Net10;
            c.IncludeResilience = true;
        });
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("10.", result);
    }

    [Fact] // RESIL-02: WorkerService + IncludeResilience -> Microsoft.Extensions.Http.Resilience NOT included for non-web
    public void GenerateWorkerProject_Resilience_DoesNotIncludeHttpResilience()
    {
        var config = CreateWorkerConfig(c => c.IncludeResilience = true);
        var result = CsprojGenerator.GenerateWorkerProject(config);
        Assert.DoesNotContain("Microsoft.Extensions.Http.Resilience", result);
    }

    // ---- RESIL-03: Resilience Program.cs fragment + no Polly ----

    [Fact] // RESIL-03: WebApi + Resilience -> Program.cs contains AddHttpClient
    public void GenerateProgram_WebApi_Resilience_IncludesAddHttpClient()
    {
        var config = CreateWebConfig(c => c.IncludeResilience = true);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddHttpClient", result);
    }

    [Fact] // RESIL-03: WebApi + Resilience -> Program.cs contains AddStandardResilienceHandler()
    public void GenerateProgram_WebApi_Resilience_IncludesAddStandardResilienceHandler()
    {
        var config = CreateWebConfig(c => c.IncludeResilience = true);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddStandardResilienceHandler()", result);
    }

    [Fact] // RESIL-03: WebApi + Resilience + ProjectName="TestApp" -> Program.cs contains "TestAppClient"
    public void GenerateProgram_WebApi_Resilience_UsesProjectNameInClientName()
    {
        var config = CreateWebConfig(c =>
        {
            c.ProjectName = "TestApp";
            c.IncludeResilience = true;
        });
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("TestAppClient", result);
    }

    [Fact] // RESIL-03: WorkerService + Resilience -> Program.cs does NOT contain AddStandardResilienceHandler
    public void GenerateProgram_WorkerService_Resilience_DoesNotIncludeResilienceCode()
    {
        var config = CreateWorkerConfig(c => c.IncludeResilience = true);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("AddStandardResilienceHandler", result);
    }

    // ---- RESIL-03 negative: Microsoft.Extensions.Http.Polly MUST NEVER appear ----

    [Fact] // RESIL-03: WebApi + Resilience -> NEVER emits Microsoft.Extensions.Http.Polly (forbidden package)
    public void GenerateWebProject_Resilience_NeverEmitsPollyPackage()
    {
        var config = CreateWebConfig(c => c.IncludeResilience = true);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("Microsoft.Extensions.Http.Polly", result);
    }

    [Fact] // RESIL-03: WebApi without IncludeResilience -> Microsoft.Extensions.Http.Resilience NOT included
    public void GenerateWebProject_NoResilience_DoesNotIncludeHttpResilience()
    {
        var config = CreateWebConfig(c => c.IncludeResilience = false);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("Microsoft.Extensions.Http.Resilience", result);
    }

    // ---- Validation: RESILIENCE_REQUIRES_WEB ----

    [Fact] // RESIL-01/RESIL-03: Console + IncludeResilience -> Validate() returns RESILIENCE_REQUIRES_WEB error
    public void Validate_Resilience_NonWebProject_ReturnsError()
    {
        var config = CreateConsoleConfig(c => c.IncludeResilience = true);
        var errors = config.Validate();
        Assert.Contains(errors, e => e.Code == "RESILIENCE_REQUIRES_WEB");
    }

    [Fact] // RESIL-01/RESIL-03: WebApi + IncludeResilience -> Validate() returns no RESILIENCE_REQUIRES_WEB error
    public void Validate_Resilience_WebProject_ReturnsNoError()
    {
        var config = CreateWebConfig(c => c.IncludeResilience = true);
        var errors = config.Validate();
        Assert.DoesNotContain(errors, e => e.Code == "RESILIENCE_REQUIRES_WEB");
    }
}
