using System.Text;
using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class CsprojGenerator
{
    private static string CommonPropertyGroup(ProjectConfiguration config, string sdk, string? outputType = null, bool isPackable = true, bool isAspireHost = false, bool treatWarningsAsErrors = true)
    {
        var tfm = NuGetVersionMap.GetTargetFramework(config.SdkVersion);
        var sb = new StringBuilder();
        sb.AppendLine($"""<Project Sdk="{sdk}">""");
        sb.AppendLine();
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine($"    <TargetFramework>{tfm}</TargetFramework>");
        if (outputType is not null)
            sb.AppendLine($"    <OutputType>{outputType}</OutputType>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        if (treatWarningsAsErrors)
        {
            sb.AppendLine("    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>");
            sb.AppendLine("    <AnalysisLevel>latest-minimum</AnalysisLevel>");
        }
        if (!isPackable)
            sb.AppendLine("    <IsPackable>false</IsPackable>");
        if (isAspireHost)
            sb.AppendLine("    <IsAspireHost>true</IsAspireHost>");
        sb.AppendLine("  </PropertyGroup>");
        return sb.ToString();
    }

    private static string BuildPackageReferences(List<(string Name, string Version)> packages)
    {
        if (packages.Count == 0) return string.Empty;
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");
        foreach (var (name, version) in packages)
        {
            sb.AppendLine($"    <PackageReference Include=\"{name}\" Version=\"{version}\" />");
        }
        sb.AppendLine("  </ItemGroup>");
        return sb.ToString();
    }

    private static string BuildProjectReference(string projectPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine($"    <ProjectReference Include=\"{projectPath}\" />");
        sb.AppendLine("  </ItemGroup>");
        return sb.ToString();
    }

    public static string GenerateWebProject(ProjectConfiguration config, List<string>? additionalPackages = null)
    {
        var sdk = "Microsoft.NET.Sdk.Web";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk));

        var packages = new List<(string Name, string Version)>();
        AppendFeaturePackages(config, packages);

        // Additional packages (caller-supplied)
        if (additionalPackages is not null)
        {
            foreach (var pkg in additionalPackages)
                packages.Add((pkg, "*"));
        }

        sb.Append(BuildPackageReferences(packages));
        sb.AppendLine();
        sb.Append("</Project>");
        return sb.ToString();
    }

    private static void AppendFeaturePackages(ProjectConfiguration config, List<(string Name, string Version)> packages)
    {
        // EF Core and Dapper packages: skip for Clean Architecture (owned by Infrastructure layer)
        if (config.Architecture != ArchitecturePattern.CleanArchitecture)
        {
            if (config.Orm == OrmOption.EfCore)
            {
                packages.Add(("Microsoft.EntityFrameworkCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore")));
                packages.Add(("Microsoft.EntityFrameworkCore.Design", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Design")));

                if (config.Database == DatabaseOption.PostgreSql)
                    packages.Add(("Npgsql.EntityFrameworkCore.PostgreSQL", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Npgsql.EntityFrameworkCore.PostgreSQL")));
                else if (config.Database == DatabaseOption.SqlServer)
                    packages.Add(("Microsoft.EntityFrameworkCore.SqlServer", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.SqlServer")));
                else if (config.Database == DatabaseOption.MySql)
                    packages.Add(("Pomelo.EntityFrameworkCore.MySql", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Pomelo.EntityFrameworkCore.MySql")));
                else if (config.Database == DatabaseOption.Sqlite)
                    packages.Add(("Microsoft.EntityFrameworkCore.Sqlite", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Sqlite")));
            }

            if (config.Orm == OrmOption.Dapper)
            {
                packages.Add(("Dapper", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Dapper")));
                var driver = config.Database switch
                {
                    DatabaseOption.MySql => "MySqlConnector",
                    DatabaseOption.Sqlite => "Microsoft.Data.Sqlite",
                    DatabaseOption.SqlServer => "Microsoft.Data.SqlClient",
                    _ => "Npgsql",  // PostgreSQL default
                };
                packages.Add((driver, NuGetVersionMap.GetPackageVersion(config.SdkVersion, driver)));
            }

            if (config.Auth == AuthOption.AspNetIdentity && config.Orm == OrmOption.EfCore)
            {
                packages.Add(("Microsoft.AspNetCore.Identity.EntityFrameworkCore",
                    NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.AspNetCore.Identity.EntityFrameworkCore")));
            }
        }

        if (config.Auth is AuthOption.Jwt or AuthOption.Keycloak)
            packages.Add(("Microsoft.AspNetCore.Authentication.JwtBearer", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.AspNetCore.Authentication.JwtBearer")));

        if (config.Logging == LoggingOption.Serilog)
        {
            packages.Add(("Serilog.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Serilog.AspNetCore")));
            packages.Add(("Serilog.Sinks.File", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Serilog.Sinks.File")));
        }

        if (config.Logging == LoggingOption.NLog)
        {
            var isWebProject = config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi;
            var nlogPackage = isWebProject ? "NLog.Web.AspNetCore" : "NLog.Extensions.Hosting";
            packages.Add((nlogPackage, NuGetVersionMap.GetPackageVersion(config.SdkVersion, nlogPackage)));
        }

        if (config.IncludeResilience && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi
            && config.Architecture != ArchitecturePattern.CleanArchitecture)
        {
            packages.Add(("Microsoft.Extensions.Http.Resilience",
                NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.Extensions.Http.Resilience")));
        }

        if (config.IncludeOpenTelemetry && config.Architecture != ArchitecturePattern.CleanArchitecture)
        {
            packages.Add(("OpenTelemetry.Extensions.Hosting", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Extensions.Hosting")));
            packages.Add(("OpenTelemetry.Instrumentation.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.AspNetCore")));
            packages.Add(("OpenTelemetry.Instrumentation.Http", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.Http")));
            packages.Add(("OpenTelemetry.Exporter.Console", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Exporter.Console")));
            packages.Add(("OpenTelemetry.Exporter.OpenTelemetryProtocol", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Exporter.OpenTelemetryProtocol")));

            if (config.Orm == OrmOption.EfCore)
                packages.Add(("OpenTelemetry.Instrumentation.EntityFrameworkCore", "1.*-*"));
        }

        if (config.IncludeRedis && config.Architecture != ArchitecturePattern.CleanArchitecture)
        {
            packages.Add(("Microsoft.Extensions.Caching.StackExchangeRedis", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.Extensions.Caching.StackExchangeRedis")));
        }

        // FluentValidation packages: skip for Clean Architecture (owned by Application layer)
        if (config.IncludeFluentValidation && config.Architecture != ArchitecturePattern.CleanArchitecture)
        {
            packages.Add(("FluentValidation", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "FluentValidation")));
            packages.Add(("FluentValidation.DependencyInjectionExtensions", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "FluentValidation.DependencyInjectionExtensions")));
        }

        // Mapster packages: skip for Clean Architecture (owned by Application layer)
        if (config.Mapping == MappingOption.Mapster && config.Architecture != ArchitecturePattern.CleanArchitecture)
        {
            packages.Add(("Mapster", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Mapster")));
            packages.Add(("Mapster.DependencyInjection", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Mapster.DependencyInjection")));
        }

        // OpenAPI Documentation packages
        if (config.ApiDocsUi != OpenApiUi.None && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        {
            var isNet8SwaggerUi = config.ApiDocsUi == OpenApiUi.SwaggerUI
                                  && config.SdkVersion == DotNetSdkVersion.Net8;

            if (isNet8SwaggerUi)
            {
                // .NET 8 SwaggerUI: classic Swashbuckle — no Microsoft.AspNetCore.OpenApi
                packages.Add(("Swashbuckle.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Swashbuckle.AspNetCore")));
            }
            else
            {
                // All other combos use Microsoft.AspNetCore.OpenApi
                packages.Add(("Microsoft.AspNetCore.OpenApi", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.AspNetCore.OpenApi")));

                switch (config.ApiDocsUi)
                {
                    case OpenApiUi.Scalar:
                        packages.Add(("Scalar.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Scalar.AspNetCore")));
                        break;
                    case OpenApiUi.SwaggerUI:
                        // .NET 9/10: UI-only sub-package + Microsoft.OpenApi pin
                        packages.Add(("Swashbuckle.AspNetCore.SwaggerUI", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Swashbuckle.AspNetCore.SwaggerUI")));
                        packages.Add(("Microsoft.OpenApi", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.OpenApi")));
                        break;
                    case OpenApiUi.Redoc:
                        packages.Add(("Swashbuckle.AspNetCore.ReDoc", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Swashbuckle.AspNetCore.ReDoc")));
                        // Microsoft.OpenApi pin only on .NET 9/10
                        if (config.SdkVersion != DotNetSdkVersion.Net8)
                            packages.Add(("Microsoft.OpenApi", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.OpenApi")));
                        break;
                }
            }
        }

        // Background Jobs packages
        // For Clean Architecture, packages go on Infrastructure (except Hangfire.AspNetCore for dashboard middleware)
        if (config.BackgroundJobs != BackgroundJobsOption.None
            && config.ProjectType != ProjectType.Console)
        {
            if (config.Architecture == ArchitecturePattern.CleanArchitecture)
            {
                // Only Hangfire.AspNetCore stays in entry point (dashboard middleware)
                if (config.BackgroundJobs == BackgroundJobsOption.Hangfire && config.Database.HasValue)
                {
                    packages.Add(("Hangfire.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Hangfire.AspNetCore")));
                }
                // Quartz and IHostedService: no entry point packages needed (transitive from Infrastructure)
            }
            else
            {
                // Non-Clean Architecture: all packages in the single project
                if (config.BackgroundJobs == BackgroundJobsOption.Hangfire && config.Database.HasValue)
                {
                    packages.Add(("Hangfire", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Hangfire")));
                    packages.Add(("Hangfire.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Hangfire.AspNetCore")));

                    var storagePackage = config.Database switch
                    {
                        DatabaseOption.PostgreSql => "Hangfire.PostgreSql",
                        DatabaseOption.SqlServer  => "Hangfire.SqlServer",
                        DatabaseOption.MySql      => "Hangfire.MySqlStorage",
                        _                         => null  // SQLite: no official Hangfire storage
                    };
                    if (storagePackage is not null)
                        packages.Add((storagePackage, NuGetVersionMap.GetPackageVersion(config.SdkVersion, storagePackage)));
                }
                else if (config.BackgroundJobs == BackgroundJobsOption.Quartz)
                {
                    packages.Add(("Quartz", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Quartz")));
                    packages.Add(("Quartz.Extensions.Hosting", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Quartz.Extensions.Hosting")));
                    packages.Add(("Quartz.Extensions.DependencyInjection", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Quartz.Extensions.DependencyInjection")));
                }
                // IHostedService: no NuGet packages needed
            }
        }
    }

    public static string GenerateClassLibrary(ProjectConfiguration config, string projectSuffix, List<string>? packages = null)
    {
        var sdk = "Microsoft.NET.Sdk";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk));

        var pkgList = new List<(string Name, string Version)>();

        // For Application layer: add Mapster package
        if (projectSuffix is "Application" && config.Mapping == MappingOption.Mapster)
        {
            pkgList.Add(("Mapster", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Mapster")));
            pkgList.Add(("Mapster.DependencyInjection", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Mapster.DependencyInjection")));
        }

        // For Application layer: add FluentValidation package
        if (projectSuffix is "Application" && config.IncludeFluentValidation)
        {
            pkgList.Add(("FluentValidation", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "FluentValidation")));
            pkgList.Add(("FluentValidation.DependencyInjectionExtensions", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "FluentValidation.DependencyInjectionExtensions")));
        }

        // For Infrastructure layer: add EF Core packages
        if (projectSuffix is "Infrastructure" && config.Orm == OrmOption.EfCore)
        {
            pkgList.Add(("Microsoft.EntityFrameworkCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore")));
            pkgList.Add(("Microsoft.EntityFrameworkCore.Design", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Design")));

            if (config.Database == DatabaseOption.PostgreSql)
                pkgList.Add(("Npgsql.EntityFrameworkCore.PostgreSQL", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Npgsql.EntityFrameworkCore.PostgreSQL")));
            else if (config.Database == DatabaseOption.SqlServer)
                pkgList.Add(("Microsoft.EntityFrameworkCore.SqlServer", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.SqlServer")));
            else if (config.Database == DatabaseOption.MySql)
                pkgList.Add(("Pomelo.EntityFrameworkCore.MySql", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Pomelo.EntityFrameworkCore.MySql")));
            else if (config.Database == DatabaseOption.Sqlite)
                pkgList.Add(("Microsoft.EntityFrameworkCore.Sqlite", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Sqlite")));

            if (config.Auth == AuthOption.AspNetIdentity)
                pkgList.Add(("Microsoft.AspNetCore.Identity.EntityFrameworkCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.AspNetCore.Identity.EntityFrameworkCore")));
        }

        // For Infrastructure layer: add Dapper packages
        if (projectSuffix is "Infrastructure" && config.Orm == OrmOption.Dapper)
        {
            pkgList.Add(("Dapper", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Dapper")));
            pkgList.Add(("Microsoft.Extensions.DependencyInjection", "*"));
            pkgList.Add(("Microsoft.Extensions.Configuration", "*"));
            var driver = config.Database switch
            {
                DatabaseOption.MySql => "MySqlConnector",
                DatabaseOption.Sqlite => "Microsoft.Data.Sqlite",
                DatabaseOption.SqlServer => "Microsoft.Data.SqlClient",
                _ => "Npgsql",  // PostgreSQL default
            };
            pkgList.Add((driver, NuGetVersionMap.GetPackageVersion(config.SdkVersion, driver)));
        }

        // For Infrastructure layer: add OpenTelemetry packages
        if (projectSuffix is "Infrastructure" && config.IncludeOpenTelemetry)
        {
            pkgList.Add(("OpenTelemetry.Extensions.Hosting", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Extensions.Hosting")));
            pkgList.Add(("OpenTelemetry.Instrumentation.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.AspNetCore")));
            pkgList.Add(("OpenTelemetry.Instrumentation.Http", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.Http")));
            pkgList.Add(("OpenTelemetry.Exporter.OpenTelemetryProtocol", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Exporter.OpenTelemetryProtocol")));

            if (config.Orm == OrmOption.EfCore)
                pkgList.Add(("OpenTelemetry.Instrumentation.EntityFrameworkCore", "1.*-*"));
        }

        // For Infrastructure layer: add Redis package
        if (projectSuffix is "Infrastructure" && config.IncludeRedis)
        {
            pkgList.Add(("Microsoft.Extensions.Caching.StackExchangeRedis", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.Extensions.Caching.StackExchangeRedis")));
        }

        // For Infrastructure layer: add Resilience package
        if (projectSuffix is "Infrastructure" && config.IncludeResilience
            && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        {
            pkgList.Add(("Microsoft.Extensions.Http.Resilience",
                NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.Extensions.Http.Resilience")));
        }

        // For Infrastructure layer: add Background Jobs packages
        if (projectSuffix is "Infrastructure"
            && config.BackgroundJobs != BackgroundJobsOption.None
            && config.ProjectType != ProjectType.Console)
        {
            if (config.BackgroundJobs == BackgroundJobsOption.IHostedService)
            {
                pkgList.Add(("Microsoft.Extensions.Hosting.Abstractions", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.Extensions.Hosting.Abstractions")));
            }
            else if (config.BackgroundJobs == BackgroundJobsOption.Hangfire && config.Database.HasValue)
            {
                pkgList.Add(("Hangfire", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Hangfire")));

                var storagePackage = config.Database switch
                {
                    DatabaseOption.PostgreSql => "Hangfire.PostgreSql",
                    DatabaseOption.SqlServer  => "Hangfire.SqlServer",
                    DatabaseOption.MySql      => "Hangfire.MySqlStorage",
                    _                         => null
                };
                if (storagePackage is not null)
                    pkgList.Add((storagePackage, NuGetVersionMap.GetPackageVersion(config.SdkVersion, storagePackage)));
            }
            else if (config.BackgroundJobs == BackgroundJobsOption.Quartz)
            {
                pkgList.Add(("Quartz", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Quartz")));
                pkgList.Add(("Quartz.Extensions.Hosting", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Quartz.Extensions.Hosting")));
                pkgList.Add(("Quartz.Extensions.DependencyInjection", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Quartz.Extensions.DependencyInjection")));
            }
        }

        if (packages is not null)
        {
            foreach (var pkg in packages)
                pkgList.Add((pkg, "*"));
        }

        sb.Append(BuildPackageReferences(pkgList));
        sb.AppendLine();
        sb.Append("</Project>");
        return sb.ToString();
    }

    public static string GenerateTestProject(ProjectConfiguration config, string mainProjectPath)
    {
        var sdk = "Microsoft.NET.Sdk";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk, isPackable: false, treatWarningsAsErrors: false));

        var packages = new List<(string Name, string Version)>();
        AppendTestFrameworkPackages(config, packages);
        AppendAssertLibraryPackages(config, packages);
        AppendMockingPackages(config, packages);
        packages.Add(("Microsoft.NET.Test.Sdk", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.NET.Test.Sdk")));
        packages.Add(("coverlet.collector", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "coverlet.collector")));

        sb.Append(BuildPackageReferences(packages));
        sb.Append(BuildProjectReference(mainProjectPath));
        sb.AppendLine();
        sb.Append("</Project>");
        return sb.ToString();
    }

    public static string GenerateIntegrationTestProject(ProjectConfiguration config, string mainProjectPath)
    {
        var sdk = "Microsoft.NET.Sdk";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk, isPackable: false, treatWarningsAsErrors: false));

        var packages = new List<(string Name, string Version)>();
        AppendTestFrameworkPackages(config, packages);
        AppendAssertLibraryPackages(config, packages);
        AppendMockingPackages(config, packages);
        packages.Add(("Microsoft.NET.Test.Sdk", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.NET.Test.Sdk")));
        packages.Add(("coverlet.collector", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "coverlet.collector")));
        if (config.Orm == OrmOption.EfCore)
            packages.Add(("Microsoft.EntityFrameworkCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore")));

        if (config.Database == DatabaseOption.PostgreSql)
            packages.Add(("Testcontainers.PostgreSql", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.PostgreSql")));
        else if (config.Database == DatabaseOption.SqlServer)
            packages.Add(("Testcontainers.MsSql", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.MsSql")));
        else if (config.Database == DatabaseOption.MySql)
            packages.Add(("Testcontainers.MySql", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.MySql")));
        else if (config.Database == DatabaseOption.Sqlite && config.Orm == OrmOption.EfCore)
            packages.Add(("Microsoft.EntityFrameworkCore.Sqlite", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Sqlite")));

        sb.Append(BuildPackageReferences(packages));
        sb.Append(BuildProjectReference(mainProjectPath));
        sb.AppendLine();
        sb.Append("</Project>");
        return sb.ToString();
    }

    private static void AppendTestFrameworkPackages(ProjectConfiguration config, List<(string Name, string Version)> packages)
    {
        switch (config.TestFramework)
        {
            case TestFrameworkOption.XUnit:
                packages.Add(("xunit", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "xunit")));
                packages.Add(("xunit.runner.visualstudio", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "xunit.runner.visualstudio")));
                break;
            case TestFrameworkOption.NUnit:
                packages.Add(("NUnit", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NUnit")));
                packages.Add(("NUnit3TestAdapter", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NUnit3TestAdapter")));
                packages.Add(("NUnit.Analyzers", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NUnit.Analyzers")));
                break;
        }
    }

    private static void AppendAssertLibraryPackages(ProjectConfiguration config, List<(string Name, string Version)> packages)
    {
        if (config.AssertLibrary == AssertLibraryOption.Shouldly)
            packages.Add(("Shouldly", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Shouldly")));
    }

    private static void AppendMockingPackages(ProjectConfiguration config, List<(string Name, string Version)> packages)
    {
        if (config.IncludeNSubstitute)
        {
            packages.Add(("NSubstitute", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NSubstitute")));
            packages.Add(("NSubstitute.Analyzers.CSharp", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "NSubstitute.Analyzers.CSharp")));
        }
        if (config.IncludeBogus)
        {
            packages.Add(("Bogus", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Bogus")));
        }
    }

    public static string GenerateConsoleProject(ProjectConfiguration config)
    {
        var sdk = "Microsoft.NET.Sdk";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk, outputType: "Exe"));

        var packages = new List<(string Name, string Version)>
        {
            ("Microsoft.Extensions.DependencyInjection", "*"),
            ("Microsoft.Extensions.Configuration", "*"),
            ("Microsoft.Extensions.Configuration.Json", "*"),
        };

        AppendFeaturePackages(config, packages);
        sb.Append(BuildPackageReferences(packages));

        // Console SDK doesn't auto-include appsettings as content
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <Content Include=\"appsettings.json\" CopyToOutputDirectory=\"PreserveNewest\" />");
        sb.AppendLine("    <Content Include=\"appsettings.Development.json\" CopyToOutputDirectory=\"PreserveNewest\" />");
        sb.AppendLine("  </ItemGroup>");

        sb.AppendLine();
        sb.Append("</Project>");
        return sb.ToString();
    }

    public static string GenerateWorkerProject(ProjectConfiguration config)
    {
        var sdk = "Microsoft.NET.Sdk.Worker";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk));

        var packages = new List<(string Name, string Version)>
        {
            ("Microsoft.Extensions.Hosting", "*"),
        };

        AppendFeaturePackages(config, packages);
        sb.Append(BuildPackageReferences(packages));
        sb.AppendLine();
        sb.Append("</Project>");
        return sb.ToString();
    }

    public static string GenerateAspireAppHostProject(ProjectConfiguration config)
    {
        var sdk = "Aspire.AppHost.Sdk/9.2.0";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk, outputType: "Exe", isAspireHost: true));

        // ProjectReference to main API project
        var mainProjectPath = config.Architecture switch
        {
            ArchitecturePattern.CleanArchitecture => $"../{config.ProjectName}.Api/{config.ProjectName}.Api.csproj",
            _ => $"../{config.ProjectName}/{config.ProjectName}.csproj",
        };

        sb.Append(BuildProjectReference(mainProjectPath));
        sb.AppendLine();
        sb.Append("</Project>");
        return sb.ToString();
    }

    public static string GenerateAspireServiceDefaultsProject(ProjectConfiguration config)
    {
        var sdk = "Microsoft.NET.Sdk";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk));

        var packages = new List<(string Name, string Version)>
        {
            ("Microsoft.Extensions.Http.Resilience", "*"),
            ("Microsoft.Extensions.ServiceDiscovery", "*"),
            ("OpenTelemetry.Extensions.Hosting", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Extensions.Hosting")),
            ("OpenTelemetry.Instrumentation.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.AspNetCore")),
            ("OpenTelemetry.Instrumentation.Http", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.Http")),
        };

        sb.Append(BuildPackageReferences(packages));
        sb.AppendLine();
        sb.Append("</Project>");
        return sb.ToString();
    }
}
