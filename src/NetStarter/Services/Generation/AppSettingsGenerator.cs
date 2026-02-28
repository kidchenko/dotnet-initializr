using System.Text.Json;
using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class AppSettingsGenerator
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
    };

    public static string GenerateAppSettings(ProjectConfiguration config)
    {
        var settings = new Dictionary<string, object>
        {
            ["Logging"] = new Dictionary<string, object>
            {
                ["LogLevel"] = new Dictionary<string, object>
                {
                    ["Default"] = "Information",
                    ["Microsoft.AspNetCore"] = "Warning",
                },
            },
            ["AllowedHosts"] = "*",
        };

        // Conditional: EF Core / Dapper connection strings
        if (config.Orm is OrmOption.EfCore or OrmOption.Dapper && config.Database.HasValue)
        {
            var projectNameLower = config.ProjectName.ToLowerInvariant();
            var connectionString = config.Database switch
            {
                DatabaseOption.PostgreSql => config.IncludeDockerCompose
                    ? $"Host=localhost;Port=5432;Database={projectNameLower}db;Username=postgres;Password=postgres"
                    : $"Host=YOUR_HOST;Port=5432;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD",
                DatabaseOption.SqlServer => config.IncludeDockerCompose
                    ? $"Server=localhost;Database={projectNameLower}db;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true"
                    : $"Server=YOUR_HOST;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true",
                DatabaseOption.MySql => config.IncludeDockerCompose
                    ? $"Server=localhost;Port=3306;Database={projectNameLower}db;User=root;Password=root"
                    : $"Server=YOUR_HOST;Port=3306;Database=YOUR_DB;User=YOUR_USER;Password=YOUR_PASSWORD",
                DatabaseOption.Sqlite => "Data Source=app.db",
                _ => string.Empty,
            };

            if (!string.IsNullOrEmpty(connectionString))
            {
                settings["ConnectionStrings"] = new Dictionary<string, object>
                {
                    ["DefaultConnection"] = connectionString,
                };
            }
        }

        // Conditional: Redis connection string
        if (config.IncludeRedis)
        {
            var redisConn = config.IncludeDockerCompose ? "localhost:6379" : "YOUR_REDIS_HOST:6379";
            if (settings.TryGetValue("ConnectionStrings", out var existingCs) && existingCs is Dictionary<string, object> csDict)
            {
                csDict["Redis"] = redisConn;
            }
            else
            {
                settings["ConnectionStrings"] = new Dictionary<string, object>
                {
                    ["Redis"] = redisConn,
                };
            }
        }

        // Conditional: JWT settings
        if (config.Auth == AuthOption.Jwt)
        {
            settings["Jwt"] = new Dictionary<string, object>
            {
                ["Issuer"] = "https://localhost",
                ["Audience"] = "https://localhost",
                ["Key"] = "YOUR-SECRET-KEY-REPLACE-WITH-A-SECURE-VALUE-AT-LEAST-32-CHARS",
            };
        }

        // Conditional: Serilog settings
        if (config.Logging == LoggingOption.Serilog)
        {
            settings["Serilog"] = new Dictionary<string, object>
            {
                ["MinimumLevel"] = new Dictionary<string, object>
                {
                    ["Default"] = "Information",
                    ["Override"] = new Dictionary<string, object>
                    {
                        ["Microsoft.AspNetCore"] = "Warning",
                    },
                },
                ["WriteTo"] = new List<object>
                {
                    new Dictionary<string, object> { ["Name"] = "Console" },
                    new Dictionary<string, object>
                    {
                        ["Name"] = "File",
                        ["Args"] = new Dictionary<string, object>
                        {
                            ["path"] = "logs/log-.txt",
                            ["rollingInterval"] = "Day",
                        },
                    },
                },
            };
        }

        return JsonSerializer.Serialize(settings, _jsonOptions);
    }

    public static string GenerateAppSettingsDevelopment(ProjectConfiguration config)
    {
        var settings = new Dictionary<string, object>
        {
            ["Logging"] = new Dictionary<string, object>
            {
                ["LogLevel"] = new Dictionary<string, object>
                {
                    ["Default"] = "Debug",
                    ["Microsoft.AspNetCore"] = "Information",
                },
            },
        };

        // Conditional: Serilog development override
        if (config.Logging == LoggingOption.Serilog)
        {
            settings["Serilog"] = new Dictionary<string, object>
            {
                ["MinimumLevel"] = new Dictionary<string, object>
                {
                    ["Default"] = "Debug",
                },
            };
        }

        return JsonSerializer.Serialize(settings, _jsonOptions);
    }
}
