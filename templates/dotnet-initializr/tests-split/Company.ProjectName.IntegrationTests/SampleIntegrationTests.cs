using Xunit;
#if (IncludeFluentAssertions)
using FluentAssertions;
#endif
#if (IncludePostgreSql)
using Testcontainers.PostgreSql;
#elif (IncludeSqlServer)
using Testcontainers.MsSql;
#elif (IncludeMySql)
using Testcontainers.MySql;
#endif

namespace Company.ProjectName.IntegrationTests;

public class SampleIntegrationTests : IAsyncLifetime
{
#if (IncludePostgreSql)
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().Build();
#elif (IncludeSqlServer)
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();
#elif (IncludeMySql)
    private readonly MySqlContainer _dbContainer = new MySqlBuilder().Build();
#endif

    public async Task InitializeAsync()
    {
#if (IncludePostgreSql || IncludeSqlServer || IncludeMySql)
        await _dbContainer.StartAsync();
#else
        await Task.CompletedTask;
#endif
    }

    public async Task DisposeAsync()
    {
#if (IncludePostgreSql || IncludeSqlServer || IncludeMySql)
        await _dbContainer.DisposeAsync();
#else
        await Task.CompletedTask;
#endif
    }

    [Fact]
    public async Task Database_ShouldBeAccessible()
    {
#if (IncludePostgreSql || IncludeSqlServer || IncludeMySql)
        // The container is running — verify connection string is available
        var connectionString = _dbContainer.GetConnectionString();
#if (IncludeFluentAssertions)
        connectionString.Should().NotBeNullOrEmpty();
#else
        Assert.False(string.IsNullOrEmpty(connectionString));
#endif
#else
        // No database container configured — basic smoke test
        await Task.CompletedTask;
        Assert.True(true, "Integration test infrastructure is set up");
#endif
    }
}
