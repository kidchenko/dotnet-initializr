#if (IncludeWebApi)
#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
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
#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    })
    .Build();

Console.WriteLine("Hello from Company.ProjectName!");

await host.RunAsync();
#elif (IncludeWorker)
using Company.ProjectName;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    })
    .Build();

await host.RunAsync();
#endif
