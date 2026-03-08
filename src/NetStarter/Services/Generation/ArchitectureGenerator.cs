using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class ArchitectureGenerator
{
    public static void GenerateCleanArchitecture(ProjectConfiguration config, Dictionary<string, string> files, string root)
    {
        var name = config.ProjectName;
        var src = $"{root}/src";

        // Domain project — pure domain model, no external dependencies
        files[$"{src}/{name}.Domain/{name}.Domain.csproj"] =
            CsprojGenerator.GenerateClassLibrary(config, "Domain");

        if (config.Orm == OrmOption.EfCore)
        {
            var entityNs = EfCoreGenerator.GetEntityNamespaceSuffix(ArchitecturePattern.CleanArchitecture);
            files[$"{src}/{name}.Domain/Entities/SampleEntity.cs"] =
                EfCoreGenerator.GenerateSampleEntity(config, entityNs);
        }

        // Application project — references Domain
        var appCsproj = CsprojGenerator.GenerateClassLibrary(config, "Application");
        appCsproj = InjectProjectReferences(appCsproj, [$"../{name}.Domain/{name}.Domain.csproj"]);
        files[$"{src}/{name}.Application/{name}.Application.csproj"] = appCsproj;

        if (config.Mapping == MappingOption.Mapster)
        {
            var mappingNs = MappingGenerator.GetNamespaceSuffix(ArchitecturePattern.CleanArchitecture);
            files[$"{src}/{name}.Application/Mapping/MappingConfig.cs"] =
                MappingGenerator.GenerateMappingConfig(config, mappingNs);
        }

        if (config.IncludeFluentValidation && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        {
            files[$"{src}/{name}.Application/Validation/HelloRequestValidator.cs"] =
                GenerateHelloRequestValidator(config, $"{config.Namespace}.Application.Validation");
        }

        // Infrastructure project — references Application and Domain, owns EF Core / Dapper
        var infraCsproj = CsprojGenerator.GenerateClassLibrary(config, "Infrastructure");
        infraCsproj = InjectProjectReferences(infraCsproj, [
            $"../{name}.Application/{name}.Application.csproj",
            $"../{name}.Domain/{name}.Domain.csproj",
        ]);
        files[$"{src}/{name}.Infrastructure/{name}.Infrastructure.csproj"] = infraCsproj;

        if (config.Orm == OrmOption.EfCore)
        {
            var dbContextNs = EfCoreGenerator.GetDbContextNamespaceSuffix(ArchitecturePattern.CleanArchitecture);
            files[$"{src}/{name}.Infrastructure/Data/AppDbContext.cs"] = config.Auth == AuthOption.AspNetIdentity
                ? EfCoreGenerator.GenerateIdentityDbContext(config, dbContextNs)
                : EfCoreGenerator.GenerateDbContext(config, dbContextNs);
        }

        // Entry point project — references Infrastructure and Application
        var entryPointCsproj = config.ProjectType switch
        {
            ProjectType.Console => CsprojGenerator.GenerateConsoleProject(config),
            ProjectType.WorkerService => CsprojGenerator.GenerateWorkerProject(config),
            _ => CsprojGenerator.GenerateWebProject(config),
        };
        entryPointCsproj = InjectProjectReferences(entryPointCsproj, [
            $"../{name}.Infrastructure/{name}.Infrastructure.csproj",
            $"../{name}.Application/{name}.Application.csproj",
        ]);
        files[$"{src}/{name}.{config.EntryPointSuffix}/{name}.{config.EntryPointSuffix}.csproj"] = entryPointCsproj;
        files[$"{src}/{name}.{config.EntryPointSuffix}/Program.cs"] = ProgramCsGenerator.Generate(config);
        files[$"{src}/{name}.{config.EntryPointSuffix}/appsettings.json"] = AppSettingsGenerator.GenerateAppSettings(config);
        files[$"{src}/{name}.{config.EntryPointSuffix}/appsettings.Development.json"] = AppSettingsGenerator.GenerateAppSettingsDevelopment(config);

        if (config.ProjectType == ProjectType.Console)
            files[$"{src}/{name}.{config.EntryPointSuffix}/ServiceCollectionExtensions.cs"] =
                ProgramCsGenerator.GenerateServiceCollectionExtensions(config);

        if (config.Auth == AuthOption.Jwt)
        {
            var authNs = AuthGenerator.GetNamespaceSuffix(ArchitecturePattern.CleanArchitecture, config.EntryPointSuffix);
            files[$"{src}/{name}.{config.EntryPointSuffix}/Auth/JwtSettings.cs"] =
                AuthGenerator.GenerateJwtSettings(config, authNs);
        }
        else if (config.Auth == AuthOption.ApiKey)
        {
            var authNs = AuthGenerator.GetNamespaceSuffix(ArchitecturePattern.CleanArchitecture, config.EntryPointSuffix);
            files[$"{src}/{name}.{config.EntryPointSuffix}/Auth/ApiKeyAuthenticationHandler.cs"] =
                AuthGenerator.GenerateApiKeyAuthHandler(config, authNs);
        }

        if (config.IncludeOpenTelemetry)
        {
            var telemetryNs = ObservabilityGenerator.GetNamespaceSuffix(ArchitecturePattern.CleanArchitecture, config.EntryPointSuffix);
            files[$"{src}/{name}.{config.EntryPointSuffix}/Telemetry/OpenTelemetryExtensions.cs"] =
                ObservabilityGenerator.GenerateOpenTelemetryExtensions(config, telemetryNs);
        }

        if (config.ProjectType == ProjectType.WebApi)
            files[$"{src}/{name}.{config.EntryPointSuffix}/Controllers/HelloController.cs"] =
                GenerateHelloController(config, $"{config.Namespace}.Api.Controllers");
        else if (config.ProjectType == ProjectType.MinimalApi)
            files[$"{src}/{name}.{config.EntryPointSuffix}/Endpoints/HelloEndpoint.cs"] =
                GenerateHelloEndpoint(config, $"{config.Namespace}.Api.Endpoints");
    }

    public static void GenerateVerticalSlice(ProjectConfiguration config, Dictionary<string, string> files, string root)
    {
        var name = config.ProjectName;
        var src = $"{root}/src";
        var proj = $"{src}/{name}";

        files[$"{proj}/{name}.csproj"] = config.ProjectType switch
        {
            ProjectType.Console => CsprojGenerator.GenerateConsoleProject(config),
            ProjectType.WorkerService => CsprojGenerator.GenerateWorkerProject(config),
            _ => CsprojGenerator.GenerateWebProject(config),
        };
        files[$"{proj}/Program.cs"] = ProgramCsGenerator.Generate(config);
        files[$"{proj}/appsettings.json"] = AppSettingsGenerator.GenerateAppSettings(config);
        files[$"{proj}/appsettings.Development.json"] = AppSettingsGenerator.GenerateAppSettingsDevelopment(config);

        if (config.ProjectType == ProjectType.Console)
            files[$"{proj}/ServiceCollectionExtensions.cs"] =
                ProgramCsGenerator.GenerateServiceCollectionExtensions(config);

        if (config.Orm == OrmOption.EfCore)
        {
            var dbContextNs = EfCoreGenerator.GetDbContextNamespaceSuffix(ArchitecturePattern.VerticalSlice);
            files[$"{proj}/Data/AppDbContext.cs"] = config.Auth == AuthOption.AspNetIdentity
                ? EfCoreGenerator.GenerateIdentityDbContext(config, dbContextNs)
                : EfCoreGenerator.GenerateDbContext(config, dbContextNs);

            var entityNs = EfCoreGenerator.GetEntityNamespaceSuffix(ArchitecturePattern.VerticalSlice);
            files[$"{proj}/Features/Hello/HelloEntity.cs"] =
                EfCoreGenerator.GenerateHelloEntity(config, entityNs);
        }

        if (config.Auth == AuthOption.Jwt)
        {
            var authNs = AuthGenerator.GetNamespaceSuffix(ArchitecturePattern.VerticalSlice);
            files[$"{proj}/Auth/JwtSettings.cs"] =
                AuthGenerator.GenerateJwtSettings(config, authNs);
        }
        else if (config.Auth == AuthOption.ApiKey)
        {
            var authNs = AuthGenerator.GetNamespaceSuffix(ArchitecturePattern.VerticalSlice);
            files[$"{proj}/Auth/ApiKeyAuthenticationHandler.cs"] =
                AuthGenerator.GenerateApiKeyAuthHandler(config, authNs);
        }

        if (config.IncludeOpenTelemetry)
        {
            var telemetryNs = ObservabilityGenerator.GetNamespaceSuffix(ArchitecturePattern.VerticalSlice);
            files[$"{proj}/Telemetry/OpenTelemetryExtensions.cs"] =
                ObservabilityGenerator.GenerateOpenTelemetryExtensions(config, telemetryNs);
        }

        if (config.Mapping == MappingOption.Mapster)
        {
            var mappingNs = MappingGenerator.GetNamespaceSuffix(ArchitecturePattern.VerticalSlice);
            files[$"{proj}/Features/Hello/HelloMappingConfig.cs"] =
                MappingGenerator.GenerateHelloMappingConfig(config, mappingNs);
        }

        if (config.IncludeFluentValidation && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        {
            files[$"{proj}/Features/Hello/HelloRequestValidator.cs"] =
                GenerateHelloRequestValidator(config, $"{config.Namespace}.Features.Hello");
        }

        if (config.ProjectType == ProjectType.WebApi)
            files[$"{proj}/Features/Hello/HelloController.cs"] =
                GenerateHelloController(config, $"{config.Namespace}.Features.Hello");
        else if (config.ProjectType == ProjectType.MinimalApi)
            files[$"{proj}/Features/Hello/HelloEndpoint.cs"] =
                GenerateHelloEndpoint(config, $"{config.Namespace}.Features.Hello");
    }

    public static void GenerateSimpleLayered(ProjectConfiguration config, Dictionary<string, string> files, string root)
    {
        var name = config.ProjectName;
        var src = $"{root}/src";
        var proj = $"{src}/{name}";

        files[$"{proj}/{name}.csproj"] = config.ProjectType switch
        {
            ProjectType.Console => CsprojGenerator.GenerateConsoleProject(config),
            ProjectType.WorkerService => CsprojGenerator.GenerateWorkerProject(config),
            _ => CsprojGenerator.GenerateWebProject(config),
        };
        files[$"{proj}/Program.cs"] = ProgramCsGenerator.Generate(config);
        files[$"{proj}/appsettings.json"] = AppSettingsGenerator.GenerateAppSettings(config);
        files[$"{proj}/appsettings.Development.json"] = AppSettingsGenerator.GenerateAppSettingsDevelopment(config);

        if (config.ProjectType == ProjectType.Console)
            files[$"{proj}/ServiceCollectionExtensions.cs"] =
                ProgramCsGenerator.GenerateServiceCollectionExtensions(config);

        if (config.Orm == OrmOption.EfCore)
        {
            var dbContextNs = EfCoreGenerator.GetDbContextNamespaceSuffix(ArchitecturePattern.SimpleLayered);
            files[$"{proj}/Data/AppDbContext.cs"] = config.Auth == AuthOption.AspNetIdentity
                ? EfCoreGenerator.GenerateIdentityDbContext(config, dbContextNs)
                : EfCoreGenerator.GenerateDbContext(config, dbContextNs);

            var entityNs = EfCoreGenerator.GetEntityNamespaceSuffix(ArchitecturePattern.SimpleLayered);
            files[$"{proj}/Data/Entities/SampleEntity.cs"] =
                EfCoreGenerator.GenerateSampleEntity(config, entityNs);
        }

        if (config.Auth == AuthOption.Jwt)
        {
            var authNs = AuthGenerator.GetNamespaceSuffix(ArchitecturePattern.SimpleLayered);
            files[$"{proj}/Auth/JwtSettings.cs"] =
                AuthGenerator.GenerateJwtSettings(config, authNs);
        }
        else if (config.Auth == AuthOption.ApiKey)
        {
            var authNs = AuthGenerator.GetNamespaceSuffix(ArchitecturePattern.SimpleLayered);
            files[$"{proj}/Auth/ApiKeyAuthenticationHandler.cs"] =
                AuthGenerator.GenerateApiKeyAuthHandler(config, authNs);
        }

        if (config.IncludeOpenTelemetry)
        {
            var telemetryNs = ObservabilityGenerator.GetNamespaceSuffix(ArchitecturePattern.SimpleLayered);
            files[$"{proj}/Telemetry/OpenTelemetryExtensions.cs"] =
                ObservabilityGenerator.GenerateOpenTelemetryExtensions(config, telemetryNs);
        }

        if (config.Mapping == MappingOption.Mapster)
        {
            var mappingNs = MappingGenerator.GetNamespaceSuffix(ArchitecturePattern.SimpleLayered);
            files[$"{proj}/Mapping/MappingConfig.cs"] =
                MappingGenerator.GenerateMappingConfig(config, mappingNs);
        }

        if (config.IncludeFluentValidation && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        {
            files[$"{proj}/Validation/HelloRequestValidator.cs"] =
                GenerateHelloRequestValidator(config, $"{config.Namespace}.Validation");
        }

        // Layer folders
        files[$"{proj}/Services/.gitkeep"] = string.Empty;

        if (config.ProjectType == ProjectType.WebApi)
            files[$"{proj}/Controllers/HelloController.cs"] =
                GenerateHelloController(config, $"{config.Namespace}.Controllers");
        else if (config.ProjectType == ProjectType.MinimalApi)
            files[$"{proj}/Endpoints/HelloEndpoint.cs"] =
                GenerateHelloEndpoint(config, $"{config.Namespace}.Endpoints");
    }

    public static void GenerateConsoleOrWorker(ProjectConfiguration config, Dictionary<string, string> files, string root)
    {
        var name = config.ProjectName;
        var src = $"{root}/src";
        var proj = $"{src}/{name}";

        if (config.ProjectType == ProjectType.Console)
        {
            files[$"{proj}/{name}.csproj"] = CsprojGenerator.GenerateConsoleProject(config);
        }
        else
        {
            files[$"{proj}/{name}.csproj"] = CsprojGenerator.GenerateWorkerProject(config);
        }

        files[$"{proj}/Program.cs"] = ProgramCsGenerator.Generate(config);
    }

    private static string GenerateHelloController(ProjectConfiguration config, string ns) =>
        $"using Microsoft.AspNetCore.Mvc;\n" +
        $"\n" +
        $"namespace {ns};\n" +
        $"\n" +
        $"[ApiController]\n" +
        $"[Route(\"api/[controller]\")]\n" +
        $"public class HelloController : ControllerBase\n" +
        $"{{\n" +
        $"    [HttpGet]\n" +
        $"    public IActionResult Get() => Ok(new {{ Message = \"Hello from {config.ProjectName}!\" }});\n" +
        $"}}\n";

    private static string GenerateHelloEndpoint(ProjectConfiguration config, string ns) =>
        $"namespace {ns};\n" +
        $"\n" +
        $"public static class HelloEndpoint\n" +
        $"{{\n" +
        $"    public static void Map(WebApplication app)\n" +
        $"    {{\n" +
        $"        app.MapGet(\"/api/hello\", () => new {{ Message = \"Hello from {config.ProjectName}!\" }});\n" +
        $"    }}\n" +
        $"}}\n";

    private static string GenerateHelloRequestValidator(ProjectConfiguration config, string ns) =>
        $"using FluentValidation;\n" +
        $"\n" +
        $"namespace {ns};\n" +
        $"\n" +
        $"public record HelloRequest(string Name);\n" +
        $"\n" +
        $"public class HelloRequestValidator : AbstractValidator<HelloRequest>\n" +
        $"{{\n" +
        $"    public HelloRequestValidator()\n" +
        $"    {{\n" +
        $"        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);\n" +
        $"    }}\n" +
        $"}}\n";

    /// <summary>
    /// Injects ProjectReference ItemGroups into an existing .csproj string before the closing Project tag.
    /// </summary>
    private static string InjectProjectReferences(string csproj, IEnumerable<string> projectPaths)
    {
        var refs = string.Join("\n", projectPaths.Select(p =>
            $"    <ProjectReference Include=\"{p}\" />"));

        var itemGroup = $"\n  <ItemGroup>\n{refs}\n  </ItemGroup>\n";

        // Insert before </Project>
        var closeTag = "</Project>";
        var insertIndex = csproj.LastIndexOf(closeTag, StringComparison.Ordinal);
        if (insertIndex < 0) return csproj + itemGroup;

        return csproj[..insertIndex] + itemGroup + closeTag;
    }
}
