using System.Text;
using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class ReadmeGenerator
{
    public static string Generate(ProjectConfiguration config)
    {
        var sb = new StringBuilder();
        var name = config.ProjectName;
        var major = GetSdkMajorVersion(config.SdkVersion);
        var tf = NuGetVersionMap.GetTargetFramework(config.SdkVersion);
        var isCleanArch = config.Architecture == ArchitecturePattern.CleanArchitecture;
        var hasPostgres = config.Orm == OrmOption.EfCore && config.Database == DatabaseOption.PostgreSql;
        var hasSqlServer = config.Orm == OrmOption.EfCore && config.Database == DatabaseOption.SqlServer;
        var hasEfCore = config.Orm == OrmOption.EfCore;

        // Title
        sb.AppendLine($"# {name}");
        sb.AppendLine();

        // Prerequisites
        sb.AppendLine("## Prerequisites");
        sb.AppendLine();
        sb.AppendLine($"- [.NET SDK {major}.0](https://dotnet.microsoft.com/download/dotnet/{major}.0) (minimum 8.0.400 for .slnx solution format support)");

        if (config.IncludeDockerfile || config.IncludeDockerCompose)
        {
            sb.AppendLine("- [Docker](https://www.docker.com/get-started) for containerization");
        }

        if (config.IncludeDotNetAspire)
        {
            sb.AppendLine("- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling): `dotnet workload install aspire`");
        }

        sb.AppendLine();

        // Getting Started
        sb.AppendLine("## Getting Started");
        sb.AppendLine();
        sb.AppendLine("```bash");
        sb.AppendLine("dotnet restore");
        sb.AppendLine("dotnet build");

        if (isCleanArch)
        {
            sb.AppendLine($"dotnet run --project src/{name}.{config.EntryPointSuffix}");
        }
        else
        {
            sb.AppendLine($"dotnet run --project src/{name}");
        }

        sb.AppendLine("```");
        sb.AppendLine();

        // Database Setup
        if (hasEfCore)
        {
            sb.AppendLine("## Database Setup");
            sb.AppendLine();

            if (hasPostgres)
            {
                sb.AppendLine("Start a local PostgreSQL instance:");
                sb.AppendLine();
                sb.AppendLine("```bash");
                sb.AppendLine($"docker run -d --name {name.ToLowerInvariant()}-postgres \\");
                sb.AppendLine("  -e POSTGRES_USER=postgres \\");
                sb.AppendLine("  -e POSTGRES_PASSWORD=postgres \\");
                sb.AppendLine($"  -e POSTGRES_DB={name.ToLowerInvariant()}db \\");
                sb.AppendLine("  -p 5432:5432 \\");
                sb.AppendLine("  postgres:16-alpine");
                sb.AppendLine("```");
                sb.AppendLine();
                sb.AppendLine("Update the connection string in `appsettings.Development.json` if needed.");
            }
            else if (hasSqlServer)
            {
                sb.AppendLine("Start a local SQL Server instance:");
                sb.AppendLine();
                sb.AppendLine("```bash");
                sb.AppendLine($"docker run -d --name {name.ToLowerInvariant()}-sqlserver \\");
                sb.AppendLine("  -e \"SA_PASSWORD=YourStrong!Passw0rd\" \\");
                sb.AppendLine("  -e \"ACCEPT_EULA=Y\" \\");
                sb.AppendLine("  -p 1433:1433 \\");
                sb.AppendLine("  mcr.microsoft.com/mssql/server:2022-latest");
                sb.AppendLine("```");
                sb.AppendLine();
                sb.AppendLine("Update the connection string in `appsettings.Development.json` if needed.");
            }

            sb.AppendLine();
            sb.AppendLine("Apply migrations:");
            sb.AppendLine();
            sb.AppendLine("```bash");

            if (isCleanArch)
            {
                sb.AppendLine($"dotnet ef database update --project src/{name}.Infrastructure --startup-project src/{name}.{config.EntryPointSuffix}");
            }
            else
            {
                sb.AppendLine($"dotnet ef database update --project src/{name}");
            }

            sb.AppendLine("```");
            sb.AppendLine();
        }

        // Running Tests
        if (config.HasTestFramework || config.IncludeTestcontainers)
        {
            sb.AppendLine("## Running Tests");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("dotnet test");
            sb.AppendLine("```");
            sb.AppendLine();
            var frameworkName = config.TestFramework switch
            {
                TestFrameworkOption.XUnit => "[xUnit](https://xunit.net/)",
                TestFrameworkOption.NUnit => "[NUnit](https://nunit.org/)",
                _ => "the configured test framework"
            };
            sb.AppendLine($"This project uses {frameworkName} as the test framework.");

            if (config.AssertLibrary == AssertLibraryOption.Shouldly)
                sb.AppendLine("Assertions use [Shouldly](https://docs.shouldly.org/).");

            if (config.IncludeTestcontainers)
            {
                sb.AppendLine("Integration tests use [Testcontainers](https://dotnet.testcontainers.org/) and require Docker to be running.");
            }

            sb.AppendLine();
        }

        // Docker
        if (config.IncludeDockerfile || config.IncludeDockerCompose)
        {
            sb.AppendLine("## Docker");
            sb.AppendLine();

            if (config.IncludeDockerfile)
            {
                sb.AppendLine("Build the Docker image:");
                sb.AppendLine();
                sb.AppendLine("```bash");
                sb.AppendLine($"docker build -t {name.ToLowerInvariant()} .");
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (config.IncludeDockerCompose)
            {
                sb.AppendLine("Run with Docker Compose:");
                sb.AppendLine();
                sb.AppendLine("```bash");
                sb.AppendLine("docker-compose up");
                sb.AppendLine("```");
                sb.AppendLine();
            }
        }

        // .NET Aspire
        if (config.IncludeDotNetAspire)
        {
            sb.AppendLine("## .NET Aspire");
            sb.AppendLine();
            sb.AppendLine(".NET Aspire provides a cloud-ready stack for observable, production-ready distributed applications.");
            sb.AppendLine();
            sb.AppendLine("Run the AppHost project to start the full application stack with the Aspire dashboard:");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine($"dotnet run --project src/{name}.AppHost");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("The Aspire dashboard will be available at the URL printed in the console output.");
            sb.AppendLine();
        }

        // Project Structure
        sb.AppendLine("## Project Structure");
        sb.AppendLine();

        var architectureName = config.Architecture switch
        {
            ArchitecturePattern.CleanArchitecture => "Clean Architecture",
            ArchitecturePattern.VerticalSlice => "Vertical Slice Architecture",
            ArchitecturePattern.SimpleLayered => "Simple Layered Architecture",
            _ => "Architecture",
        };

        sb.AppendLine($"This project follows **{architectureName}**.");
        sb.AppendLine();
        sb.AppendLine("```");

        if (config.Architecture == ArchitecturePattern.CleanArchitecture)
        {
            sb.AppendLine($"{name}/");
            sb.AppendLine("├── src/");
            sb.AppendLine($"│   ├── {name}.{config.EntryPointSuffix}/          # Web API entry point");
            sb.AppendLine($"│   ├── {name}.Application/  # Use cases and business logic");
            sb.AppendLine($"│   ├── {name}.Domain/       # Domain entities and interfaces");
            sb.AppendLine($"│   └── {name}.Infrastructure/ # Data access and external services");

            if (config.IncludeDotNetAspire)
            {
                sb.AppendLine($"│   ├── {name}.AppHost/     # .NET Aspire orchestrator");
                sb.AppendLine($"│   └── {name}.ServiceDefaults/ # Shared Aspire defaults");
            }

            if (config.HasTestFramework || config.IncludeTestcontainers)
            {
                sb.AppendLine("└── tests/");
                if (config.HasTestFramework)
                    sb.AppendLine($"    ├── {name}.Tests/   # Unit tests");
                if (config.IncludeTestcontainers)
                    sb.AppendLine($"    └── {name}.IntegrationTests/ # Integration tests");
            }
        }
        else
        {
            sb.AppendLine($"{name}/");
            sb.AppendLine("├── src/");
            sb.AppendLine($"│   └── {name}/              # Application code");

            if (config.HasTestFramework || config.IncludeTestcontainers)
            {
                sb.AppendLine("└── tests/");
                if (config.HasTestFramework)
                    sb.AppendLine($"    ├── {name}.Tests/   # Unit tests");
                if (config.IncludeTestcontainers)
                    sb.AppendLine($"    └── {name}.IntegrationTests/ # Integration tests");
            }
        }

        sb.AppendLine("```");
        sb.AppendLine();

        // Serilog note
        if (config.IncludeSerilog)
        {
            sb.AppendLine("## Logging");
            sb.AppendLine();
            sb.AppendLine("This project uses [Serilog](https://serilog.net/) for structured logging.");
            sb.AppendLine("Log files are written to the `logs/` directory (e.g., `logs/log-.txt`).");
            sb.AppendLine();
        }

        // OpenTelemetry note
        if (config.IncludeOpenTelemetry)
        {
            sb.AppendLine("## Observability");
            sb.AppendLine();
            sb.AppendLine("This project is instrumented with [OpenTelemetry](https://opentelemetry.io/).");
            sb.AppendLine("Configure the OTLP exporter endpoint via the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable.");
            sb.AppendLine("Example:");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static int GetSdkMajorVersion(DotNetSdkVersion sdk) => sdk switch
    {
        DotNetSdkVersion.Net8 => 8,
        DotNetSdkVersion.Net9 => 9,
        DotNetSdkVersion.Net10 => 10,
        _ => 9,
    };
}
