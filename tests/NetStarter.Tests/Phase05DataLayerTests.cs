using NetStarter.Models;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Comprehensive tests covering all Phase 5 DATA and CACHE requirements.
/// </summary>
public class Phase05DataLayerTests
{
    // ---- DATA-01: Dapper ORM option exists ----

    [Fact]
    public void DATA_01_DapperOrmOptionExists()
    {
        var config = new ProjectConfiguration { Orm = OrmOption.Dapper };
        Assert.Equal(OrmOption.Dapper, config.Orm);
    }

    // ---- DATA-02: MySQL database option exists ----

    [Fact]
    public void DATA_02_MySqlDatabaseOptionExists()
    {
        var config = new ProjectConfiguration { Orm = OrmOption.EfCore, Database = DatabaseOption.MySql };
        Assert.Equal(DatabaseOption.MySql, config.Database);
    }

    // ---- DATA-03: SQLite database option exists ----

    [Fact]
    public void DATA_03_SqliteDatabaseOptionExists()
    {
        var config = new ProjectConfiguration { Orm = OrmOption.EfCore, Database = DatabaseOption.Sqlite };
        Assert.Equal(DatabaseOption.Sqlite, config.Database);
    }

    // ---- DATA-04: Dapper projects include correct ADO.NET driver ----

    [Theory]
    [InlineData(DatabaseOption.PostgreSql, "Npgsql")]
    [InlineData(DatabaseOption.MySql, "MySqlConnector")]
    [InlineData(DatabaseOption.Sqlite, "Microsoft.Data.Sqlite")]
    [InlineData(DatabaseOption.SqlServer, "Microsoft.Data.SqlClient")]
    public void DATA_04_DapperProjectIncludesCorrectDriver(DatabaseOption db, string expectedDriver)
    {
        var config = new ProjectConfiguration
        {
            Orm = OrmOption.Dapper,
            Database = db,
            SdkVersion = DotNetSdkVersion.Net10,
            Architecture = ArchitecturePattern.SimpleLayered,
        };
        var csproj = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Include=\"Dapper\"", csproj);
        Assert.Contains($"Include=\"{expectedDriver}\"", csproj);
    }

    // ---- DATA-05: EF Core + MySQL uses Pomelo with ServerVersion.AutoDetect ----

    [Fact]
    public void DATA_05_EfCoreMySqlUsesPomelo()
    {
        var config = new ProjectConfiguration
        {
            Orm = OrmOption.EfCore,
            Database = DatabaseOption.MySql,
            SdkVersion = DotNetSdkVersion.Net10,
            Architecture = ArchitecturePattern.SimpleLayered,
        };
        var csproj = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Pomelo.EntityFrameworkCore.MySql", csproj);
        Assert.Contains("Version=\"9.", csproj);

        var programCs = ProgramCsGenerator.Generate(config);
        Assert.Contains("UseMySql", programCs);
        Assert.Contains("ServerVersion.AutoDetect", programCs);
    }

    // ---- DATA-06: Docker-compose includes MySQL service ----

    [Fact]
    public void DATA_06_DockerComposeMySqlService()
    {
        var config = new ProjectConfiguration
        {
            Orm = OrmOption.EfCore,
            Database = DatabaseOption.MySql,
            IncludeDockerCompose = true,
        };
        var compose = DockerGenerator.GenerateDockerCompose(config);
        Assert.Contains("mysql:", compose);
        Assert.Contains("image: mysql:8.0", compose);
        Assert.Contains("3306:3306", compose);
    }

    // ---- DATA-07: Docker-compose suppresses db service for SQLite ----

    [Fact]
    public void DATA_07_DockerComposeSqliteNoDatabaseService()
    {
        var config = new ProjectConfiguration
        {
            Orm = OrmOption.EfCore,
            Database = DatabaseOption.Sqlite,
            IncludeDockerCompose = true,
        };
        var compose = DockerGenerator.GenerateDockerCompose(config);
        Assert.DoesNotContain("postgres:", compose);
        Assert.DoesNotContain("sqlserver:", compose);
        Assert.DoesNotContain("mysql:", compose);
    }

    // ---- CACHE-01: Redis bool property exists on config ----

    [Fact]
    public void CACHE_01_RedisPropertyExists()
    {
        var config = new ProjectConfiguration { IncludeRedis = true };
        Assert.True(config.IncludeRedis);
    }

    // ---- CACHE-02: Redis wires AddStackExchangeRedisCache with InstanceName ----

    [Fact]
    public void CACHE_02_RedisProgramCsWiring()
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            IncludeRedis = true,
        };
        var programCs = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddStackExchangeRedisCache", programCs);
        Assert.Contains("InstanceName = \"TestApp:\"", programCs);
        Assert.Contains("GetConnectionString(\"Redis\")", programCs);
    }

    // ---- CACHE-03: Docker-compose includes Redis service ----

    [Fact]
    public void CACHE_03_DockerComposeRedisService()
    {
        var config = new ProjectConfiguration
        {
            IncludeRedis = true,
            IncludeDockerCompose = true,
        };
        var compose = DockerGenerator.GenerateDockerCompose(config);
        Assert.Contains("redis:", compose);
        Assert.Contains("redis:7-alpine", compose);
        Assert.Contains("6379:6379", compose);
    }

    // ---- DapperGenerator unit tests ----

    [Fact]
    public void Dapper_PostgreSql_UsesNpgsqlDataSource()
    {
        var config = new ProjectConfiguration { Orm = OrmOption.Dapper, Database = DatabaseOption.PostgreSql };
        var output = DapperGenerator.Generate(config);
        Assert.Contains("NpgsqlDataSource.Create", output);
        Assert.Contains("AddDapperConnection", output);
        Assert.DoesNotContain("IDbConnection", output);
    }

    [Fact]
    public void Dapper_MySql_UsesMySqlDataSource()
    {
        var config = new ProjectConfiguration { Orm = OrmOption.Dapper, Database = DatabaseOption.MySql };
        var output = DapperGenerator.Generate(config);
        Assert.Contains("MySqlDataSource", output);
        Assert.Contains("AddDapperConnection", output);
    }

    [Fact]
    public void Dapper_Sqlite_UsesIDbConnection()
    {
        var config = new ProjectConfiguration { Orm = OrmOption.Dapper, Database = DatabaseOption.Sqlite };
        var output = DapperGenerator.Generate(config);
        Assert.Contains("IDbConnection", output);
        Assert.Contains("SqliteConnection", output);
    }

    [Fact]
    public void Dapper_SqlServer_UsesIDbConnection()
    {
        var config = new ProjectConfiguration { Orm = OrmOption.Dapper, Database = DatabaseOption.SqlServer };
        var output = DapperGenerator.Generate(config);
        Assert.Contains("IDbConnection", output);
        Assert.Contains("SqlConnection", output);
    }

    // ---- Dapper: Program.cs emits AddDapperConnection call ----

    [Fact]
    public void Dapper_ProgramCs_EmitsAddDapperConnectionCall()
    {
        var config = new ProjectConfiguration
        {
            Orm = OrmOption.Dapper,
            Database = DatabaseOption.PostgreSql,
            ProjectName = "MyApp",
            Namespace = "MyApp",
        };
        var programCs = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddDapperConnection(builder.Configuration)", programCs);
    }

    // ---- Orchestration: ProjectGenerationService includes DapperExtensions.cs ----

    [Theory]
    [InlineData(DatabaseOption.PostgreSql)]
    [InlineData(DatabaseOption.MySql)]
    [InlineData(DatabaseOption.Sqlite)]
    [InlineData(DatabaseOption.SqlServer)]
    public void ProjectGenerationService_DapperOrm_OutputContainsDapperExtensionsFile(DatabaseOption db)
    {
        var config = new ProjectConfiguration
        {
            Orm = OrmOption.Dapper,
            Database = db,
            ProjectName = "TestApp",
            Namespace = "TestApp",
        };
        var service = new ProjectGenerationService();
        var files = service.Generate(config);
        Assert.True(
            files.Keys.Any(k => k.Contains("DapperExtensions.cs")),
            $"Expected output files to contain a DapperExtensions.cs entry for Dapper + {db}. Keys: {string.Join(", ", files.Keys)}");
    }
}
