using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class TestProjectGenerator
{
    public static string GenerateSampleUnitTest(ProjectConfiguration config)
    {
        var ns = config.Namespace;
        var usings = GetTestUsings(config);
        var (attribute, body) = GetUnitTestBody(config);

        return
            $"namespace {ns}.Tests;\n" +
            $"\n" +
            usings +
            $"\n" +
            $"public class SampleTests\n" +
            $"{{\n" +
            $"    {attribute}\n" +
            $"    public void Sample_ShouldPass()\n" +
            $"    {{\n" +
            $"        var result = 1 + 1;\n" +
            $"        {body}\n" +
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

        var usings = GetTestUsings(config);
        var (attribute, assertion) = GetIntegrationTestParts(config);
        var (lifecycleInterface, lifecycleMethods) = GetLifecycleParts(config);

        if (config.Database == DatabaseOption.PostgreSql)
        {
            return
                $"namespace {ns}.IntegrationTests;\n" +
                $"\n" +
                usings +
                $"using Microsoft.EntityFrameworkCore;\n" +
                $"using Testcontainers.PostgreSql;\n" +
                $"using {dbContextNamespace};\n" +
                $"\n" +
                $"public class DatabaseIntegrationTests{lifecycleInterface}\n" +
                $"{{\n" +
                $"    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(\"postgres:16-alpine\").Build();\n" +
                $"\n" +
                lifecycleMethods +
                $"\n" +
                $"    {attribute}\n" +
                $"    public async Task Database_ShouldConnect()\n" +
                $"    {{\n" +
                $"        var options = new DbContextOptionsBuilder<AppDbContext>()\n" +
                $"            .UseNpgsql(_container.GetConnectionString())\n" +
                $"            .Options;\n" +
                $"        await using var context = new AppDbContext(options);\n" +
                $"        var canConnect = await context.Database.CanConnectAsync();\n" +
                $"        {assertion}\n" +
                $"    }}\n" +
                $"}}\n";
        }
        else
        {
            return
                $"namespace {ns}.IntegrationTests;\n" +
                $"\n" +
                usings +
                $"using Microsoft.EntityFrameworkCore;\n" +
                $"using Testcontainers.MsSql;\n" +
                $"using {dbContextNamespace};\n" +
                $"\n" +
                $"public class DatabaseIntegrationTests{lifecycleInterface}\n" +
                $"{{\n" +
                $"    private readonly MsSqlContainer _container = new MsSqlBuilder(\"mcr.microsoft.com/mssql/server:2022-latest\").Build();\n" +
                $"\n" +
                lifecycleMethods +
                $"\n" +
                $"    {attribute}\n" +
                $"    public async Task Database_ShouldConnect()\n" +
                $"    {{\n" +
                $"        var options = new DbContextOptionsBuilder<AppDbContext>()\n" +
                $"            .UseSqlServer(_container.GetConnectionString())\n" +
                $"            .Options;\n" +
                $"        await using var context = new AppDbContext(options);\n" +
                $"        var canConnect = await context.Database.CanConnectAsync();\n" +
                $"        {assertion}\n" +
                $"    }}\n" +
                $"}}\n";
        }
    }

    private static string GetTestUsings(ProjectConfiguration config)
    {
        var usings = config.TestFramework switch
        {
            TestFrameworkOption.XUnit => "using Xunit;\n",
            TestFrameworkOption.NUnit => "using NUnit.Framework;\n",
            _ => ""
        };

        if (config.AssertLibrary == AssertLibraryOption.Shouldly)
            usings += "using Shouldly;\n";

        return usings;
    }

    private static (string Attribute, string Body) GetUnitTestBody(ProjectConfiguration config)
    {
        var attribute = config.TestFramework switch
        {
            TestFrameworkOption.XUnit => "[Fact]",
            TestFrameworkOption.NUnit => "[Test]",
            _ => "[Fact]"
        };

        var body = (config.TestFramework, config.AssertLibrary) switch
        {
            (_, AssertLibraryOption.Shouldly) => "result.ShouldBe(2);",
            (TestFrameworkOption.NUnit, AssertLibraryOption.None) => "Assert.That(result, Is.EqualTo(2));",
            _ => "Assert.Equal(2, result);"
        };

        return (attribute, body);
    }

    private static (string Attribute, string Assertion) GetIntegrationTestParts(ProjectConfiguration config)
    {
        var attribute = config.TestFramework switch
        {
            TestFrameworkOption.XUnit => "[Fact]",
            TestFrameworkOption.NUnit => "[Test]",
            _ => "[Fact]"
        };

        var assertion = (config.TestFramework, config.AssertLibrary) switch
        {
            (_, AssertLibraryOption.Shouldly) => "canConnect.ShouldBeTrue();",
            (TestFrameworkOption.NUnit, AssertLibraryOption.None) => "Assert.That(canConnect, Is.True);",
            _ => "Assert.True(canConnect);"
        };

        return (attribute, assertion);
    }

    private static (string Interface, string Methods) GetLifecycleParts(ProjectConfiguration config)
    {
        return config.TestFramework switch
        {
            TestFrameworkOption.NUnit => (
                "",
                "    [OneTimeSetUp]\n" +
                "    public async Task OneTimeSetUp() => await _container.StartAsync();\n" +
                "    [OneTimeTearDown]\n" +
                "    public async Task OneTimeTearDown() => await _container.DisposeAsync();\n"
            ),
            _ => (
                " : IAsyncLifetime",
                "    public async Task InitializeAsync() => await _container.StartAsync();\n" +
                "    public async Task DisposeAsync() => await _container.DisposeAsync();\n"
            )
        };
    }
}
