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

        // Conditional: EF Core connection strings
        if (config.Orm == OrmOption.EfCore && config.Database.HasValue)
        {
            var connectionString = config.Database switch
            {
                DatabaseOption.PostgreSql => $"Host=localhost;Database={config.ProjectName};Username=postgres;Password=postgres",
                DatabaseOption.SqlServer => $"Server=localhost;Database={config.ProjectName};Trusted_Connection=true;TrustServerCertificate=true",
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
