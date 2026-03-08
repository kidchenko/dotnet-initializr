using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class ObservabilityGenerator
{
    public static string GenerateOpenTelemetryExtensions(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        var efCoreInstrumentation = config.Orm == OrmOption.EfCore
            ? "\n                       .AddEntityFrameworkCoreInstrumentation()"
            : string.Empty;

        var efCoreUsing = config.Orm == OrmOption.EfCore
            ? $"using OpenTelemetry.Instrumentation.EntityFrameworkCore;\n"
            : string.Empty;

        return
            $"namespace {ns};\n" +
            $"\n" +
            $"using Microsoft.Extensions.Configuration;\n" +
            $"using Microsoft.Extensions.DependencyInjection;\n" +
            $"using OpenTelemetry.Resources;\n" +
            $"using OpenTelemetry.Trace;\n" +
            $"using OpenTelemetry.Metrics;\n" +
            efCoreUsing +
            $"\n" +
            $"public static class OpenTelemetryExtensions\n" +
            $"{{\n" +
            $"    public static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services, IConfiguration configuration)\n" +
            $"    {{\n" +
            $"        services.AddOpenTelemetry()\n" +
            $"            .ConfigureResource(resource => resource.AddService(\"{config.ProjectName}\"))\n" +
            $"            .WithTracing(tracing =>\n" +
            $"            {{\n" +
            $"                tracing.AddAspNetCoreInstrumentation()\n" +
            $"                       .AddHttpClientInstrumentation(){efCoreInstrumentation};\n" +
            $"                tracing.AddOtlpExporter();\n" +
            $"            }})\n" +
            $"            .WithMetrics(metrics =>\n" +
            $"            {{\n" +
            $"                metrics.AddAspNetCoreInstrumentation()\n" +
            $"                       .AddOtlpExporter();\n" +
            $"            }});\n" +
            $"        return services;\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GetNamespaceSuffix(ArchitecturePattern architecture) =>
        GetNamespaceSuffix(architecture, "Api");

    public static string GetNamespaceSuffix(ArchitecturePattern architecture, string entryPointSuffix) => architecture switch
    {
        ArchitecturePattern.CleanArchitecture => "Infrastructure.Telemetry",
        _ => "Telemetry",
    };
}
