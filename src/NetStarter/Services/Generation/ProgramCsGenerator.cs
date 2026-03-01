using System.Text;
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

        if (config.Logging == LoggingOption.Serilog)
        {
            usings += $"using Serilog;\n";
            body += $"        services.AddLogging(builder =>\n" +
                    $"            builder.AddSerilog(new LoggerConfiguration()\n" +
                    $"                .ReadFrom.Configuration(configuration)\n" +
                    $"                .CreateLogger()));\n";
        }

        if (config.Logging == LoggingOption.NLog)
        {
            usings += $"using NLog.Extensions.Hosting;\n";
            body += $"        // NLog configured via Host in Program.cs\n";
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

    private static string GenerateWorkerService(ProjectConfiguration config)
    {
        var sb = new StringBuilder();

        if (config.Logging == LoggingOption.NLog)
            sb.Append("using NLog.Extensions.Hosting;\n");

        sb.Append($"using {config.Namespace};\n");
        sb.Append("\n");
        sb.Append("var builder = Host.CreateApplicationBuilder(args);\n");

        if (config.Logging == LoggingOption.NLog)
        {
            sb.Append("builder.Logging.ClearProviders();\n");
            sb.Append("builder.UseNLog();\n");
        }

        sb.Append("builder.Services.AddHostedService<Worker>();\n");
        sb.Append("\n");
        sb.Append("var host = builder.Build();\n");
        sb.Append("host.Run();\n");

        return sb.ToString();
    }

    private static string GenerateWebApplication(ProjectConfiguration config)
    {
        var sb = new StringBuilder();
        AddUsingsFragment(config, sb);
        sb.Append("var builder = WebApplication.CreateBuilder(args);\n");
        sb.Append("\n");
        AddSerilogFragment(config, sb);
        AddNLogFragment(config, sb);
        AddDatabaseFragment(config, sb);
        AddAuthFragment(config, sb);
        AddHealthChecksFragment(config, sb);
        AddOpenTelemetryFragment(config, sb);
        AddMappingFragment(config, sb);
        AddRedisCacheFragment(config, sb);
        AddFluentValidationFragment(config, sb);
        AddControllersFragment(config, sb);
        sb.Append("\n");
        sb.Append("var app = builder.Build();\n");
        sb.Append("\n");
        AddMiddlewareFragment(config, sb);
        sb.Append("app.Run();\n");
        return sb.ToString();
    }

    private static void AddUsingsFragment(ProjectConfiguration config, StringBuilder sb)
    {
        var hasUsings = false;

        if (config.Orm == OrmOption.EfCore)
        {
            var dbContextNs = EfCoreGenerator.GetDbContextNamespaceSuffix(config.Architecture);
            sb.Append($"using Microsoft.EntityFrameworkCore;\n");
            sb.Append($"using {config.Namespace}.{dbContextNs};\n");
            hasUsings = true;
        }

        if (config.Auth == AuthOption.Jwt)
        {
            sb.Append($"using Microsoft.AspNetCore.Authentication.JwtBearer;\n");
            sb.Append($"using Microsoft.IdentityModel.Tokens;\n");
            sb.Append($"using System.Text;\n");
            hasUsings = true;
        }

        if (config.Auth == AuthOption.Keycloak)
        {
            sb.Append($"using Microsoft.AspNetCore.Authentication.JwtBearer;\n");
            hasUsings = true;
        }

        if (config.Auth == AuthOption.ApiKey)
        {
            sb.Append($"using {config.Namespace}.{AuthGenerator.GetNamespaceSuffix(config.Architecture)};\n");
            sb.Append($"using Microsoft.AspNetCore.Authentication;\n");
            hasUsings = true;
        }

        if (config.Auth == AuthOption.AspNetIdentity)
        {
            sb.Append($"using Microsoft.AspNetCore.Identity;\n");
            hasUsings = true;
        }

        if (config.Logging == LoggingOption.Serilog)
        {
            sb.Append($"using Serilog;\n");
            hasUsings = true;
        }

        if (config.Logging == LoggingOption.NLog)
        {
            var isWebProject = config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi;
            sb.Append(isWebProject ? "using NLog.Web;\n" : "using NLog.Extensions.Hosting;\n");
            hasUsings = true;
        }

        if (config.IncludeOpenTelemetry)
        {
            var telemetryNs = ObservabilityGenerator.GetNamespaceSuffix(config.Architecture);
            sb.Append($"using {config.Namespace}.{telemetryNs};\n");
            hasUsings = true;
        }

        if (config.Mapping == MappingOption.Mapster)
        {
            sb.Append($"using Mapster;\n");
            sb.Append($"using MapsterMapper;\n");
            hasUsings = true;
        }

        if (config.IncludeFluentValidation)
        {
            sb.Append($"using FluentValidation;\n");
            hasUsings = true;
        }

        if (hasUsings)
            sb.Append("\n");
    }

    private static void AddSerilogFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (config.Logging != LoggingOption.Serilog) return;
        sb.Append("builder.Host.UseSerilog((context, cfg) =>\n");
        sb.Append("    cfg.ReadFrom.Configuration(context.Configuration));\n");
        sb.Append("\n");
    }

    private static void AddNLogFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (config.Logging != LoggingOption.NLog) return;
        sb.Append("builder.Logging.ClearProviders();\n");
        sb.Append("builder.Host.UseNLog();\n");
        sb.Append("\n");
    }

    private static void AddDatabaseFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (config.Orm == OrmOption.EfCore)
        {
            if (config.Database == DatabaseOption.MySql)
            {
                sb.Append("builder.Services.AddDbContext<AppDbContext>(options =>\n");
                sb.Append("    options.UseMySql(\n");
                sb.Append("        builder.Configuration.GetConnectionString(\"DefaultConnection\"),\n");
                sb.Append("        ServerVersion.AutoDetect(\n");
                sb.Append("            builder.Configuration.GetConnectionString(\"DefaultConnection\"))));\n");
            }
            else
            {
                var dbMethod = config.Database switch
                {
                    DatabaseOption.PostgreSql => "UseNpgsql",
                    DatabaseOption.SqlServer => "UseSqlServer",
                    _ => "UseSqlite",
                };
                sb.Append($"builder.Services.AddDbContext<AppDbContext>(options =>\n");
                sb.Append($"    options.{dbMethod}(builder.Configuration.GetConnectionString(\"DefaultConnection\")));\n");
            }
            return;
        }

        if (config.Orm == OrmOption.Dapper)
        {
            sb.Append("builder.Services.AddDapperConnection(builder.Configuration);\n");
            return;
        }
    }

    private static void AddRedisCacheFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (!config.IncludeRedis) return;
        sb.Append("builder.Services.AddStackExchangeRedisCache(options =>\n");
        sb.Append("{\n");
        sb.Append("    options.Configuration = builder.Configuration.GetConnectionString(\"Redis\");\n");
        sb.Append($"    options.InstanceName = \"{config.ProjectName}:\";\n");
        sb.Append("});\n");
    }

    private static void AddAuthFragment(ProjectConfiguration config, StringBuilder sb)
    {
        switch (config.Auth)
        {
            case AuthOption.Jwt:
                sb.Append("builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)\n");
                sb.Append("    .AddJwtBearer(options =>\n");
                sb.Append("    {\n");
                sb.Append("        options.TokenValidationParameters = new TokenValidationParameters\n");
                sb.Append("        {\n");
                sb.Append("            ValidateIssuer = true,\n");
                sb.Append("            ValidateAudience = true,\n");
                sb.Append("            ValidateLifetime = true,\n");
                sb.Append("            ValidateIssuerSigningKey = true,\n");
                sb.Append("            ValidIssuer = builder.Configuration[\"Jwt:Issuer\"],\n");
                sb.Append("            ValidAudience = builder.Configuration[\"Jwt:Audience\"],\n");
                sb.Append("            IssuerSigningKey = new SymmetricSecurityKey(\n");
                sb.Append("                Encoding.UTF8.GetBytes(builder.Configuration[\"Jwt:Key\"]!))\n");
                sb.Append("        };\n");
                sb.Append("    });\n");
                break;
            case AuthOption.Keycloak:
                sb.Append("builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)\n");
                sb.Append("    .AddJwtBearer(options =>\n");
                sb.Append("    {\n");
                sb.Append("        options.Authority = builder.Configuration[\"Keycloak:Authority\"];\n");
                sb.Append("        options.Audience = builder.Configuration[\"Keycloak:Audience\"];\n");
                sb.Append("        options.RequireHttpsMetadata = false;\n");
                sb.Append("    });\n");
                sb.Append("builder.Services.AddAuthorization();\n");
                break;
            case AuthOption.ApiKey:
                sb.Append("builder.Services.AddAuthentication(\"ApiKey\")\n");
                sb.Append("    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(\"ApiKey\", _ => { });\n");
                sb.Append("builder.Services.AddAuthorization();\n");
                break;
            case AuthOption.AspNetIdentity:
                sb.Append("builder.Services.AddIdentity<IdentityUser, IdentityRole>()\n");
                sb.Append("    .AddEntityFrameworkStores<AppDbContext>()\n");
                sb.Append("    .AddDefaultTokenProviders();\n");
                sb.Append("builder.Services.AddAuthorization();\n");
                break;
        }
    }

    private static void AddHealthChecksFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (!config.IncludeHealthChecks) return;
        sb.Append("builder.Services.AddHealthChecks();\n");
    }

    private static void AddOpenTelemetryFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (!config.IncludeOpenTelemetry) return;
        sb.Append("builder.Services.AddAppOpenTelemetry(builder.Configuration);\n");
    }

    private static void AddMappingFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (config.Mapping != MappingOption.Mapster) return;
        sb.Append("builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);\n");
        sb.Append("builder.Services.AddScoped<IMapper, ServiceMapper>();\n");
    }

    private static void AddFluentValidationFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (!config.IncludeFluentValidation) return;
        sb.Append("builder.Services.AddValidatorsFromAssemblyContaining<Program>();\n");
    }

    private static void AddControllersFragment(ProjectConfiguration config, StringBuilder sb)
    {
        if (config.ProjectType == ProjectType.WebApi)
            sb.Append("builder.Services.AddControllers();\n");
        else if (config.ProjectType == ProjectType.MinimalApi)
            sb.Append("builder.Services.AddEndpointsApiExplorer();\n");
    }

    private static void AddMiddlewareFragment(ProjectConfiguration config, StringBuilder sb)
    {
        var middlewareStart = sb.Length;

        if (config.Auth is AuthOption.Jwt or AuthOption.Keycloak or AuthOption.ApiKey or AuthOption.AspNetIdentity)
        {
            sb.Append("app.UseAuthentication();\n");
            sb.Append("app.UseAuthorization();\n");
        }

        if (config.IncludeHealthChecks)
            sb.Append("app.MapHealthChecks(\"/health\");\n");

        if (config.ProjectType == ProjectType.WebApi)
            sb.Append("app.MapControllers();\n");
        else if (config.ProjectType == ProjectType.MinimalApi)
            sb.Append($"app.MapGet(\"/api/hello\", () => new {{ Message = \"Hello from {config.ProjectName}!\" }});\n");

        if (sb.Length > middlewareStart)
            sb.Append("\n");
    }
}
