#if (IncludeWebApi)
#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
#endif
#if (IncludeSerilog)
using Serilog;
#endif
#if (IncludeNLog)
using NLog;
using NLog.Web;
#endif
#if (IncludeValidation)
using FluentValidation;
#endif
#if (IncludeOpenTelemetry)
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
#endif
#if (IncludeQuartz)
using Quartz;
#endif
#if (IncludeHangfire && IncludeMySql)
using Hangfire.MySql;
#endif
#if (IncludeScalar)
using Scalar.AspNetCore;
#endif
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

#if (IncludeEfCore)
builder.Services.AddDbContext<Company.ProjectName.Data.AppDbContext>(options =>
{
#if (IncludePostgreSql)
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeSqlite)
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
#endif
});
#elif (IncludeDapper)
builder.Services.AddScoped<System.Data.IDbConnection>(sp =>
{
#if (IncludePostgreSql)
    return new Npgsql.NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
    return new Microsoft.Data.SqlClient.SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
    return new MySqlConnector.MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlite)
    return new Microsoft.Data.Sqlite.SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
#endif
});
#endif

#if (IncludeSerilog)
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
#endif
#if (IncludeNLog)
builder.Logging.ClearProviders();
builder.Host.UseNLog();
#endif
#if (IncludeValidation)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
#endif
#if (IncludeCaching)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
#endif
#if (IncludeResilience)
builder.Services.ConfigureHttpClientDefaults(http =>
    http.AddStandardResilienceHandler());
#endif
#if (IncludeOpenTelemetry)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("Company.ProjectName"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());
#endif
#if (IncludeMapping)
Mapster.TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
#endif

#if (IncludeHangfire)
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
#if (IncludePostgreSql)
    .UsePostgreSqlStorage(c => c.UseConnectionString(
        builder.Configuration.GetConnectionString("DefaultConnection")!)));
#elif (IncludeSqlServer)
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeMySql)
    .UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        new MySqlStorageOptions { TablesPrefix = "Hangfire" })));
#elif (IncludeSqlite)
    .UseSQLiteStorage());
#endif
builder.Services.AddHangfireServer();
#endif
#if (IncludeQuartz)
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("SampleJob");
    q.AddJob<Company.ProjectName.Jobs.SampleJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("SampleJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(5)
            .RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
#endif
#if (IncludeIHostedService)
builder.Services.AddHostedService<Company.ProjectName.Jobs.SampleBackgroundService>();
#endif

#if (IncludeJwt)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Jwt:Issuer"];
        options.Audience = builder.Configuration["Authentication:Jwt:Audience"];
    });
builder.Services.AddAuthorization();
#elif (IncludeKeycloak)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keycloak realm URL, e.g. http://localhost:8080/realms/myrealm
        options.Authority = builder.Configuration["Authentication:Keycloak:Authority"];
        options.Audience = builder.Configuration["Authentication:Keycloak:Audience"];
        // Standard OIDC discovery — no explicit MetadataAddress needed
    });
builder.Services.AddAuthorization();
#elif (IncludeAspNetIdentity)
builder.Services.AddIdentity<Microsoft.AspNetCore.Identity.IdentityUser, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<Company.ProjectName.Data.AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
// Note: No UI pages or MapIdentityApi endpoints generated — add your own as needed
#elif (IncludeApiKey)
builder.Services.AddAuthentication(Company.ProjectName.Auth.ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, Company.ProjectName.Auth.ApiKeyAuthenticationHandler>(
        Company.ProjectName.Auth.ApiKeyAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();
#endif

#if (IncludeHealthChecks)
builder.Services.AddHealthChecks();
#endif

#if ((IncludeSwaggerUI || IncludeRedoc) && IncludeNet8)
builder.Services.AddSwaggerGen();
#endif

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
#if (IncludeSwaggerUI && IncludeNet8)
    app.UseSwagger();
    app.UseSwaggerUI();
#elif (IncludeSwaggerUI && !IncludeNet8)
    // net9/net10: OpenAPI document at /openapi/v1.json (from AddOpenApi)
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "API v1"));
#endif
#if (IncludeRedoc && IncludeNet8)
    app.UseSwagger();
    app.UseReDoc(c => c.SpecUrl("/swagger/v1/swagger.json"));
#elif (IncludeRedoc && !IncludeNet8)
    app.UseReDoc(c => c.SpecUrl("/openapi/v1.json"));
#endif
#if (IncludeScalar && IncludeNet8)
    app.MapScalarApiReference(options => options
        .WithOpenApiRoutePattern("/swagger/v1/swagger.json"));
#elif (IncludeScalar && !IncludeNet8)
    app.MapScalarApiReference();
#endif
}

#if (IncludeHangfire)
app.UseHangfireDashboard("/hangfire");
#endif
app.UseHttpsRedirection();
#if (IncludeAnyAuth)
app.UseAuthentication();
#endif
app.UseAuthorization();
app.MapControllers();
#if (IncludeHealthChecks)
app.MapHealthChecks("/health");
#endif

app.Run();
#elif (IncludeMinimalApi)
#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
#endif
#if (IncludeSerilog)
using Serilog;
#endif
#if (IncludeNLog)
using NLog;
using NLog.Web;
#endif
#if (IncludeValidation)
using FluentValidation;
#endif
#if (IncludeOpenTelemetry)
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
#endif
#if (IncludeQuartz)
using Quartz;
#endif
#if (IncludeHangfire && IncludeMySql)
using Hangfire.MySql;
#endif
#if (IncludeScalar)
using Scalar.AspNetCore;
#endif
var builder = WebApplication.CreateBuilder(args);

#if (IncludeEfCore)
builder.Services.AddDbContext<Company.ProjectName.Data.AppDbContext>(options =>
{
#if (IncludePostgreSql)
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeSqlite)
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
#endif
});
#elif (IncludeDapper)
builder.Services.AddScoped<System.Data.IDbConnection>(sp =>
{
#if (IncludePostgreSql)
    return new Npgsql.NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
    return new Microsoft.Data.SqlClient.SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
    return new MySqlConnector.MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlite)
    return new Microsoft.Data.Sqlite.SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
#endif
});
#endif

#if (IncludeSerilog)
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
#endif
#if (IncludeNLog)
builder.Logging.ClearProviders();
builder.Host.UseNLog();
#endif
#if (IncludeValidation)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
#endif
#if (IncludeCaching)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
#endif
#if (IncludeResilience)
builder.Services.ConfigureHttpClientDefaults(http =>
    http.AddStandardResilienceHandler());
#endif
#if (IncludeOpenTelemetry)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("Company.ProjectName"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());
#endif
#if (IncludeMapping)
Mapster.TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
#endif

#if (IncludeHangfire)
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
#if (IncludePostgreSql)
    .UsePostgreSqlStorage(c => c.UseConnectionString(
        builder.Configuration.GetConnectionString("DefaultConnection")!)));
#elif (IncludeSqlServer)
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeMySql)
    .UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        new MySqlStorageOptions { TablesPrefix = "Hangfire" })));
#elif (IncludeSqlite)
    .UseSQLiteStorage());
#endif
builder.Services.AddHangfireServer();
#endif
#if (IncludeQuartz)
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("SampleJob");
    q.AddJob<Company.ProjectName.Jobs.SampleJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("SampleJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(5)
            .RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
#endif
#if (IncludeIHostedService)
builder.Services.AddHostedService<Company.ProjectName.Jobs.SampleBackgroundService>();
#endif

#if (IncludeJwt)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Jwt:Issuer"];
        options.Audience = builder.Configuration["Authentication:Jwt:Audience"];
    });
builder.Services.AddAuthorization();
#elif (IncludeKeycloak)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keycloak realm URL, e.g. http://localhost:8080/realms/myrealm
        options.Authority = builder.Configuration["Authentication:Keycloak:Authority"];
        options.Audience = builder.Configuration["Authentication:Keycloak:Audience"];
        // Standard OIDC discovery — no explicit MetadataAddress needed
    });
builder.Services.AddAuthorization();
#elif (IncludeAspNetIdentity)
builder.Services.AddIdentity<Microsoft.AspNetCore.Identity.IdentityUser, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<Company.ProjectName.Data.AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
// Note: No UI pages or MapIdentityApi endpoints generated — add your own as needed
#elif (IncludeApiKey)
builder.Services.AddAuthentication(Company.ProjectName.Auth.ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, Company.ProjectName.Auth.ApiKeyAuthenticationHandler>(
        Company.ProjectName.Auth.ApiKeyAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();
#endif

#if (IncludeHealthChecks)
builder.Services.AddHealthChecks();
#endif

#if ((IncludeSwaggerUI || IncludeRedoc) && IncludeNet8)
builder.Services.AddSwaggerGen();
#endif

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
#if (IncludeSwaggerUI && IncludeNet8)
    app.UseSwagger();
    app.UseSwaggerUI();
#elif (IncludeSwaggerUI && !IncludeNet8)
    // net9/net10: OpenAPI document at /openapi/v1.json (from AddOpenApi)
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "API v1"));
#endif
#if (IncludeRedoc && IncludeNet8)
    app.UseSwagger();
    app.UseReDoc(c => c.SpecUrl("/swagger/v1/swagger.json"));
#elif (IncludeRedoc && !IncludeNet8)
    app.UseReDoc(c => c.SpecUrl("/openapi/v1.json"));
#endif
#if (IncludeScalar && IncludeNet8)
    app.MapScalarApiReference(options => options
        .WithOpenApiRoutePattern("/swagger/v1/swagger.json"));
#elif (IncludeScalar && !IncludeNet8)
    app.MapScalarApiReference();
#endif
}

#if (IncludeHangfire)
app.UseHangfireDashboard("/hangfire");
#endif
app.UseHttpsRedirection();
#if (IncludeAnyAuth)
app.UseAuthentication();
#endif

app.MapGet("/hello", () => "Hello World");
#if (IncludeHealthChecks)
app.MapHealthChecks("/health");
#endif

app.Run();
#elif (IncludeConsole)
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#if (IncludeSerilog)
using Serilog;
#endif
#if (IncludeNLog)
using NLog.Extensions.Hosting;
#endif
#if (IncludeValidation)
using FluentValidation;
#endif
#if (IncludeOpenTelemetry)
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
#endif

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register your services here
#if (IncludeEfCore)
        services.AddDbContext<Company.ProjectName.Data.AppDbContext>(options =>
        {
#if (IncludePostgreSql)
            options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
            options.UseMySql(
                context.Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(context.Configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeSqlite)
            options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection"));
#endif
        });
#elif (IncludeDapper)
        services.AddScoped<System.Data.IDbConnection>(sp =>
        {
#if (IncludePostgreSql)
            return new Npgsql.NpgsqlConnection(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
            return new Microsoft.Data.SqlClient.SqlConnection(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
            return new MySqlConnector.MySqlConnection(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlite)
            return new Microsoft.Data.Sqlite.SqliteConnection(context.Configuration.GetConnectionString("DefaultConnection"));
#endif
        });
#endif
#if (IncludeValidation)
        services.AddValidatorsFromAssemblyContaining<Program>();
#endif
#if (IncludeCaching)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = context.Configuration.GetConnectionString("Redis");
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
                .AddHttpClientInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(m => m
                .AddHttpClientInstrumentation()
                .AddConsoleExporter());
#endif
#if (IncludeMapping)
        Mapster.TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
#endif
    })
#if (IncludeSerilog)
    .UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration))
#endif
#if (IncludeNLog)
    .ConfigureLogging(logging => logging.ClearProviders())
    .UseNLog()
#endif
    .Build();

Console.WriteLine("Hello from Company.ProjectName!");

await host.RunAsync();
#elif (IncludeWorker)
using Company.ProjectName;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#if (IncludeSerilog)
using Serilog;
#endif
#if (IncludeNLog)
using NLog.Extensions.Hosting;
#endif
#if (IncludeValidation)
using FluentValidation;
#endif
#if (IncludeOpenTelemetry)
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
#endif
#if (IncludeQuartz)
using Quartz;
#endif
#if (IncludeHangfire && IncludeMySql)
using Hangfire.MySql;
#endif

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
#if (IncludeEfCore)
        services.AddDbContext<Company.ProjectName.Data.AppDbContext>(options =>
        {
#if (IncludePostgreSql)
            options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
            options.UseMySql(
                context.Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(context.Configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeSqlite)
            options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection"));
#endif
        });
#elif (IncludeDapper)
        services.AddScoped<System.Data.IDbConnection>(sp =>
        {
#if (IncludePostgreSql)
            return new Npgsql.NpgsqlConnection(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
            return new Microsoft.Data.SqlClient.SqlConnection(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
            return new MySqlConnector.MySqlConnection(context.Configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlite)
            return new Microsoft.Data.Sqlite.SqliteConnection(context.Configuration.GetConnectionString("DefaultConnection"));
#endif
        });
#endif
#if (IncludeValidation)
        services.AddValidatorsFromAssemblyContaining<Program>();
#endif
#if (IncludeCaching)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = context.Configuration.GetConnectionString("Redis");
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
                .AddHttpClientInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(m => m
                .AddHttpClientInstrumentation()
                .AddConsoleExporter());
#endif
#if (IncludeMapping)
        Mapster.TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
#endif
#if (IncludeHangfire)
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
#if (IncludePostgreSql)
            .UsePostgreSqlStorage(c => c.UseConnectionString(
                context.Configuration.GetConnectionString("DefaultConnection")!)));
#elif (IncludeSqlServer)
            .UseSqlServerStorage(
                context.Configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeMySql)
            .UseStorage(new MySqlStorage(
                context.Configuration.GetConnectionString("DefaultConnection")!,
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
            q.AddJob<Company.ProjectName.Jobs.SampleJob>(opts => opts.WithIdentity(jobKey));
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
        services.AddHostedService<Company.ProjectName.Jobs.SampleBackgroundService>();
#endif
    })
#if (IncludeSerilog)
    .UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration))
#endif
#if (IncludeNLog)
    .ConfigureLogging(logging => logging.ClearProviders())
    .UseNLog()
#endif
    .Build();

await host.RunAsync();
#endif
