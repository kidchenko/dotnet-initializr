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
        if (config.Orm == OrmOption.EfCore)
        {
            packages.Add(("Microsoft.EntityFrameworkCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore")));
            packages.Add(("Microsoft.EntityFrameworkCore.Design", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Design")));

            if (config.Database == DatabaseOption.PostgreSql)
                packages.Add(("Npgsql.EntityFrameworkCore.PostgreSQL", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Npgsql.EntityFrameworkCore.PostgreSQL")));
            else if (config.Database == DatabaseOption.SqlServer)
                packages.Add(("Microsoft.EntityFrameworkCore.SqlServer", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.SqlServer")));
        }

        if (config.Auth == AuthOption.Jwt)
            packages.Add(("Microsoft.AspNetCore.Authentication.JwtBearer", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.AspNetCore.Authentication.JwtBearer")));

        if (config.IncludeSerilog)
        {
            packages.Add(("Serilog.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Serilog.AspNetCore")));
            packages.Add(("Serilog.Sinks.File", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Serilog.Sinks.File")));
        }

        if (config.IncludeOpenTelemetry)
        {
            packages.Add(("OpenTelemetry.Extensions.Hosting", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Extensions.Hosting")));
            packages.Add(("OpenTelemetry.Instrumentation.AspNetCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.AspNetCore")));
            packages.Add(("OpenTelemetry.Instrumentation.Http", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Instrumentation.Http")));
            packages.Add(("OpenTelemetry.Exporter.OpenTelemetryProtocol", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "OpenTelemetry.Exporter.OpenTelemetryProtocol")));

            if (config.Orm == OrmOption.EfCore)
                packages.Add(("OpenTelemetry.Instrumentation.EntityFrameworkCore", "1.*-*"));
        }

        if (config.Mapping == MappingOption.Mapster)
            packages.Add(("Mapster", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Mapster")));
    }

    public static string GenerateClassLibrary(ProjectConfiguration config, string projectSuffix, List<string>? packages = null)
    {
        var sdk = "Microsoft.NET.Sdk";
        var sb = new StringBuilder();
        sb.Append(CommonPropertyGroup(config, sdk));

        var pkgList = new List<(string Name, string Version)>();

        // For Infrastructure layer: add EF Core packages
        if (projectSuffix is "Infrastructure" && config.Orm == OrmOption.EfCore)
        {
            pkgList.Add(("Microsoft.EntityFrameworkCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore")));
            pkgList.Add(("Microsoft.EntityFrameworkCore.Design", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.Design")));

            if (config.Database == DatabaseOption.PostgreSql)
                pkgList.Add(("Npgsql.EntityFrameworkCore.PostgreSQL", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Npgsql.EntityFrameworkCore.PostgreSQL")));
            else if (config.Database == DatabaseOption.SqlServer)
                pkgList.Add(("Microsoft.EntityFrameworkCore.SqlServer", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore.SqlServer")));
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
        packages.Add(("Microsoft.NET.Test.Sdk", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.NET.Test.Sdk")));
        packages.Add(("coverlet.collector", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "coverlet.collector")));
        packages.Add(("Microsoft.EntityFrameworkCore", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Microsoft.EntityFrameworkCore")));

        if (config.Database == DatabaseOption.PostgreSql)
            packages.Add(("Testcontainers.PostgreSql", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.PostgreSql")));
        else if (config.Database == DatabaseOption.SqlServer)
            packages.Add(("Testcontainers.MsSql", NuGetVersionMap.GetPackageVersion(config.SdkVersion, "Testcontainers.MsSql")));

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
