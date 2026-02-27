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
        ["Microsoft.AspNetCore.Authentication.JwtBearer"] = new()
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
