using System.Text;
using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class InfrastructureExtensionsGenerator
{
    public static string GenerateInfrastructureExtensions(ProjectConfiguration config)
    {
        var ns = $"{config.Namespace}.Infrastructure";
        var sb = new StringBuilder();

        // Usings
        sb.Append($"using Microsoft.Extensions.Configuration;\n");
        sb.Append($"using Microsoft.Extensions.DependencyInjection;\n");

        if (config.Orm == OrmOption.EfCore)
        {
            var dbContextNs = EfCoreGenerator.GetDbContextNamespaceSuffix(ArchitecturePattern.CleanArchitecture);
            sb.Append($"using Microsoft.EntityFrameworkCore;\n");
            sb.Append($"using {config.Namespace}.{dbContextNs};\n");
        }

        if (config.Orm == OrmOption.Dapper)
        {
            var dapperNs = DapperGenerator.GetNamespace(config);
            sb.Append($"using {dapperNs};\n");
        }

        if (config.Auth == AuthOption.AspNetIdentity)
        {
            sb.Append($"using Microsoft.AspNetCore.Identity;\n");
        }

        if (config.IncludeOpenTelemetry)
        {
            var telemetryNs = ObservabilityGenerator.GetNamespaceSuffix(ArchitecturePattern.CleanArchitecture, config.EntryPointSuffix);
            sb.Append($"using {config.Namespace}.{telemetryNs};\n");
        }

        if (config.BackgroundJobs == BackgroundJobsOption.Hangfire && config.ProjectType != ProjectType.Console)
        {
            sb.Append("using Hangfire;\n");
            var storageUsing = config.Database switch
            {
                DatabaseOption.PostgreSql => "using Hangfire.PostgreSql;\n",
                DatabaseOption.MySql      => "using Hangfire.MySql;\n",
                _                         => null,
            };
            if (storageUsing is not null)
                sb.Append(storageUsing);
        }

        if (config.BackgroundJobs == BackgroundJobsOption.Quartz && config.ProjectType != ProjectType.Console)
        {
            sb.Append("using Quartz;\n");
            var jobsNs = BackgroundJobsGenerator.GetJobsNamespace(config);
            sb.Append($"using {jobsNs};\n");
        }

        if (config.BackgroundJobs == BackgroundJobsOption.IHostedService && config.ProjectType != ProjectType.Console)
        {
            var jobsNs = BackgroundJobsGenerator.GetJobsNamespace(config);
            sb.Append($"using {jobsNs};\n");
        }

        sb.Append($"\n");
        sb.Append($"namespace {ns};\n");
        sb.Append($"\n");
        sb.Append($"public static class InfrastructureExtensions\n");
        sb.Append($"{{\n");
        sb.Append($"    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)\n");
        sb.Append($"    {{\n");

        // EF Core
        if (config.Orm == OrmOption.EfCore)
        {
            if (config.Database == DatabaseOption.MySql)
            {
                sb.Append("        services.AddDbContext<AppDbContext>(options =>\n");
                sb.Append("            options.UseMySql(\n");
                sb.Append("                configuration.GetConnectionString(\"DefaultConnection\"),\n");
                sb.Append("                ServerVersion.AutoDetect(\n");
                sb.Append("                    configuration.GetConnectionString(\"DefaultConnection\"))));\n");
            }
            else
            {
                var dbMethod = config.Database switch
                {
                    DatabaseOption.PostgreSql => "UseNpgsql",
                    DatabaseOption.SqlServer => "UseSqlServer",
                    _ => "UseSqlite",
                };
                sb.Append($"        services.AddDbContext<AppDbContext>(options =>\n");
                sb.Append($"            options.{dbMethod}(configuration.GetConnectionString(\"DefaultConnection\")));\n");
            }
        }

        // Dapper
        if (config.Orm == OrmOption.Dapper)
        {
            sb.Append("        services.AddDapperConnection(configuration);\n");
        }

        // ASP.NET Identity
        if (config.Auth == AuthOption.AspNetIdentity)
        {
            sb.Append("        services.AddIdentity<IdentityUser, IdentityRole>()\n");
            sb.Append("            .AddEntityFrameworkStores<AppDbContext>()\n");
            sb.Append("            .AddDefaultTokenProviders();\n");
        }

        // OpenTelemetry
        if (config.IncludeOpenTelemetry)
        {
            sb.Append("        services.AddAppOpenTelemetry(configuration);\n");
        }

        // Redis
        if (config.IncludeRedis)
        {
            sb.Append("        services.AddStackExchangeRedisCache(options =>\n");
            sb.Append("        {\n");
            sb.Append("            options.Configuration = configuration.GetConnectionString(\"Redis\");\n");
            sb.Append($"            options.InstanceName = \"{config.ProjectName}:\";\n");
            sb.Append("        });\n");
        }

        // Resilience (HttpClient)
        if (config.IncludeResilience && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        {
            sb.Append($"        services.AddHttpClient(\"{config.ProjectName}Client\")\n");
            sb.Append("            .AddStandardResilienceHandler();\n");
        }

        // Background Jobs
        if (config.BackgroundJobs != BackgroundJobsOption.None && config.ProjectType != ProjectType.Console)
        {
            switch (config.BackgroundJobs)
            {
                case BackgroundJobsOption.IHostedService:
                    sb.Append("        services.AddHostedService<SampleBackgroundService>();\n");
                    break;
                case BackgroundJobsOption.Hangfire when config.Database.HasValue:
                    sb.Append("        services.AddHangfire(config => config\n");
                    sb.Append("            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)\n");
                    sb.Append("            .UseSimpleAssemblyNameTypeSerializer()\n");
                    sb.Append("            .UseRecommendedSerializerSettings()\n");
                    var storageLine = config.Database switch
                    {
                        DatabaseOption.PostgreSql =>
                            "            .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(configuration.GetConnectionString(\"DefaultConnection\")))",
                        DatabaseOption.SqlServer =>
                            "            .UseSqlServerStorage(configuration.GetConnectionString(\"DefaultConnection\"))",
                        DatabaseOption.MySql =>
                            "            .UseStorage(new MySqlStorage(configuration.GetConnectionString(\"DefaultConnection\"), new MySqlStorageOptions()))",
                        _ =>
                            "            .UseMemoryStorage()",
                    };
                    sb.Append(storageLine + ");\n");
                    sb.Append("        services.AddHangfireServer();\n");
                    break;
                case BackgroundJobsOption.Quartz:
                    sb.Append("        services.AddQuartz(q =>\n");
                    sb.Append("        {\n");
                    sb.Append("            q.ScheduleJob<SampleQuartzJob>(trigger => trigger\n");
                    sb.Append("                .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()));\n");
                    sb.Append("        });\n");
                    sb.Append("        services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);\n");
                    break;
            }
        }

        sb.Append($"        return services;\n");
        sb.Append($"    }}\n");
        sb.Append($"}}\n");

        return sb.ToString();
    }

    public static string GenerateApplicationExtensions(ProjectConfiguration config)
    {
        var ns = $"{config.Namespace}.Application";
        var sb = new StringBuilder();

        // Usings
        sb.Append($"using Microsoft.Extensions.DependencyInjection;\n");

        if (config.Mapping == MappingOption.Mapster)
        {
            sb.Append($"using Mapster;\n");
            sb.Append($"using MapsterMapper;\n");
        }

        if (config.IncludeFluentValidation)
        {
            sb.Append($"using FluentValidation;\n");
        }

        sb.Append($"\n");
        sb.Append($"namespace {ns};\n");
        sb.Append($"\n");
        sb.Append($"public static class ApplicationExtensions\n");
        sb.Append($"{{\n");
        sb.Append($"    public static IServiceCollection AddApplication(this IServiceCollection services)\n");
        sb.Append($"    {{\n");

        if (config.Mapping == MappingOption.Mapster)
        {
            sb.Append("        services.AddSingleton(TypeAdapterConfig.GlobalSettings);\n");
            sb.Append("        services.AddScoped<IMapper, ServiceMapper>();\n");
        }

        if (config.IncludeFluentValidation)
        {
            sb.Append("        services.AddValidatorsFromAssembly(typeof(ApplicationExtensions).Assembly);\n");
        }

        sb.Append($"        return services;\n");
        sb.Append($"    }}\n");
        sb.Append($"}}\n");

        return sb.ToString();
    }

    public static bool HasInfrastructureServices(ProjectConfiguration config) =>
        config.Orm != OrmOption.None
        || config.Auth == AuthOption.AspNetIdentity
        || config.IncludeOpenTelemetry
        || config.IncludeRedis
        || (config.IncludeResilience && config.ProjectType is ProjectType.WebApi or ProjectType.MinimalApi)
        || (config.BackgroundJobs != BackgroundJobsOption.None && config.ProjectType != ProjectType.Console);

    public static bool HasApplicationServices(ProjectConfiguration config) =>
        config.Mapping == MappingOption.Mapster
        || config.IncludeFluentValidation;
}
