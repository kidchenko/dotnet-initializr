using NetStarter.Models;
using NetStarter.Services.Generation;

namespace NetStarter.Services;

public class CliCommandService
{
    public List<string> BuildCommands(ProjectConfiguration config)
    {
        var commands = new List<string>();
        var tfm = NuGetVersionMap.GetTargetFramework(config.SdkVersion);

        var templateName = config.ProjectType switch
        {
            ProjectType.WebApi => "webapi",
            ProjectType.MinimalApi => "webapi --use-minimal-apis",
            ProjectType.Console => "console",
            ProjectType.WorkerService => "worker",
            _ => "webapi",
        };

        var apiProjectName = config.Architecture == ArchitecturePattern.CleanArchitecture
            ? config.EntryPointProjectName
            : config.ProjectName;

        var srcPath = $"src/{apiProjectName}";

        // Solution
        commands.Add($"mkdir -p src");
        commands.Add($"dotnet new sln -n {config.ProjectName}");

        // Main project
        commands.Add($"dotnet new {templateName} -n {apiProjectName} -o {srcPath} -f {tfm}");

        // Clean Architecture additional class libraries
        if (config.Architecture == ArchitecturePattern.CleanArchitecture)
        {
            commands.Add($"dotnet new classlib -n {config.ProjectName}.Domain -o src/{config.ProjectName}.Domain -f {tfm}");
            commands.Add($"dotnet new classlib -n {config.ProjectName}.Application -o src/{config.ProjectName}.Application -f {tfm}");
            commands.Add($"dotnet new classlib -n {config.ProjectName}.Infrastructure -o src/{config.ProjectName}.Infrastructure -f {tfm}");

            // Project references: Application → Domain, Infrastructure → Application, Entry point → Infrastructure + Application
            commands.Add($"dotnet add src/{config.ProjectName}.Application reference src/{config.ProjectName}.Domain");
            commands.Add($"dotnet add src/{config.ProjectName}.Infrastructure reference src/{config.ProjectName}.Application");
            commands.Add($"dotnet add {srcPath} reference src/{config.ProjectName}.Infrastructure");
            commands.Add($"dotnet add {srcPath} reference src/{config.ProjectName}.Application");
        }

        // EF Core packages
        if (config.Orm == OrmOption.EfCore)
        {
            var efVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore");
            var efDesignVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Design");
            commands.Add($"dotnet add {srcPath} package Microsoft.EntityFrameworkCore --version {efVersion}");
            commands.Add($"dotnet add {srcPath} package Microsoft.EntityFrameworkCore.Design --version {efDesignVersion}");

            if (config.Database == DatabaseOption.PostgreSql)
            {
                var npgsqlVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Npgsql.EntityFrameworkCore.PostgreSQL");
                commands.Add($"dotnet add {srcPath} package Npgsql.EntityFrameworkCore.PostgreSQL --version {npgsqlVersion}");
            }
            else if (config.Database == DatabaseOption.SqlServer)
            {
                var sqlServerVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.SqlServer");
                commands.Add($"dotnet add {srcPath} package Microsoft.EntityFrameworkCore.SqlServer --version {sqlServerVersion}");
            }
        }

        // JWT Bearer
        if (config.Auth == AuthOption.Jwt)
        {
            var jwtVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.AspNetCore.Authentication.JwtBearer");
            commands.Add($"dotnet add {srcPath} package Microsoft.AspNetCore.Authentication.JwtBearer --version {jwtVersion}");
        }

        // Serilog
        if (config.Logging == LoggingOption.Serilog)
        {
            var serilogVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Serilog.AspNetCore");
            var serilogFileVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Serilog.Sinks.File");
            commands.Add($"dotnet add {srcPath} package Serilog.AspNetCore --version {serilogVersion}");
            commands.Add($"dotnet add {srcPath} package Serilog.Sinks.File --version {serilogFileVersion}");
        }

        // NLog
        if (config.Logging == LoggingOption.NLog)
        {
            if (config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
            {
                var nlogWebVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NLog.Web.AspNetCore");
                commands.Add($"dotnet add {srcPath} package NLog.Web.AspNetCore --version {nlogWebVersion}");
            }
            else
            {
                var nlogHostingVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NLog.Extensions.Hosting");
                commands.Add($"dotnet add {srcPath} package NLog.Extensions.Hosting --version {nlogHostingVersion}");
            }
        }

        // Mapster
        if (config.Mapping == MappingOption.Mapster)
        {
            var mapsterVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Mapster");
            commands.Add($"dotnet add {srcPath} package Mapster --version {mapsterVersion}");
        }

        // OpenTelemetry
        if (config.IncludeOpenTelemetry)
        {
            var otelVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Extensions.Hosting");
            var otelProtocolVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Exporter.OpenTelemetryProtocol");
            var otelAspNetCoreVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.AspNetCore");
            var otelHttpVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.Http");
            commands.Add($"dotnet add {srcPath} package OpenTelemetry.Extensions.Hosting --version {otelVersion}");
            commands.Add($"dotnet add {srcPath} package OpenTelemetry.Exporter.OpenTelemetryProtocol --version {otelProtocolVersion}");
            commands.Add($"dotnet add {srcPath} package OpenTelemetry.Instrumentation.AspNetCore --version {otelAspNetCoreVersion}");
            commands.Add($"dotnet add {srcPath} package OpenTelemetry.Instrumentation.Http --version {otelHttpVersion}");
        }

        // Test project
        if (config.HasTestFramework)
        {
            commands.Add($"mkdir -p tests");
            var testProjectName = $"{config.ProjectName}.Tests";
            var testPath = $"tests/{testProjectName}";
            var testTemplate = config.TestFramework == TestFrameworkOption.NUnit ? "nunit" : "xunit";
            commands.Add($"dotnet new {testTemplate} -n {testProjectName} -o {testPath} -f {tfm}");
            commands.Add($"dotnet add {testPath} reference {srcPath}");

            AddTestFrameworkPackages(commands, config, testPath);
            AddAssertLibraryPackages(commands, config, testPath);

            var testSdkVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.NET.Test.Sdk");
            var coverletVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "coverlet.collector");
            commands.Add($"dotnet add {testPath} package Microsoft.NET.Test.Sdk --version {testSdkVersion}");
            commands.Add($"dotnet add {testPath} package coverlet.collector --version {coverletVersion}");
        }

        // Testcontainers integration test project (separate from unit tests)
        if (config.IncludeTestcontainers && config.Database.HasValue)
        {
            if (!config.HasTestFramework)
                commands.Add($"mkdir -p tests");

            var integrationProjectName = $"{config.ProjectName}.IntegrationTests";
            var integrationPath = $"tests/{integrationProjectName}";
            var integrationTemplate = config.TestFramework == TestFrameworkOption.NUnit ? "nunit" : "xunit";
            commands.Add($"dotnet new {integrationTemplate} -n {integrationProjectName} -o {integrationPath} -f {tfm}");
            commands.Add($"dotnet add {integrationPath} reference {srcPath}");

            AddTestFrameworkPackages(commands, config, integrationPath);
            AddAssertLibraryPackages(commands, config, integrationPath);

            var testSdkVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.NET.Test.Sdk");
            var coverletVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "coverlet.collector");
            commands.Add($"dotnet add {integrationPath} package Microsoft.NET.Test.Sdk --version {testSdkVersion}");
            commands.Add($"dotnet add {integrationPath} package coverlet.collector --version {coverletVersion}");

            if (config.Database == DatabaseOption.PostgreSql)
            {
                var tcVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.PostgreSql");
                commands.Add($"dotnet add {integrationPath} package Testcontainers.PostgreSql --version {tcVersion}");
            }
            else if (config.Database == DatabaseOption.SqlServer)
            {
                var tcVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.MsSql");
                commands.Add($"dotnet add {integrationPath} package Testcontainers.MsSql --version {tcVersion}");
            }
        }

        // Add projects to solution
        commands.Add($"dotnet sln add src/{apiProjectName}/{apiProjectName}.csproj");

        if (config.Architecture == ArchitecturePattern.CleanArchitecture)
        {
            commands.Add($"dotnet sln add src/{config.ProjectName}.Domain/{config.ProjectName}.Domain.csproj");
            commands.Add($"dotnet sln add src/{config.ProjectName}.Application/{config.ProjectName}.Application.csproj");
            commands.Add($"dotnet sln add src/{config.ProjectName}.Infrastructure/{config.ProjectName}.Infrastructure.csproj");
        }

        if (config.HasTestFramework)
        {
            var testProjectName = $"{config.ProjectName}.Tests";
            commands.Add($"dotnet sln add tests/{testProjectName}/{testProjectName}.csproj");
        }

        if (config.IncludeTestcontainers && config.Database.HasValue)
        {
            var integrationProjectName = $"{config.ProjectName}.IntegrationTests";
            commands.Add($"dotnet sln add tests/{integrationProjectName}/{integrationProjectName}.csproj");
        }

        return commands;
    }

    private static void AddTestFrameworkPackages(List<string> commands, ProjectConfiguration config, string projectPath)
    {
        switch (config.TestFramework)
        {
            case TestFrameworkOption.XUnit:
                var xunitVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "xunit");
                var xunitRunnerVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "xunit.runner.visualstudio");
                commands.Add($"dotnet add {projectPath} package xunit --version {xunitVersion}");
                commands.Add($"dotnet add {projectPath} package xunit.runner.visualstudio --version {xunitRunnerVersion}");
                break;
            case TestFrameworkOption.NUnit:
                var nunitVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NUnit");
                var nunitAdapterVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NUnit3TestAdapter");
                var nunitAnalyzersVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NUnit.Analyzers");
                commands.Add($"dotnet add {projectPath} package NUnit --version {nunitVersion}");
                commands.Add($"dotnet add {projectPath} package NUnit3TestAdapter --version {nunitAdapterVersion}");
                commands.Add($"dotnet add {projectPath} package NUnit.Analyzers --version {nunitAnalyzersVersion}");
                break;
        }
    }

    private static void AddAssertLibraryPackages(List<string> commands, ProjectConfiguration config, string projectPath)
    {
        if (config.AssertLibrary == AssertLibraryOption.Shouldly)
        {
            var shouldlyVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Shouldly");
            commands.Add($"dotnet add {projectPath} package Shouldly --version {shouldlyVersion}");
        }
    }
}
