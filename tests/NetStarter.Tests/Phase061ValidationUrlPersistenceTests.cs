using NetStarter.Models;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Tests covering Phase 6.1 audit gap fixes:
/// FOUND-02 (Validate() enforcement), CACHE-01 (Redis config),
/// DATA-02/DATA-03 (Integration test EF Core guard).
/// </summary>
public class Phase061ValidationUrlPersistenceTests
{
    private static ProjectConfiguration CreateWebConfig(
        OrmOption orm = OrmOption.EfCore,
        DatabaseOption db = DatabaseOption.PostgreSql,
        AuthOption auth = AuthOption.None) => new()
    {
        ProjectName = "TestApp",
        Namespace = "TestApp",
        SdkVersion = DotNetSdkVersion.Net8,
        ProjectType = ProjectType.MinimalApi,
        Architecture = ArchitecturePattern.SimpleLayered,
        Orm = orm,
        Database = db,
        Auth = auth,
    };

    // ---- FOUND-02: Validate() enforcement ----

    [Fact]
    public void FOUND_02_Validate_IdentityWithDapper_ReturnsIdentityRequiresEfCoreError()
    {
        // Proves Validate() catches the invalid combo that previously bypassed the UI guard via URL params
        var config = CreateWebConfig(orm: OrmOption.Dapper, auth: AuthOption.AspNetIdentity);
        var errors = config.Validate();
        Assert.Single(errors);
        Assert.Equal("IDENTITY_REQUIRES_EFCORE", errors[0].Code);
    }

    [Fact]
    public void FOUND_02_Validate_IdentityWithEfCore_ReturnsNoErrors()
    {
        // Positive regression test: valid combo produces no errors
        var config = CreateWebConfig(orm: OrmOption.EfCore, auth: AuthOption.AspNetIdentity);
        var errors = config.Validate();
        Assert.Empty(errors);
    }

    // ---- DATA-02/DATA-03: Integration test EF Core guard ----

    [Fact]
    public void DATA_02_03_IntegrationTestProject_DapperOrm_DoesNotIncludeEfCore()
    {
        // Proves CsprojGenerator guard works: Dapper config must not include EF Core in integration test project
        var config = CreateWebConfig(orm: OrmOption.Dapper, db: DatabaseOption.MySql);
        var csproj = CsprojGenerator.GenerateIntegrationTestProject(config, "../TestApp/TestApp.csproj");
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", csproj);
    }

    [Fact]
    public void DATA_02_03_IntegrationTestProject_EfCoreOrm_IncludesEfCore()
    {
        // Positive regression test: EF Core config should include Microsoft.EntityFrameworkCore
        var config = CreateWebConfig(orm: OrmOption.EfCore, db: DatabaseOption.PostgreSql);
        var csproj = CsprojGenerator.GenerateIntegrationTestProject(config, "../TestApp/TestApp.csproj");
        Assert.Contains("Microsoft.EntityFrameworkCore", csproj);
    }

    // ---- CACHE-01: Redis config ----

    [Fact]
    public void CACHE_01_RedisConfig_GeneratesRedisPackage()
    {
        // Verifies the Redis feature works at the generator level
        var config = CreateWebConfig();
        config.IncludeRedis = true;
        var csproj = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.Extensions.Caching.StackExchangeRedis", csproj);
    }
}
