using NetStarter.Models;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Comprehensive tests covering all Phase 6 AUTH, VAL, and TEST requirements.
/// </summary>
public class Phase06AuthValidationTests
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

    // ---- AUTH-01: ASP.NET Identity selectable ----

    [Fact]
    public void AUTH_01_AspNetIdentityOptionExists()
    {
        var config = CreateWebConfig(orm: OrmOption.EfCore, auth: AuthOption.AspNetIdentity);
        Assert.Equal(AuthOption.AspNetIdentity, config.Auth);
        var errors = config.Validate();
        Assert.Empty(errors);
    }

    // ---- AUTH-02: Identity requires EF Core (Validate enforcement) ----

    [Fact]
    public void AUTH_02_IdentityWithDapper_ReturnsValidationError()
    {
        var config = CreateWebConfig(orm: OrmOption.Dapper, auth: AuthOption.AspNetIdentity);
        var errors = config.Validate();
        Assert.Single(errors);
        Assert.Equal("IDENTITY_REQUIRES_EFCORE", errors[0].Code);
    }

    [Fact]
    public void AUTH_02_IdentityWithNoOrm_ReturnsValidationError()
    {
        var config = CreateWebConfig(orm: OrmOption.None, auth: AuthOption.AspNetIdentity);
        var errors = config.Validate();
        Assert.Single(errors);
        Assert.Equal("IDENTITY_REQUIRES_EFCORE", errors[0].Code);
    }

    [Fact]
    public void AUTH_02_IdentityWithEfCore_NoValidationError()
    {
        var config = CreateWebConfig(orm: OrmOption.EfCore, auth: AuthOption.AspNetIdentity);
        var errors = config.Validate();
        Assert.Empty(errors);
    }

    // ---- AUTH-03: API Key selectable ----

    [Fact]
    public void AUTH_03_ApiKeyAuth_ProgramCsContainsAddAuthentication()
    {
        var config = CreateWebConfig(auth: AuthOption.ApiKey);
        var programCs = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddAuthentication(\"ApiKey\")", programCs);
    }

    // ---- AUTH-04: API Key inline handler generated, no NuGet ----

    [Fact]
    public void AUTH_04_ApiKeyHandler_ContainsRequiredElements()
    {
        var config = CreateWebConfig(auth: AuthOption.ApiKey);
        var handler = AuthGenerator.GenerateApiKeyAuthHandler(config, AuthGenerator.GetNamespaceSuffix(config.Architecture));
        Assert.Contains("ApiKeyAuthenticationHandler", handler);
        Assert.Contains("HandleAuthenticateAsync", handler);
        Assert.Contains("X-Api-Key", handler);
    }

    [Fact]
    public void AUTH_04_ApiKeyAuth_CsprojDoesNotIncludeJwtOrIdentityPackage()
    {
        var config = CreateWebConfig(auth: AuthOption.ApiKey);
        var csproj = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("JwtBearer", csproj);
        Assert.DoesNotContain("Identity.EntityFrameworkCore", csproj);
    }

    // ---- AUTH-05: Keycloak selectable ----

    [Fact]
    public void AUTH_05_Keycloak_ProgramCsContainsAddJwtBearerAndAuthority()
    {
        var config = CreateWebConfig(auth: AuthOption.Keycloak);
        var programCs = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddJwtBearer", programCs);
        Assert.Contains("Keycloak:Authority", programCs);
    }

    // ---- AUTH-06: Keycloak appsettings and docker ----

    [Fact]
    public void AUTH_06_Keycloak_AppSettingsContainsRequiredKeys()
    {
        var config = CreateWebConfig(auth: AuthOption.Keycloak);
        var appSettings = AppSettingsGenerator.GenerateAppSettings(config);
        Assert.Contains("Keycloak", appSettings);
        Assert.Contains("Authority", appSettings);
        Assert.Contains("YOUR_REALM", appSettings);
        Assert.Contains("YOUR_CLIENT_ID", appSettings);
    }

    [Fact]
    public void AUTH_06_Keycloak_DockerComposeContainsKeycloakService()
    {
        var config = CreateWebConfig(auth: AuthOption.Keycloak);
        config.IncludeDockerCompose = true;
        var compose = DockerGenerator.GenerateDockerCompose(config);
        Assert.Contains("keycloak", compose);
        Assert.Contains("quay.io/keycloak", compose);
        Assert.Contains("start-dev", compose);
    }

    [Fact]
    public void AUTH_06_Keycloak_DockerComposeDisabled_NoDockerComposeFileGenerated()
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = DotNetSdkVersion.Net8,
            ProjectType = ProjectType.MinimalApi,
            Architecture = ArchitecturePattern.SimpleLayered,
            Orm = OrmOption.EfCore,
            Database = DatabaseOption.PostgreSql,
            Auth = AuthOption.Keycloak,
            IncludeDockerCompose = false,
        };
        var service = new ProjectGenerationService();
        var files = service.Generate(config);
        Assert.False(
            files.Keys.Any(k => k.EndsWith("docker-compose.yml")),
            "Expected no docker-compose.yml when IncludeDockerCompose=false");
    }

    // ---- VAL-01: FluentValidation selectable ----

    [Fact]
    public void VAL_01_FluentValidation_ProgramCsContainsAddValidators()
    {
        var config = CreateWebConfig();
        config.IncludeFluentValidation = true;
        var programCs = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddValidatorsFromAssemblyContaining", programCs);
    }

    // ---- VAL-02: FluentValidation 12 + DependencyInjectionExtensions packages ----

    [Fact]
    public void VAL_02_FluentValidation_CsprojContainsBothPackages()
    {
        var config = CreateWebConfig();
        config.IncludeFluentValidation = true;
        var csproj = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("FluentValidation", csproj);
        Assert.Contains("FluentValidation.DependencyInjectionExtensions", csproj);
    }

    [Fact]
    public void VAL_02_FluentValidation_CsprojDoesNotContainAspNetCorePackage()
    {
        var config = CreateWebConfig();
        config.IncludeFluentValidation = true;
        var csproj = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("FluentValidation.AspNetCore", csproj);
    }

    // ---- TEST-01: NSubstitute selectable ----

    [Fact]
    public void TEST_01_NSubstitute_TestProjectContainsNSubstitute()
    {
        var config = CreateWebConfig();
        config.IncludeNSubstitute = true;
        config.TestFramework = TestFrameworkOption.XUnit;
        var csproj = CsprojGenerator.GenerateTestProject(config, "../TestApp/TestApp.csproj");
        Assert.Contains("NSubstitute", csproj);
    }

    // ---- TEST-02: NSubstitute + Analyzers together ----

    [Fact]
    public void TEST_02_NSubstitute_TestProjectContainsBothNSubstituteAndAnalyzers()
    {
        var config = CreateWebConfig();
        config.IncludeNSubstitute = true;
        var csproj = CsprojGenerator.GenerateTestProject(config, "../TestApp/TestApp.csproj");
        Assert.Contains("Include=\"NSubstitute\"", csproj);
        Assert.Contains("NSubstitute.Analyzers.CSharp", csproj);
    }

    [Fact]
    public void TEST_02_NSubstituteDisabled_TestProjectContainsNeither()
    {
        var config = CreateWebConfig();
        config.IncludeNSubstitute = false;
        var csproj = CsprojGenerator.GenerateTestProject(config, "../TestApp/TestApp.csproj");
        Assert.DoesNotContain("NSubstitute", csproj);
        Assert.DoesNotContain("NSubstitute.Analyzers.CSharp", csproj);
    }

    // ---- TEST-03: Bogus selectable ----

    [Fact]
    public void TEST_03_Bogus_TestProjectContainsBogus()
    {
        var config = CreateWebConfig();
        config.IncludeBogus = true;
        config.TestFramework = TestFrameworkOption.XUnit;
        var csproj = CsprojGenerator.GenerateTestProject(config, "../TestApp/TestApp.csproj");
        Assert.Contains("Bogus", csproj);
    }

    // ---- TEST-04: Bogus package only, no sample Faker class ----

    [Fact]
    public void TEST_04_Bogus_TestProjectContainsBogusPackage()
    {
        var config = CreateWebConfig();
        config.IncludeBogus = true;
        var csproj = CsprojGenerator.GenerateTestProject(config, "../TestApp/TestApp.csproj");
        Assert.Contains("Bogus", csproj);
    }

    [Fact]
    public void TEST_04_Bogus_NoFakerClassGenerated()
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = DotNetSdkVersion.Net8,
            ProjectType = ProjectType.MinimalApi,
            Architecture = ArchitecturePattern.SimpleLayered,
            TestFramework = TestFrameworkOption.XUnit,
            IncludeBogus = true,
        };
        var service = new ProjectGenerationService();
        var files = service.Generate(config);
        Assert.False(
            files.Keys.Any(k => k.Contains("Faker")),
            $"Expected no Faker class file in generated output. Keys with 'Faker': {string.Join(", ", files.Keys.Where(k => k.Contains("Faker")))}");
    }

    // ---- Regression: JWT auth unchanged ----

    [Fact]
    public void Regression_JwtAuth_ProgramCsContainsTokenValidationParameters()
    {
        var config = CreateWebConfig(auth: AuthOption.Jwt);
        var programCs = ProgramCsGenerator.Generate(config);
        Assert.Contains("TokenValidationParameters", programCs);
        Assert.Contains("SymmetricSecurityKey", programCs);
        Assert.Contains("AddJwtBearer", programCs);
    }

    // ---- Regression: Auth=None generates no auth code ----

    [Fact]
    public void Regression_AuthNone_ProgramCsDoesNotContainUseAuthentication()
    {
        var config = CreateWebConfig(auth: AuthOption.None);
        var programCs = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("UseAuthentication", programCs);
        Assert.DoesNotContain("UseAuthorization", programCs);
    }
}
