using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class ProgramCsGenerator
{
    public static string Generate(ProjectConfiguration config) => config.ProjectType switch
    {
        ProjectType.Console => GenerateConsole(config),
        ProjectType.WorkerService => GenerateWorkerService(config),
        _ => GenerateWebApplication(config),
    };

    private static string GenerateConsole(ProjectConfiguration config)
    {
        var entryPointNs = config.Architecture == ArchitecturePattern.CleanArchitecture
            ? $"{config.Namespace}.{config.EntryPointSuffix}"
            : config.Namespace;

        return
            $"using Microsoft.Extensions.Configuration;\n" +
            $"using Microsoft.Extensions.DependencyInjection;\n" +
            $"using {entryPointNs};\n" +
            $"\n" +
            $"var configuration = new ConfigurationBuilder()\n" +
            $"    .AddJsonFile(\"appsettings.json\")\n" +
            $"    .AddJsonFile(\"appsettings.Development.json\", optional: true)\n" +
            $"    .Build();\n" +
            $"\n" +
            $"var services = new ServiceCollection();\n" +
            $"services.AddAppServices(configuration);\n" +
            $"\n" +
            $"var serviceProvider = services.BuildServiceProvider();\n" +
            $"\n" +
            $"Console.WriteLine(\"Hello from {config.ProjectName}!\");\n";
    }

    public static string GenerateServiceCollectionExtensions(ProjectConfiguration config)
    {
        var ns = config.Architecture == ArchitecturePattern.CleanArchitecture
            ? $"{config.Namespace}.{config.EntryPointSuffix}"
            : config.Namespace;

        var usings = $"using Microsoft.Extensions.Configuration;\n" +
                     $"using Microsoft.Extensions.DependencyInjection;\n";
        var body = string.Empty;

        if (config.Orm == OrmOption.EfCore)
        {
            var dbContextNs = EfCoreGenerator.GetDbContextNamespaceSuffix(config.Architecture);
            usings += $"using Microsoft.EntityFrameworkCore;\n";
            usings += $"using {config.Namespace}.{dbContextNs};\n";

            var dbMethod = config.Database switch
            {
                DatabaseOption.PostgreSql => "UseNpgsql",
                DatabaseOption.SqlServer => "UseSqlServer",
                _ => "UseSqlite",
            };
            body += $"        services.AddDbContext<AppDbContext>(options =>\n" +
                    $"            options.{dbMethod}(configuration.GetConnectionString(\"DefaultConnection\")));\n";
        }

        if (config.IncludeSerilog)
        {
            usings += $"using Serilog;\n";
            body += $"        services.AddLogging(builder =>\n" +
                    $"            builder.AddSerilog(new LoggerConfiguration()\n" +
                    $"                .ReadFrom.Configuration(configuration)\n" +
                    $"                .CreateLogger()));\n";
        }

        if (config.Mapping == MappingOption.Mapster)
        {
            usings += $"using Mapster;\n";
            usings += $"using MapsterMapper;\n";
            body += $"        services.AddSingleton(TypeAdapterConfig.GlobalSettings);\n" +
                    $"        services.AddScoped<IMapper, ServiceMapper>();\n";
        }

        return
            usings +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public static class ServiceCollectionExtensions\n" +
            $"{{\n" +
            $"    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)\n" +
            $"    {{\n" +
            body +
            $"        return services;\n" +
            $"    }}\n" +
            $"}}\n";
    }

    private static string GenerateWorkerService(ProjectConfiguration config) =>
        $"using {config.Namespace};\n" +
        $"\n" +
        $"var builder = Host.CreateApplicationBuilder(args);\n" +
        $"builder.Services.AddHostedService<Worker>();\n" +
        $"\n" +
        $"var host = builder.Build();\n" +
        $"host.Run();\n";

    private static string GenerateWebApplication(ProjectConfiguration config)
    {
        var usings = BuildUsings(config);
        var services = BuildServiceRegistrations(config);
        var middleware = BuildMiddleware(config);

        return usings +
               $"var builder = WebApplication.CreateBuilder(args);\n" +
               services +
               $"\n" +
               $"var app = builder.Build();\n" +
               $"\n" +
               middleware +
               $"app.Run();\n";
    }

    private static string BuildUsings(ProjectConfiguration config)
    {
        var usings = string.Empty;

        if (config.Orm == OrmOption.EfCore)
        {
            var dbContextNs = EfCoreGenerator.GetDbContextNamespaceSuffix(config.Architecture);
            usings += $"using Microsoft.EntityFrameworkCore;\n";
            usings += $"using {config.Namespace}.{dbContextNs};\n";
        }

        if (config.Auth == AuthOption.Jwt)
        {
            usings += $"using Microsoft.AspNetCore.Authentication.JwtBearer;\n";
            usings += $"using Microsoft.IdentityModel.Tokens;\n";
            usings += $"using System.Text;\n";
        }

        if (config.IncludeSerilog)
            usings += $"using Serilog;\n";

        if (config.IncludeOpenTelemetry)
        {
            var telemetryNs = ObservabilityGenerator.GetNamespaceSuffix(config.Architecture);
            usings += $"using {config.Namespace}.{telemetryNs};\n";
        }

        if (config.Mapping == MappingOption.Mapster)
        {
            usings += $"using Mapster;\n";
            usings += $"using MapsterMapper;\n";
        }

        return string.IsNullOrEmpty(usings) ? string.Empty : usings + "\n";
    }

    private static string BuildServiceRegistrations(ProjectConfiguration config)
    {
        var services = string.Empty;

        if (config.IncludeSerilog)
            services += $"builder.Host.UseSerilog((context, cfg) =>\n" +
                        $"    cfg.ReadFrom.Configuration(context.Configuration));\n" +
                        $"\n";

        if (config.Orm == OrmOption.EfCore)
        {
            var dbMethod = config.Database switch
            {
                DatabaseOption.PostgreSql => "UseNpgsql",
                DatabaseOption.SqlServer => "UseSqlServer",
                _ => "UseSqlite",
            };
            var dbContextNs = EfCoreGenerator.GetDbContextNamespaceSuffix(config.Architecture);
            services += $"builder.Services.AddDbContext<AppDbContext>(options =>\n" +
                        $"    options.{dbMethod}(builder.Configuration.GetConnectionString(\"DefaultConnection\")));\n";
        }

        if (config.Auth == AuthOption.Jwt)
        {
            services +=
                $"builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)\n" +
                $"    .AddJwtBearer(options =>\n" +
                $"    {{\n" +
                $"        options.TokenValidationParameters = new TokenValidationParameters\n" +
                $"        {{\n" +
                $"            ValidateIssuer = true,\n" +
                $"            ValidateAudience = true,\n" +
                $"            ValidateLifetime = true,\n" +
                $"            ValidateIssuerSigningKey = true,\n" +
                $"            ValidIssuer = builder.Configuration[\"Jwt:Issuer\"],\n" +
                $"            ValidAudience = builder.Configuration[\"Jwt:Audience\"],\n" +
                $"            IssuerSigningKey = new SymmetricSecurityKey(\n" +
                $"                Encoding.UTF8.GetBytes(builder.Configuration[\"Jwt:Key\"]!))\n" +
                $"        }};\n" +
                $"    }});\n";
        }

        if (config.IncludeHealthChecks)
            services += $"builder.Services.AddHealthChecks();\n";

        if (config.IncludeOpenTelemetry)
            services += $"builder.Services.AddAppOpenTelemetry(builder.Configuration);\n";

        if (config.Mapping == MappingOption.Mapster)
        {
            services += $"builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);\n";
            services += $"builder.Services.AddScoped<IMapper, ServiceMapper>();\n";
        }

        if (config.ProjectType == ProjectType.WebApi)
            services += $"builder.Services.AddControllers();\n";
        else if (config.ProjectType == ProjectType.MinimalApi)
            services += $"builder.Services.AddEndpointsApiExplorer();\n";

        return string.IsNullOrEmpty(services) ? string.Empty : "\n" + services;
    }

    private static string BuildMiddleware(ProjectConfiguration config)
    {
        var middleware = string.Empty;

        if (config.Auth == AuthOption.Jwt)
        {
            middleware += $"app.UseAuthentication();\n";
            middleware += $"app.UseAuthorization();\n";
        }

        if (config.IncludeHealthChecks)
            middleware += $"app.MapHealthChecks(\"/health\");\n";

        if (config.ProjectType == ProjectType.WebApi)
            middleware += $"app.MapControllers();\n";
        else if (config.ProjectType == ProjectType.MinimalApi)
            middleware += $"app.MapGet(\"/api/hello\", () => new {{ Message = \"Hello from {config.ProjectName}!\" }});\n";

        return string.IsNullOrEmpty(middleware) ? string.Empty : middleware + "\n";
    }
}
