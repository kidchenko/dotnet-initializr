using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class DapperGenerator
{
    public static string Generate(ProjectConfiguration config)
    {
        var ns = GetNamespace(config);
        return config.Database switch
        {
            DatabaseOption.PostgreSql => GenerateDbDataSource(ns, "Npgsql", "NpgsqlDataSource", "NpgsqlDataSource.Create"),
            DatabaseOption.MySql => GenerateDbDataSource(ns, "MySqlConnector", "MySqlDataSource", "new MySqlDataSource"),
            DatabaseOption.Sqlite => GenerateIDbConnection(ns, "Microsoft.Data.Sqlite", "SqliteConnection"),
            DatabaseOption.SqlServer => GenerateIDbConnection(ns, "Microsoft.Data.SqlClient", "SqlConnection"),
            _ => GenerateDbDataSource(ns, "Npgsql", "NpgsqlDataSource", "NpgsqlDataSource.Create"),
        };
    }

    private static string GenerateDbDataSource(string ns, string usingDirective, string dataSourceType, string factoryExpression)
    {
        return
            $"using {usingDirective};\n" +
            $"using Microsoft.Extensions.DependencyInjection;\n" +
            $"using Microsoft.Extensions.Configuration;\n" +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public static class DapperExtensions\n" +
            $"{{\n" +
            $"    public static IServiceCollection AddDapperConnection(\n" +
            $"        this IServiceCollection services, IConfiguration configuration)\n" +
            $"    {{\n" +
            $"        var connectionString = configuration.GetConnectionString(\"DefaultConnection\")!;\n" +
            $"        services.AddSingleton({factoryExpression}(connectionString));\n" +
            $"        return services;\n" +
            $"    }}\n" +
            $"}}\n";
    }

    private static string GenerateIDbConnection(string ns, string usingDirective, string connectionType)
    {
        return
            $"using {usingDirective};\n" +
            $"using System.Data;\n" +
            $"using Microsoft.Extensions.DependencyInjection;\n" +
            $"using Microsoft.Extensions.Configuration;\n" +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public static class DapperExtensions\n" +
            $"{{\n" +
            $"    public static IServiceCollection AddDapperConnection(\n" +
            $"        this IServiceCollection services, IConfiguration configuration)\n" +
            $"    {{\n" +
            $"        var connectionString = configuration.GetConnectionString(\"DefaultConnection\")!;\n" +
            $"        services.AddScoped<IDbConnection>(_ => new {connectionType}(connectionString));\n" +
            $"        return services;\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GetNamespace(ProjectConfiguration config)
    {
        var suffix = config.Architecture switch
        {
            ArchitecturePattern.CleanArchitecture => "Infrastructure.Data",
            _ => "Data",
        };
        return $"{config.Namespace}.{suffix}";
    }

    public static string GetFilePath(ProjectConfiguration config)
    {
        return config.Architecture switch
        {
            ArchitecturePattern.CleanArchitecture => $"src/{config.ProjectName}.Infrastructure/Data/DapperExtensions.cs",
            _ => $"src/{config.ProjectName}/Data/DapperExtensions.cs",
        };
    }
}
