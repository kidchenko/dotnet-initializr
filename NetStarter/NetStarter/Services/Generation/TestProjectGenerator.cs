using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class TestProjectGenerator
{
    public static string GenerateSampleUnitTest(ProjectConfiguration config)
    {
        var ns = config.Namespace;

        return
            $"namespace {ns}.Tests;\n" +
            $"\n" +
            $"using Xunit;\n" +
            $"using FluentAssertions;\n" +
            $"\n" +
            $"public class SampleTests\n" +
            $"{{\n" +
            $"    [Fact]\n" +
            $"    public void Sample_ShouldPass()\n" +
            $"    {{\n" +
            $"        var result = 1 + 1;\n" +
            $"        result.Should().Be(2);\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GenerateIntegrationTest(ProjectConfiguration config)
    {
        var ns = config.Namespace;
        var isCleanArch = config.Architecture == ArchitecturePattern.CleanArchitecture;

        var dbContextNamespace = isCleanArch
            ? $"{ns}.Infrastructure.Data"
            : $"{ns}.Data";

        if (config.Database == DatabaseOption.PostgreSql)
        {
            return
                $"namespace {ns}.IntegrationTests;\n" +
                $"\n" +
                $"using Xunit;\n" +
                $"using FluentAssertions;\n" +
                $"using Microsoft.EntityFrameworkCore;\n" +
                $"using Testcontainers.PostgreSql;\n" +
                $"using {dbContextNamespace};\n" +
                $"\n" +
                $"public class DatabaseIntegrationTests : IAsyncLifetime\n" +
                $"{{\n" +
                $"    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(\"postgres:16-alpine\").Build();\n" +
                $"\n" +
                $"    public async Task InitializeAsync() => await _container.StartAsync();\n" +
                $"    public async Task DisposeAsync() => await _container.DisposeAsync();\n" +
                $"\n" +
                $"    [Fact]\n" +
                $"    public async Task Database_ShouldConnect()\n" +
                $"    {{\n" +
                $"        var options = new DbContextOptionsBuilder<AppDbContext>()\n" +
                $"            .UseNpgsql(_container.GetConnectionString())\n" +
                $"            .Options;\n" +
                $"        await using var context = new AppDbContext(options);\n" +
                $"        var canConnect = await context.Database.CanConnectAsync();\n" +
                $"        canConnect.Should().BeTrue();\n" +
                $"    }}\n" +
                $"}}\n";
        }
        else
        {
            return
                $"namespace {ns}.IntegrationTests;\n" +
                $"\n" +
                $"using Xunit;\n" +
                $"using FluentAssertions;\n" +
                $"using Microsoft.EntityFrameworkCore;\n" +
                $"using Testcontainers.MsSql;\n" +
                $"using {dbContextNamespace};\n" +
                $"\n" +
                $"public class DatabaseIntegrationTests : IAsyncLifetime\n" +
                $"{{\n" +
                $"    private readonly MsSqlContainer _container = new MsSqlBuilder(\"mcr.microsoft.com/mssql/server:2022-latest\").Build();\n" +
                $"\n" +
                $"    public async Task InitializeAsync() => await _container.StartAsync();\n" +
                $"    public async Task DisposeAsync() => await _container.DisposeAsync();\n" +
                $"\n" +
                $"    [Fact]\n" +
                $"    public async Task Database_ShouldConnect()\n" +
                $"    {{\n" +
                $"        var options = new DbContextOptionsBuilder<AppDbContext>()\n" +
                $"            .UseSqlServer(_container.GetConnectionString())\n" +
                $"            .Options;\n" +
                $"        await using var context = new AppDbContext(options);\n" +
                $"        var canConnect = await context.Database.CanConnectAsync();\n" +
                $"        canConnect.Should().BeTrue();\n" +
                $"    }}\n" +
                $"}}\n";
        }
    }
}
