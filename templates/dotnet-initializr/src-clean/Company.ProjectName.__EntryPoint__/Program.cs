#if (IncludeWebApi)
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;
#if (IncludeSerilog)
using Serilog;
#endif
#if (IncludeNLog)
using NLog;
using NLog.Web;
#endif

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

#if (IncludeSerilog)
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
#endif
#if (IncludeNLog)
builder.Logging.ClearProviders();
builder.Host.UseNLog();
#endif

#if (IncludeHealthChecks)
builder.Services.AddHealthChecks();
#endif

#if (IncludeSwaggerUI && IncludeNet8)
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
}

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
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;
#if (IncludeSerilog)
using Serilog;
#endif
#if (IncludeNLog)
using NLog;
using NLog.Web;
#endif

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

#if (IncludeSerilog)
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
#endif
#if (IncludeNLog)
builder.Logging.ClearProviders();
builder.Host.UseNLog();
#endif

#if (IncludeHealthChecks)
builder.Services.AddHealthChecks();
#endif

#if (IncludeSwaggerUI && IncludeNet8)
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
}

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
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#if (IncludeSerilog)
using Serilog;
#endif
#if (IncludeNLog)
using NLog.Extensions.Hosting;
#endif

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
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
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#if (IncludeSerilog)
using Serilog;
#endif
#if (IncludeNLog)
using NLog.Extensions.Hosting;
#endif

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
        services.AddHostedService<Worker>();
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
