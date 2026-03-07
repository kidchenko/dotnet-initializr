using NetStarter.Models;
using NetStarter.Services;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Tests covering Phase 12 tech debt gap closures:
/// LOG-01-CLI (NLog CLI commands), LOG-04-DEV-SETTINGS (NLog dev appsettings),
/// RESP-02-FILETREE and JOBS-03-SQLITE are UI-only gaps verified visually.
/// </summary>
public class Phase12TechDebtTests
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

    // ---- LOG-01-CLI: NLog CLI commands ----

    [Fact] // LOG-01-CLI: WebApi + NLog -> CLI contains "NLog.Web.AspNetCore" dotnet add command
    public void BuildCommands_WebApi_NLog_IncludesNLogWebPackageCommand()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var service = new CliCommandService();
        var commands = service.BuildCommands(config);
        Assert.Contains(commands, cmd => cmd.Contains("NLog.Web.AspNetCore"));
    }

    [Fact] // LOG-01-CLI: MinimalApi + NLog -> CLI contains "NLog.Web.AspNetCore" dotnet add command
    public void BuildCommands_MinimalApi_NLog_IncludesNLogWebPackageCommand()
    {
        var config = CreateWebConfig(c =>
        {
            c.ProjectType = ProjectType.MinimalApi;
            c.Logging = LoggingOption.NLog;
        });
        var service = new CliCommandService();
        var commands = service.BuildCommands(config);
        Assert.Contains(commands, cmd => cmd.Contains("NLog.Web.AspNetCore"));
    }

    [Fact] // LOG-01-CLI: WorkerService + NLog -> CLI contains "NLog.Extensions.Hosting" dotnet add command
    public void BuildCommands_Worker_NLog_IncludesNLogExtensionsHostingCommand()
    {
        var config = CreateWorkerConfig(c => c.Logging = LoggingOption.NLog);
        var service = new CliCommandService();
        var commands = service.BuildCommands(config);
        Assert.Contains(commands, cmd => cmd.Contains("NLog.Extensions.Hosting"));
    }

    [Fact] // LOG-01-CLI: WebApi + NLog -> CLI does NOT contain "NLog.Extensions.Hosting" (wrong package for web)
    public void BuildCommands_WebApi_NLog_DoesNotIncludeNLogExtensionsHosting()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var service = new CliCommandService();
        var commands = service.BuildCommands(config);
        Assert.DoesNotContain(commands, cmd => cmd.Contains("NLog.Extensions.Hosting"));
    }

    [Fact] // LOG-01-CLI: Serilog -> CLI does NOT contain any "NLog" package command
    public void BuildCommands_Serilog_DoesNotIncludeNLogCommands()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.Serilog);
        var service = new CliCommandService();
        var commands = service.BuildCommands(config);
        Assert.DoesNotContain(commands, cmd => cmd.Contains("NLog"));
    }

    // ---- LOG-04-DEV-SETTINGS: NLog development appsettings ----

    [Fact] // LOG-04-DEV-SETTINGS: NLog -> appsettings.Development.json contains "NLog" section key
    public void GenerateAppSettingsDevelopment_NLog_IncludesNLogSection()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = AppSettingsGenerator.GenerateAppSettingsDevelopment(config);
        Assert.Contains("\"NLog\"", result);
    }

    [Fact] // LOG-04-DEV-SETTINGS: NLog -> appsettings.Development.json is not bare empty object {}
    public void GenerateAppSettingsDevelopment_NLog_IsNotEmpty()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.NLog);
        var result = AppSettingsGenerator.GenerateAppSettingsDevelopment(config);
        Assert.NotEqual("{}", result.Trim());
        Assert.Contains("rules", result);
    }

    [Fact] // LOG-04-DEV-SETTINGS: Serilog -> appsettings.Development.json still contains "Serilog" (regression guard)
    public void GenerateAppSettingsDevelopment_Serilog_StillWorks()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.Serilog);
        var result = AppSettingsGenerator.GenerateAppSettingsDevelopment(config);
        Assert.Contains("\"Serilog\"", result);
    }

    [Fact] // LOG-04-DEV-SETTINGS: None logging -> appsettings.Development.json contains "Logging" (regression guard)
    public void GenerateAppSettingsDevelopment_None_ContainsDefaultLogging()
    {
        var config = CreateWebConfig(c => c.Logging = LoggingOption.None);
        var result = AppSettingsGenerator.GenerateAppSettingsDevelopment(config);
        Assert.Contains("\"Logging\"", result);
    }
}
