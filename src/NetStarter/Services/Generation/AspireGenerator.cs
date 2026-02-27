using System.Text;
using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class AspireGenerator
{
    public static string GenerateAppHostProgram(ProjectConfiguration config)
    {
        var name = config.ProjectName;
        var nameLower = name.ToLowerInvariant();
        var isCleanArch = config.Architecture == ArchitecturePattern.CleanArchitecture;
        var hasPostgres = config.Orm == OrmOption.EfCore && config.Database == DatabaseOption.PostgreSql;
        var hasSqlServer = config.Orm == OrmOption.EfCore && config.Database == DatabaseOption.SqlServer;
        var hasDb = hasPostgres || hasSqlServer;

        var sb = new StringBuilder();
        sb.AppendLine("var builder = DistributedApplication.CreateBuilder(args);");
        sb.AppendLine();

        if (hasPostgres)
        {
            sb.AppendLine($"var postgres = builder.AddPostgres(\"postgres\").AddDatabase(\"{nameLower}\");");
        }
        else if (hasSqlServer)
        {
            sb.AppendLine($"var sqlserver = builder.AddSqlServer(\"sqlserver\").AddDatabase(\"{nameLower}\");");
        }

        if (hasDb)
        {
            sb.AppendLine();
        }

        var projectRef = isCleanArch
            ? $"Projects.{name}_Api"
            : $"Projects.{name}";

        if (hasPostgres)
        {
            sb.AppendLine($"var api = builder.AddProject<{projectRef}>(\"{nameLower}-api\")");
            sb.AppendLine("    .WithReference(postgres);");
        }
        else if (hasSqlServer)
        {
            sb.AppendLine($"var api = builder.AddProject<{projectRef}>(\"{nameLower}-api\")");
            sb.AppendLine("    .WithReference(sqlserver);");
        }
        else
        {
            sb.AppendLine($"var api = builder.AddProject<{projectRef}>(\"{nameLower}-api\");");
        }

        sb.AppendLine();
        sb.AppendLine("builder.Build().Run();");

        return sb.ToString();
    }

    public static string GenerateServiceDefaultsExtensions(ProjectConfiguration config)
    {
        var ns = $"{config.Namespace}.ServiceDefaults";

        return
            $"using Microsoft.AspNetCore.Builder;\n" +
            $"using Microsoft.AspNetCore.Diagnostics.HealthChecks;\n" +
            $"using Microsoft.Extensions.DependencyInjection;\n" +
            $"using Microsoft.Extensions.Diagnostics.HealthChecks;\n" +
            $"using Microsoft.Extensions.Hosting;\n" +
            $"using Microsoft.Extensions.Logging;\n" +
            $"using OpenTelemetry;\n" +
            $"using OpenTelemetry.Metrics;\n" +
            $"using OpenTelemetry.Trace;\n" +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public static class Extensions\n" +
            $"{{\n" +
            $"    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)\n" +
            $"    {{\n" +
            $"        builder.ConfigureOpenTelemetry();\n" +
            $"        builder.AddDefaultHealthChecks();\n" +
            $"        builder.Services.AddServiceDiscovery();\n" +
            $"        builder.Services.ConfigureHttpClientDefaults(http =>\n" +
            $"        {{\n" +
            $"            http.AddStandardResilienceHandler();\n" +
            $"            http.AddServiceDiscovery();\n" +
            $"        }});\n" +
            $"        return builder;\n" +
            $"    }}\n" +
            $"\n" +
            $"    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)\n" +
            $"    {{\n" +
            $"        builder.Logging.AddOpenTelemetry(logging =>\n" +
            $"        {{\n" +
            $"            logging.IncludeFormattedMessage = true;\n" +
            $"            logging.IncludeScopes = true;\n" +
            $"        }});\n" +
            $"\n" +
            $"        builder.Services.AddOpenTelemetry()\n" +
            $"            .WithMetrics(metrics =>\n" +
            $"            {{\n" +
            $"                metrics.AddAspNetCoreInstrumentation()\n" +
            $"                    .AddHttpClientInstrumentation()\n" +
            $"                    .AddRuntimeInstrumentation();\n" +
            $"            }})\n" +
            $"            .WithTracing(tracing =>\n" +
            $"            {{\n" +
            $"                tracing.AddAspNetCoreInstrumentation()\n" +
            $"                    .AddHttpClientInstrumentation();\n" +
            $"            }});\n" +
            $"\n" +
            $"        builder.AddOpenTelemetryExporters();\n" +
            $"        return builder;\n" +
            $"    }}\n" +
            $"\n" +
            $"    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)\n" +
            $"    {{\n" +
            $"        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration[\"OTEL_EXPORTER_OTLP_ENDPOINT\"]);\n" +
            $"        if (useOtlpExporter)\n" +
            $"        {{\n" +
            $"            builder.Services.AddOpenTelemetry().UseOtlpExporter();\n" +
            $"        }}\n" +
            $"        return builder;\n" +
            $"    }}\n" +
            $"\n" +
            $"    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)\n" +
            $"    {{\n" +
            $"        builder.Services.AddHealthChecks()\n" +
            $"            .AddCheck(\"self\", () => HealthCheckResult.Healthy(), [\"live\"]);\n" +
            $"        return builder;\n" +
            $"    }}\n" +
            $"\n" +
            $"    public static WebApplication MapDefaultEndpoints(this WebApplication app)\n" +
            $"    {{\n" +
            $"        app.MapHealthChecks(\"/health\");\n" +
            $"        app.MapHealthChecks(\"/alive\", new HealthCheckOptions\n" +
            $"        {{\n" +
            $"            Predicate = r => r.Tags.Contains(\"live\")\n" +
            $"        }});\n" +
            $"        return app;\n" +
            $"    }}\n" +
            $"}}\n";
    }
}
