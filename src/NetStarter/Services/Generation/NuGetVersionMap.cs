using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class NuGetVersionMap
{
    private static readonly Dictionary<string, Dictionary<DotNetSdkVersion, string>> _versions = new()
    {
        // SDK-major-aligned packages
        ["Microsoft.EntityFrameworkCore"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Microsoft.EntityFrameworkCore.Design"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Npgsql.EntityFrameworkCore.PostgreSQL"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Microsoft.EntityFrameworkCore.SqlServer"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Microsoft.EntityFrameworkCore.Sqlite"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Microsoft.Data.Sqlite"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Microsoft.AspNetCore.Authentication.JwtBearer"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Microsoft.AspNetCore.OpenApi"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Microsoft.Extensions.Caching.StackExchangeRedis"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Microsoft.Extensions.Http.Resilience"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Serilog.AspNetCore"] = new()
        {
            [DotNetSdkVersion.Net8] = "8.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },

        // Independent-versioned packages (same across all SDKs)
        ["Dapper"] = new()
        {
            [DotNetSdkVersion.Net8] = "2.*",
            [DotNetSdkVersion.Net9] = "2.*",
            [DotNetSdkVersion.Net10] = "2.*",
        },
        ["MySqlConnector"] = new()
        {
            [DotNetSdkVersion.Net8] = "2.*",
            [DotNetSdkVersion.Net9] = "2.*",
            [DotNetSdkVersion.Net10] = "2.*",
        },
        // Pomelo: pinned to 9.* — no stable 10.x as of Feb 2026 (per STATE.md decision)
        ["Pomelo.EntityFrameworkCore.MySql"] = new()
        {
            [DotNetSdkVersion.Net8] = "9.*",
            [DotNetSdkVersion.Net9] = "9.*",
            [DotNetSdkVersion.Net10] = "9.*",
        },
        ["StackExchange.Redis"] = new()
        {
            [DotNetSdkVersion.Net8] = "2.*",
            [DotNetSdkVersion.Net9] = "2.*",
            [DotNetSdkVersion.Net10] = "2.*",
        },
        ["NSubstitute"] = new()
        {
            [DotNetSdkVersion.Net8] = "5.*",
            [DotNetSdkVersion.Net9] = "5.*",
            [DotNetSdkVersion.Net10] = "5.*",
        },
        ["NSubstitute.Analyzers.CSharp"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["Bogus"] = new()
        {
            [DotNetSdkVersion.Net8] = "35.*",
            [DotNetSdkVersion.Net9] = "35.*",
            [DotNetSdkVersion.Net10] = "35.*",
        },
        ["FluentValidation"] = new()
        {
            [DotNetSdkVersion.Net8] = "12.*",
            [DotNetSdkVersion.Net9] = "12.*",
            [DotNetSdkVersion.Net10] = "12.*",
        },
        ["FluentValidation.DependencyInjectionExtensions"] = new()
        {
            [DotNetSdkVersion.Net8] = "12.*",
            [DotNetSdkVersion.Net9] = "12.*",
            [DotNetSdkVersion.Net10] = "12.*",
        },
        ["Hangfire"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["Hangfire.AspNetCore"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["Hangfire.SqlServer"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["Hangfire.PostgreSql"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["Hangfire.MySqlStorage"] = new()
        {
            [DotNetSdkVersion.Net8] = "2.*",
            [DotNetSdkVersion.Net9] = "2.*",
            [DotNetSdkVersion.Net10] = "2.*",
        },
        ["Quartz"] = new()
        {
            [DotNetSdkVersion.Net8] = "3.*",
            [DotNetSdkVersion.Net9] = "3.*",
            [DotNetSdkVersion.Net10] = "3.*",
        },
        ["Quartz.Extensions.Hosting"] = new()
        {
            [DotNetSdkVersion.Net8] = "3.*",
            [DotNetSdkVersion.Net9] = "3.*",
            [DotNetSdkVersion.Net10] = "3.*",
        },
        ["Quartz.Extensions.DependencyInjection"] = new()
        {
            [DotNetSdkVersion.Net8] = "3.*",
            [DotNetSdkVersion.Net9] = "3.*",
            [DotNetSdkVersion.Net10] = "3.*",
        },
        ["NLog.Web.AspNetCore"] = new()
        {
            [DotNetSdkVersion.Net8] = "6.*",
            [DotNetSdkVersion.Net9] = "6.*",
            [DotNetSdkVersion.Net10] = "6.*",
        },
        ["Swashbuckle.AspNetCore"] = new()
        {
            [DotNetSdkVersion.Net8] = "10.*",
            [DotNetSdkVersion.Net9] = "10.*",
            [DotNetSdkVersion.Net10] = "10.*",
        },
        ["Scalar.AspNetCore"] = new()
        {
            [DotNetSdkVersion.Net8] = "2.*",
            [DotNetSdkVersion.Net9] = "2.*",
            [DotNetSdkVersion.Net10] = "2.*",
        },
        ["Serilog.Sinks.File"] = new()
        {
            [DotNetSdkVersion.Net8] = "7.*",
            [DotNetSdkVersion.Net9] = "7.*",
            [DotNetSdkVersion.Net10] = "7.*",
        },
        ["OpenTelemetry.Exporter.OpenTelemetryProtocol"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["OpenTelemetry.Extensions.Hosting"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["OpenTelemetry.Instrumentation.AspNetCore"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["OpenTelemetry.Instrumentation.Http"] = new()
        {
            [DotNetSdkVersion.Net8] = "1.*",
            [DotNetSdkVersion.Net9] = "1.*",
            [DotNetSdkVersion.Net10] = "1.*",
        },
        ["Mapster"] = new()
        {
            [DotNetSdkVersion.Net8] = "7.4.*",
            [DotNetSdkVersion.Net9] = "7.4.*",
            [DotNetSdkVersion.Net10] = "7.4.*",
        },
        ["xunit"] = new()
        {
            [DotNetSdkVersion.Net8] = "2.*",
            [DotNetSdkVersion.Net9] = "2.*",
            [DotNetSdkVersion.Net10] = "2.*",
        },
        ["xunit.runner.visualstudio"] = new()
        {
            [DotNetSdkVersion.Net8] = "2.*",
            [DotNetSdkVersion.Net9] = "2.*",
            [DotNetSdkVersion.Net10] = "2.*",
        },
        ["Shouldly"] = new()
        {
            [DotNetSdkVersion.Net8] = "4.*",
            [DotNetSdkVersion.Net9] = "4.*",
            [DotNetSdkVersion.Net10] = "4.*",
        },
        ["NUnit"] = new()
        {
            [DotNetSdkVersion.Net8] = "4.*",
            [DotNetSdkVersion.Net9] = "4.*",
            [DotNetSdkVersion.Net10] = "4.*",
        },
        ["NUnit3TestAdapter"] = new()
        {
            [DotNetSdkVersion.Net8] = "4.*",
            [DotNetSdkVersion.Net9] = "4.*",
            [DotNetSdkVersion.Net10] = "4.*",
        },
        ["NUnit.Analyzers"] = new()
        {
            [DotNetSdkVersion.Net8] = "4.*",
            [DotNetSdkVersion.Net9] = "4.*",
            [DotNetSdkVersion.Net10] = "4.*",
        },
        ["Testcontainers.PostgreSql"] = new()
        {
            [DotNetSdkVersion.Net8] = "4.*",
            [DotNetSdkVersion.Net9] = "4.*",
            [DotNetSdkVersion.Net10] = "4.*",
        },
        ["Testcontainers.MsSql"] = new()
        {
            [DotNetSdkVersion.Net8] = "4.*",
            [DotNetSdkVersion.Net9] = "4.*",
            [DotNetSdkVersion.Net10] = "4.*",
        },
        ["Microsoft.NET.Test.Sdk"] = new()
        {
            [DotNetSdkVersion.Net8] = "17.*",
            [DotNetSdkVersion.Net9] = "17.*",
            [DotNetSdkVersion.Net10] = "17.*",
        },
        ["coverlet.collector"] = new()
        {
            [DotNetSdkVersion.Net8] = "6.*",
            [DotNetSdkVersion.Net9] = "6.*",
            [DotNetSdkVersion.Net10] = "6.*",
        },
    };

    public static string GetPackageVersion(DotNetSdkVersion sdk, string packageName)
    {
        if (_versions.TryGetValue(packageName, out var sdkMap) &&
            sdkMap.TryGetValue(sdk, out var version))
        {
            return version;
        }

        return "*";
    }

    public static string GetTargetFramework(DotNetSdkVersion sdk) => sdk switch
    {
        DotNetSdkVersion.Net8 => "net8.0",
        DotNetSdkVersion.Net9 => "net9.0",
        DotNetSdkVersion.Net10 => "net10.0",
        _ => "net9.0",
    };
}
