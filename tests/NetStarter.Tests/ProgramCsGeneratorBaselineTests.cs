using NetStarter.Models;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Baseline regression tests for ProgramCsGenerator v1.0 outputs.
/// These tests capture the exact byte-for-byte output before the
/// GenerateWebApplication refactor (04-01 foundation plan).
/// All assertions must remain green after the refactor.
/// </summary>
public class ProgramCsGeneratorBaselineTests
{
    // ---- Config 1: WebApi + CleanArchitecture + EfCore/PostgreSql + Jwt + Serilog + HealthChecks + OpenTelemetry + Mapster ----

    private static ProjectConfiguration AllFeaturesConfig() => new()
    {
        ProjectName = "MyProject",
        Namespace = "MyProject",
        ProjectType = ProjectType.WebApi,
        Architecture = ArchitecturePattern.CleanArchitecture,
        Orm = OrmOption.EfCore,
        Database = DatabaseOption.PostgreSql,
        Auth = AuthOption.Jwt,
        Logging = LoggingOption.Serilog,
        IncludeHealthChecks = true,
        IncludeOpenTelemetry = true,
        Mapping = MappingOption.Mapster,
    };

    // ---- Config 2: MinimalApi + SimpleLayered + None ORM + no auth + no observability ----

    private static ProjectConfiguration MinimalWebConfig() => new()
    {
        ProjectName = "MyProject",
        Namespace = "MyProject",
        ProjectType = ProjectType.MinimalApi,
        Architecture = ArchitecturePattern.SimpleLayered,
        Orm = OrmOption.None,
        Auth = AuthOption.None,
        Mapping = MappingOption.None,
    };

    // ---- Config 3: WebApi + VerticalSlice + EfCore/SqlServer + no auth + Serilog only ----

    private static ProjectConfiguration VerticalSliceConfig() => new()
    {
        ProjectName = "MyProject",
        Namespace = "MyProject",
        ProjectType = ProjectType.WebApi,
        Architecture = ArchitecturePattern.VerticalSlice,
        Orm = OrmOption.EfCore,
        Database = DatabaseOption.SqlServer,
        Auth = AuthOption.None,
        Logging = LoggingOption.Serilog,
        Mapping = MappingOption.None,
    };

    // ---- Config 4: Console project ----

    private static ProjectConfiguration ConsoleConfig() => new()
    {
        ProjectName = "MyProject",
        Namespace = "MyProject",
        ProjectType = ProjectType.Console,
        Architecture = ArchitecturePattern.CleanArchitecture,
    };

    // ---- Config 5: WorkerService project ----

    private static ProjectConfiguration WorkerServiceConfig() => new()
    {
        ProjectName = "MyProject",
        Namespace = "MyProject",
        ProjectType = ProjectType.WorkerService,
    };

    [Fact]
    public void Generate_AllFeatures_WebApi_ProducesExpectedOutput()
    {
        var config = AllFeaturesConfig();
        var result = ProgramCsGenerator.Generate(config);

        // Usings
        Assert.Contains("using Microsoft.EntityFrameworkCore;\n", result);
        Assert.Contains("using MyProject.Infrastructure.Data;\n", result);
        Assert.Contains("using Microsoft.AspNetCore.Authentication.JwtBearer;\n", result);
        Assert.Contains("using Microsoft.IdentityModel.Tokens;\n", result);
        Assert.Contains("using System.Text;\n", result);
        Assert.Contains("using Serilog;\n", result);
        Assert.Contains("using MyProject.Api.Telemetry;\n", result);
        Assert.Contains("using Mapster;\n", result);
        Assert.Contains("using MapsterMapper;\n", result);

        // Builder creation
        Assert.Contains("var builder = WebApplication.CreateBuilder(args);\n", result);

        // Service registrations
        Assert.Contains("builder.Host.UseSerilog((context, cfg) =>\n    cfg.ReadFrom.Configuration(context.Configuration));\n", result);
        Assert.Contains("builder.Services.AddDbContext<AppDbContext>(options =>\n    options.UseNpgsql(builder.Configuration.GetConnectionString(\"DefaultConnection\")));\n", result);
        Assert.Contains("builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)\n", result);
        Assert.Contains("builder.Services.AddHealthChecks();\n", result);
        Assert.Contains("builder.Services.AddAppOpenTelemetry(builder.Configuration);\n", result);
        Assert.Contains("builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);\n", result);
        Assert.Contains("builder.Services.AddScoped<IMapper, ServiceMapper>();\n", result);
        Assert.Contains("builder.Services.AddControllers();\n", result);

        // App build
        Assert.Contains("var app = builder.Build();\n", result);

        // Middleware
        Assert.Contains("app.UseAuthentication();\n", result);
        Assert.Contains("app.UseAuthorization();\n", result);
        Assert.Contains("app.MapHealthChecks(\"/health\");\n", result);
        Assert.Contains("app.MapControllers();\n", result);

        // End
        Assert.Contains("app.Run();\n", result);
    }

    [Fact]
    public void Generate_AllFeatures_WebApi_ProducesExactOutput()
    {
        var config = AllFeaturesConfig();
        var result = ProgramCsGenerator.Generate(config);

        const string expected =
            "using Microsoft.EntityFrameworkCore;\n" +
            "using MyProject.Infrastructure.Data;\n" +
            "using Microsoft.AspNetCore.Authentication.JwtBearer;\n" +
            "using Microsoft.IdentityModel.Tokens;\n" +
            "using System.Text;\n" +
            "using Serilog;\n" +
            "using MyProject.Api.Telemetry;\n" +
            "using Mapster;\n" +
            "using MapsterMapper;\n" +
            "\n" +
            "var builder = WebApplication.CreateBuilder(args);\n" +
            "\n" +
            "builder.Host.UseSerilog((context, cfg) =>\n" +
            "    cfg.ReadFrom.Configuration(context.Configuration));\n" +
            "\n" +
            "builder.Services.AddDbContext<AppDbContext>(options =>\n" +
            "    options.UseNpgsql(builder.Configuration.GetConnectionString(\"DefaultConnection\")));\n" +
            "builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)\n" +
            "    .AddJwtBearer(options =>\n" +
            "    {\n" +
            "        options.TokenValidationParameters = new TokenValidationParameters\n" +
            "        {\n" +
            "            ValidateIssuer = true,\n" +
            "            ValidateAudience = true,\n" +
            "            ValidateLifetime = true,\n" +
            "            ValidateIssuerSigningKey = true,\n" +
            "            ValidIssuer = builder.Configuration[\"Jwt:Issuer\"],\n" +
            "            ValidAudience = builder.Configuration[\"Jwt:Audience\"],\n" +
            "            IssuerSigningKey = new SymmetricSecurityKey(\n" +
            "                Encoding.UTF8.GetBytes(builder.Configuration[\"Jwt:Key\"]!))\n" +
            "        };\n" +
            "    });\n" +
            "builder.Services.AddAuthorization();\n" +
            "builder.Services.AddHealthChecks();\n" +
            "builder.Services.AddAppOpenTelemetry(builder.Configuration);\n" +
            "builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);\n" +
            "builder.Services.AddScoped<IMapper, ServiceMapper>();\n" +
            "builder.Services.AddControllers();\n" +
            "\n" +
            "var app = builder.Build();\n" +
            "\n" +
            "app.UseAuthentication();\n" +
            "app.UseAuthorization();\n" +
            "app.MapHealthChecks(\"/health\");\n" +
            "app.MapControllers();\n" +
            "\n" +
            "app.Run();\n";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_MinimalWeb_ProducesExactOutput()
    {
        var config = MinimalWebConfig();
        var result = ProgramCsGenerator.Generate(config);

        const string expected =
            "var builder = WebApplication.CreateBuilder(args);\n" +
            "\n" +
            "builder.Services.AddEndpointsApiExplorer();\n" +
            "\n" +
            "var app = builder.Build();\n" +
            "\n" +
            "app.MapGet(\"/api/hello\", () => new { Message = \"Hello from MyProject!\" });\n" +
            "\n" +
            "app.Run();\n";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_VerticalSlice_EfCore_Serilog_ProducesExactOutput()
    {
        var config = VerticalSliceConfig();
        var result = ProgramCsGenerator.Generate(config);

        const string expected =
            "using Microsoft.EntityFrameworkCore;\n" +
            "using MyProject.Data;\n" +
            "using Serilog;\n" +
            "\n" +
            "var builder = WebApplication.CreateBuilder(args);\n" +
            "\n" +
            "builder.Host.UseSerilog((context, cfg) =>\n" +
            "    cfg.ReadFrom.Configuration(context.Configuration));\n" +
            "\n" +
            "builder.Services.AddDbContext<AppDbContext>(options =>\n" +
            "    options.UseSqlServer(builder.Configuration.GetConnectionString(\"DefaultConnection\")));\n" +
            "builder.Services.AddControllers();\n" +
            "\n" +
            "var app = builder.Build();\n" +
            "\n" +
            "app.MapControllers();\n" +
            "\n" +
            "app.Run();\n";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_Console_ProducesExactOutput()
    {
        var config = ConsoleConfig();
        var result = ProgramCsGenerator.Generate(config);

        const string expected =
            "using Microsoft.Extensions.Configuration;\n" +
            "using Microsoft.Extensions.DependencyInjection;\n" +
            "using MyProject.Cli;\n" +
            "\n" +
            "var configuration = new ConfigurationBuilder()\n" +
            "    .AddJsonFile(\"appsettings.json\")\n" +
            "    .AddJsonFile(\"appsettings.Development.json\", optional: true)\n" +
            "    .Build();\n" +
            "\n" +
            "var services = new ServiceCollection();\n" +
            "services.AddAppServices(configuration);\n" +
            "\n" +
            "var serviceProvider = services.BuildServiceProvider();\n" +
            "\n" +
            "Console.WriteLine(\"Hello from MyProject!\");\n";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_WorkerService_ProducesExactOutput()
    {
        var config = WorkerServiceConfig();
        var result = ProgramCsGenerator.Generate(config);

        const string expected =
            "\n" +
            "var builder = Host.CreateApplicationBuilder(args);\n" +
            "\n" +
            "var host = builder.Build();\n" +
            "host.Run();\n";

        Assert.Equal(expected, result);
    }
}
