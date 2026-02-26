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
            ? $"{config.ProjectName}.Api"
            : config.ProjectName;

        // Solution
        commands.Add($"dotnet new sln -n {config.ProjectName}");

        // Main project
        commands.Add($"dotnet new {templateName} -n {apiProjectName} -f {tfm}");

        // Clean Architecture additional class libraries
        if (config.Architecture == ArchitecturePattern.CleanArchitecture)
        {
            commands.Add($"dotnet new classlib -n {config.ProjectName}.Domain -f {tfm}");
            commands.Add($"dotnet new classlib -n {config.ProjectName}.Application -f {tfm}");
            commands.Add($"dotnet new classlib -n {config.ProjectName}.Infrastructure -f {tfm}");
        }

        // EF Core packages
        if (config.Orm == OrmOption.EfCore)
        {
            var efVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore");
            var efDesignVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Design");
            commands.Add($"dotnet add {apiProjectName} package Microsoft.EntityFrameworkCore --version {efVersion}");
            commands.Add($"dotnet add {apiProjectName} package Microsoft.EntityFrameworkCore.Design --version {efDesignVersion}");

            if (config.Database == DatabaseOption.PostgreSql)
            {
                var npgsqlVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Npgsql.EntityFrameworkCore.PostgreSQL");
                commands.Add($"dotnet add {apiProjectName} package Npgsql.EntityFrameworkCore.PostgreSQL --version {npgsqlVersion}");
            }
            else if (config.Database == DatabaseOption.SqlServer)
            {
                var sqlServerVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.SqlServer");
                commands.Add($"dotnet add {apiProjectName} package Microsoft.EntityFrameworkCore.SqlServer --version {sqlServerVersion}");
            }
        }

        // JWT Bearer
        if (config.Auth == AuthOption.Jwt)
        {
            var jwtVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.AspNetCore.Authentication.JwtBearer");
            commands.Add($"dotnet add {apiProjectName} package Microsoft.AspNetCore.Authentication.JwtBearer --version {jwtVersion}");
        }

        // Serilog
        if (config.IncludeSerilog)
        {
            var serilogVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Serilog.AspNetCore");
            var serilogFileVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Serilog.Sinks.File");
            commands.Add($"dotnet add {apiProjectName} package Serilog.AspNetCore --version {serilogVersion}");
            commands.Add($"dotnet add {apiProjectName} package Serilog.Sinks.File --version {serilogFileVersion}");
        }

        // Mapster
        if (config.Mapping == MappingOption.Mapster)
        {
            var mapsterVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Mapster");
            commands.Add($"dotnet add {apiProjectName} package Mapster --version {mapsterVersion}");
        }

        // OpenTelemetry
        if (config.IncludeOpenTelemetry)
        {
            var otelVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Extensions.Hosting");
            var otelProtocolVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Exporter.OpenTelemetryProtocol");
            var otelAspNetCoreVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.AspNetCore");
            var otelHttpVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.Http");
            commands.Add($"dotnet add {apiProjectName} package OpenTelemetry.Extensions.Hosting --version {otelVersion}");
            commands.Add($"dotnet add {apiProjectName} package OpenTelemetry.Exporter.OpenTelemetryProtocol --version {otelProtocolVersion}");
            commands.Add($"dotnet add {apiProjectName} package OpenTelemetry.Instrumentation.AspNetCore --version {otelAspNetCoreVersion}");
            commands.Add($"dotnet add {apiProjectName} package OpenTelemetry.Instrumentation.Http --version {otelHttpVersion}");
        }

        // xUnit test project
        if (config.IncludeXUnit)
        {
            var testProjectName = $"{config.ProjectName}.Tests";
            commands.Add($"dotnet new xunit -n {testProjectName} -f {tfm}");

            var xunitVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "xunit");
            var xunitRunnerVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "xunit.runner.visualstudio");
            var fluentVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "FluentAssertions");
            var testSdkVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.NET.Test.Sdk");
            var coverletVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "coverlet.collector");
            commands.Add($"dotnet add {testProjectName} package xunit --version {xunitVersion}");
            commands.Add($"dotnet add {testProjectName} package xunit.runner.visualstudio --version {xunitRunnerVersion}");
            commands.Add($"dotnet add {testProjectName} package FluentAssertions --version {fluentVersion}");
            commands.Add($"dotnet add {testProjectName} package Microsoft.NET.Test.Sdk --version {testSdkVersion}");
            commands.Add($"dotnet add {testProjectName} package coverlet.collector --version {coverletVersion}");

            // Testcontainers
            if (config.IncludeTestcontainers && config.Database.HasValue)
            {
                if (config.Database == DatabaseOption.PostgreSql)
                {
                    var tcVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.PostgreSql");
                    commands.Add($"dotnet add {testProjectName} package Testcontainers.PostgreSql --version {tcVersion}");
                }
                else if (config.Database == DatabaseOption.SqlServer)
                {
                    var tcVersion = NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.MsSql");
                    commands.Add($"dotnet add {testProjectName} package Testcontainers.MsSql --version {tcVersion}");
                }
            }
        }

        // Add projects to solution
        commands.Add($"dotnet sln add {apiProjectName}/{apiProjectName}.csproj");

        if (config.Architecture == ArchitecturePattern.CleanArchitecture)
        {
            commands.Add($"dotnet sln add {config.ProjectName}.Domain/{config.ProjectName}.Domain.csproj");
            commands.Add($"dotnet sln add {config.ProjectName}.Application/{config.ProjectName}.Application.csproj");
            commands.Add($"dotnet sln add {config.ProjectName}.Infrastructure/{config.ProjectName}.Infrastructure.csproj");
        }

        if (config.IncludeXUnit)
        {
            var testProjectName = $"{config.ProjectName}.Tests";
            commands.Add($"dotnet sln add {testProjectName}/{testProjectName}.csproj");
        }

        return commands;
    }
}
