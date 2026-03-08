#if (IncludeWebApi)
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

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
    .AddEntityFrameworkStores<Company.ProjectName.Infrastructure.Data.AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
// Note: No UI pages or MapIdentityApi endpoints generated — add your own as needed
#elif (IncludeApiKey)
builder.Services.AddAuthentication(Company.ProjectName.__EntryPoint__.Auth.ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, Company.ProjectName.__EntryPoint__.Auth.ApiKeyAuthenticationHandler>(
        Company.ProjectName.__EntryPoint__.Auth.ApiKeyAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();
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

app.Run();
#elif (IncludeMinimalApi)
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
    .AddEntityFrameworkStores<Company.ProjectName.Infrastructure.Data.AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
// Note: No UI pages or MapIdentityApi endpoints generated — add your own as needed
#elif (IncludeApiKey)
builder.Services.AddAuthentication(Company.ProjectName.__EntryPoint__.Auth.ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, Company.ProjectName.__EntryPoint__.Auth.ApiKeyAuthenticationHandler>(
        Company.ProjectName.__EntryPoint__.Auth.ApiKeyAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();
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

app.Run();
#elif (IncludeConsole)
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
    })
    .Build();

Console.WriteLine("Hello from Company.ProjectName!");

await host.RunAsync();
#elif (IncludeWorker)
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
#endif
