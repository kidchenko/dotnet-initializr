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

    private static string GenerateConsole(ProjectConfiguration config) =>
        $"namespace {config.Namespace};\n" +
        $"\n" +
        $"Console.WriteLine(\"Hello from {config.ProjectName}!\");\n";

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
            usings += $"using Microsoft.EntityFrameworkCore;\n";

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
                        $"    cfg.ReadFrom.Configuration(context.Configuration)\n" +
                        $"       .WriteTo.Console()\n" +
                        $"       .WriteTo.File(\"logs/log-.txt\", rollingInterval: RollingInterval.Day));\n" +
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
