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
#if (IncludeScalar)
using Scalar.AspNetCore;
#endif
#if (IncludeHangfire)
using Hangfire;
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

#if (IncludeJwt)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();
#endif

#if (IncludeHealthChecks)
builder.Services.AddHealthChecks();
#endif

#if ((IncludeSwaggerUI || IncludeRedoc) && IncludeNet8)
builder.Services.AddSwaggerGen();
#endif
#if ((IncludeScalar || IncludeSwaggerUI || IncludeRedoc) && !IncludeNet8)
builder.Services.AddOpenApi();
#endif

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
#if ((IncludeScalar || IncludeSwaggerUI || IncludeRedoc) && !IncludeNet8)
    app.MapOpenApi();
#endif
#if (IncludeSwaggerUI && IncludeNet8)
    app.UseSwagger();
    app.UseSwaggerUI();
#elif (IncludeSwaggerUI && !IncludeNet8)
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
#if (IncludeHangfire)
    app.UseHangfireDashboard("/hangfire");
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
#if (IncludeScalar)
using Scalar.AspNetCore;
#endif
#if (IncludeHangfire)
using Hangfire;
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

#if (IncludeJwt)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();
#endif

#if (IncludeHealthChecks)
builder.Services.AddHealthChecks();
#endif

#if ((IncludeSwaggerUI || IncludeRedoc) && IncludeNet8)
builder.Services.AddSwaggerGen();
#endif
#if ((IncludeScalar || IncludeSwaggerUI || IncludeRedoc) && !IncludeNet8)
builder.Services.AddOpenApi();
#endif

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
#if ((IncludeScalar || IncludeSwaggerUI || IncludeRedoc) && !IncludeNet8)
    app.MapOpenApi();
#endif
#if (IncludeSwaggerUI && IncludeNet8)
    app.UseSwagger();
    app.UseSwaggerUI();
#elif (IncludeSwaggerUI && !IncludeNet8)
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
#if (IncludeHangfire)
    app.UseHangfireDashboard("/hangfire");
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
