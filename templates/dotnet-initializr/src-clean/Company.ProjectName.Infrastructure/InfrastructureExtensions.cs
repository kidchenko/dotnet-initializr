using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
using Company.ProjectName.Infrastructure.Data;
#endif
#if (IncludeDapper)
using System.Data;
#if (IncludePostgreSql)
using Npgsql;
#elif (IncludeSqlServer)
using Microsoft.Data.SqlClient;
#elif (IncludeMySql)
using MySqlConnector;
#elif (IncludeSqlite)
using Microsoft.Data.Sqlite;
#endif
#endif
#if (IncludeAnyOrm)
using Company.ProjectName.Domain.Repositories;
#endif
#if (IncludeJwt || IncludeKeycloak)
using Microsoft.AspNetCore.Authentication.JwtBearer;
#endif
#if (IncludeAspNetIdentity)
using Microsoft.AspNetCore.Identity;
#endif
#if (IncludeApiKey)
using Microsoft.AspNetCore.Authentication;
using Company.ProjectName.Infrastructure.Auth;
#endif
#if (IncludeOpenTelemetry)
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
#endif
#if (IncludeHangfire)
using Hangfire;
#if (IncludeMySql)
using Hangfire.MySql;
#endif
#endif
#if (IncludeQuartz)
using Quartz;
using Company.ProjectName.Infrastructure.Jobs;
#endif
#if (IncludeIHostedService)
using Company.ProjectName.Infrastructure.Jobs;
#endif

namespace Company.ProjectName.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
#if (IncludeEfCore)
        services.AddDbContext<AppDbContext>(options =>
        {
#if (IncludePostgreSql)
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeSqlite)
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
#endif
        });
        // To create migrations: dotnet ef migrations add Initial --project ../Company.ProjectName.Infrastructure
        // To apply migrations: dotnet ef database update --project ../Company.ProjectName.Infrastructure

        services.AddScoped<ISampleRepository, SampleRepository>();
#endif
#if (IncludeDapper)
        services.AddScoped<IDbConnection>(sp =>
        {
#if (IncludePostgreSql)
            return new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
            return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
            return new MySqlConnection(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlite)
            return new SqliteConnection(configuration.GetConnectionString("DefaultConnection"));
#endif
        });

        services.AddScoped<ISampleRepository, SampleRepository>();
#endif

#if (IncludeJwt)
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Jwt:Issuer"];
                options.Audience = configuration["Authentication:Jwt:Audience"];
            });
        services.AddAuthorization();
#elif (IncludeKeycloak)
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keycloak realm URL, e.g. http://localhost:8080/realms/myrealm
                options.Authority = configuration["Authentication:Keycloak:Authority"];
                options.Audience = configuration["Authentication:Keycloak:Audience"];
            });
        services.AddAuthorization();
#elif (IncludeAspNetIdentity)
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        services.AddAuthorization();
        // Note: No UI pages or MapIdentityApi endpoints generated — add your own as needed
#elif (IncludeApiKey)
        services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, null);
        services.AddAuthorization();
#endif

#if (IncludeCaching)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
#endif
#if (IncludeResilience)
        services.ConfigureHttpClientDefaults(http =>
            http.AddStandardResilienceHandler());
#endif
#if (IncludeOpenTelemetry)
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("Company.ProjectName"))
            .WithTracing(t => t
#if (IncludeWebProject)
                .AddAspNetCoreInstrumentation()
#endif
                .AddHttpClientInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(m => m
#if (IncludeWebProject)
                .AddAspNetCoreInstrumentation()
#endif
                .AddHttpClientInstrumentation()
                .AddConsoleExporter());
#endif
#if (IncludeHangfire)
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
#if (IncludePostgreSql)
            .UsePostgreSqlStorage(c => c.UseConnectionString(
                configuration.GetConnectionString("DefaultConnection")!)));
#elif (IncludeSqlServer)
            .UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeMySql)
            .UseStorage(new MySqlStorage(
                configuration.GetConnectionString("DefaultConnection")!,
                new MySqlStorageOptions { TablesPrefix = "Hangfire" })));
#elif (IncludeSqlite)
            .UseSQLiteStorage());
#endif
        services.AddHangfireServer();
#endif
#if (IncludeQuartz)
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("SampleJob");
            q.AddJob<SampleJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("SampleJob-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(5)
                    .RepeatForever()));
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
#endif
#if (IncludeIHostedService)
        services.AddHostedService<SampleBackgroundService>();
#endif

        return services;
    }
}
