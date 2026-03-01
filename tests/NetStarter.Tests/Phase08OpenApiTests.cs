using NetStarter.Models;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

/// <summary>
/// Comprehensive tests covering all Phase 8 DOCS requirements.
/// OpenAPI documentation UI picker, NuGet packages, and Program.cs code generation
/// for Scalar, SwaggerUI (classic Net8 / sub-package Net9+), and Redoc across SDK versions.
/// </summary>
public class Phase08OpenApiTests
{
    private static ProjectConfiguration CreateWebConfig(
        DotNetSdkVersion sdk = DotNetSdkVersion.Net10,
        Action<ProjectConfiguration>? configure = null)
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = sdk,
            ProjectType = ProjectType.WebApi,
        };
        configure?.Invoke(config);
        return config;
    }

    private static ProjectConfiguration CreateConsoleConfig(
        DotNetSdkVersion sdk = DotNetSdkVersion.Net10,
        Action<ProjectConfiguration>? configure = null)
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = sdk,
            ProjectType = ProjectType.Console,
        };
        configure?.Invoke(config);
        return config;
    }

    private static ProjectConfiguration CreateWorkerConfig(
        DotNetSdkVersion sdk = DotNetSdkVersion.Net10,
        Action<ProjectConfiguration>? configure = null)
    {
        var config = new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = sdk,
            ProjectType = ProjectType.WorkerService,
        };
        configure?.Invoke(config);
        return config;
    }

    // ---- DOCS-01: UI Picker (OpenApiUi enum) ----

    [Fact] // DOCS-01: OpenApiUi enum has four expected values
    public void DOCS01_OpenApiUi_Enum_Has_Four_Options()
    {
        var values = Enum.GetValues<OpenApiUi>();
        Assert.Contains(OpenApiUi.None, values);
        Assert.Contains(OpenApiUi.Scalar, values);
        Assert.Contains(OpenApiUi.SwaggerUI, values);
        Assert.Contains(OpenApiUi.Redoc, values);
        Assert.Equal(4, values.Length);
    }

    [Fact] // DOCS-01: New ProjectConfiguration has ApiDocsUi defaulting to OpenApiUi.None
    public void DOCS01_OpenApiUi_Default_Is_None()
    {
        var config = new ProjectConfiguration();
        Assert.Equal(OpenApiUi.None, config.ApiDocsUi);
    }

    [Fact] // DOCS-01: Console project with Scalar produces no OpenAPI packages in csproj
    public void DOCS01_OpenApiUi_Hidden_For_Console()
    {
        var config = CreateConsoleConfig(configure: c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = CsprojGenerator.GenerateConsoleProject(config);
        Assert.DoesNotContain("Scalar.AspNetCore", result);
        Assert.DoesNotContain("Microsoft.AspNetCore.OpenApi", result);
    }

    [Fact] // DOCS-01: Worker project with Scalar produces no OpenAPI packages in csproj
    public void DOCS01_OpenApiUi_Hidden_For_Worker()
    {
        var config = CreateWorkerConfig(configure: c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = CsprojGenerator.GenerateWorkerProject(config);
        Assert.DoesNotContain("Scalar.AspNetCore", result);
        Assert.DoesNotContain("Microsoft.AspNetCore.OpenApi", result);
    }

    [Fact] // DOCS-01: Console with Scalar triggers OPENAPI_REQUIRES_WEB validation error
    public void DOCS01_OpenApiUi_Validation_OPENAPI_REQUIRES_WEB()
    {
        var config = CreateConsoleConfig(configure: c => c.ApiDocsUi = OpenApiUi.Scalar);
        var errors = config.Validate();
        Assert.Contains(errors, e => e.Code == "OPENAPI_REQUIRES_WEB");
    }

    [Fact] // DOCS-01: WebApi with Scalar does NOT trigger OPENAPI_REQUIRES_WEB validation error
    public void DOCS01_OpenApiUi_WebApi_NoValidationError()
    {
        var config = CreateWebConfig(configure: c => c.ApiDocsUi = OpenApiUi.Scalar);
        var errors = config.Validate();
        Assert.DoesNotContain(errors, e => e.Code == "OPENAPI_REQUIRES_WEB");
    }

    // ---- DOCS-02: Microsoft.AspNetCore.OpenApi + AddOpenApi + MapOpenApi ----

    [Fact] // DOCS-02: Scalar .NET 10 -> csproj contains Microsoft.AspNetCore.OpenApi
    public void DOCS02_Scalar_Net10_Has_OpenApi_Package()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.AspNetCore.OpenApi", result);
    }

    [Fact] // DOCS-02: Scalar .NET 10 -> Program.cs contains builder.Services.AddOpenApi()
    public void DOCS02_Scalar_Net10_Has_AddOpenApi()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddOpenApi()", result);
    }

    [Fact] // DOCS-02: Scalar .NET 10 -> Program.cs contains app.MapOpenApi()
    public void DOCS02_Scalar_Net10_Has_MapOpenApi()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("app.MapOpenApi()", result);
    }

    [Fact] // DOCS-02: SwaggerUI .NET 9 -> csproj contains Microsoft.AspNetCore.OpenApi
    public void DOCS02_SwaggerUI_Net9_Has_OpenApi_Package()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net9, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.AspNetCore.OpenApi", result);
    }

    [Fact] // DOCS-02: SwaggerUI .NET 8 does NOT contain Microsoft.AspNetCore.OpenApi (uses classic Swashbuckle)
    public void DOCS02_Net8_SwaggerUI_No_OpenApi_Package()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("Microsoft.AspNetCore.OpenApi", result);
    }

    // ---- DOCS-03: Scalar ----

    [Fact] // DOCS-03: Scalar .NET 10 -> csproj contains Scalar.AspNetCore
    public void DOCS03_Scalar_Net10_Has_Scalar_Package()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Scalar.AspNetCore", result);
    }

    [Fact] // DOCS-03: Scalar .NET 10 -> Program.cs contains app.MapScalarApiReference()
    public void DOCS03_Scalar_Net10_Has_MapScalarApiReference()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("MapScalarApiReference()", result);
    }

    [Fact] // DOCS-03: Scalar .NET 10 -> Program.cs contains using Scalar.AspNetCore
    public void DOCS03_Scalar_Net10_Has_Using_Scalar()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("using Scalar.AspNetCore;", result);
    }

    [Fact] // DOCS-03: Scalar .NET 8 -> same pattern as .NET 10 (AddOpenApi + MapOpenApi + MapScalarApiReference)
    public void DOCS03_Scalar_Net8_Same_As_Net10()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.Scalar);
        var csproj = CsprojGenerator.GenerateWebProject(config);
        var program = ProgramCsGenerator.Generate(config);

        Assert.Contains("Microsoft.AspNetCore.OpenApi", csproj);
        Assert.Contains("Scalar.AspNetCore", csproj);
        Assert.Contains("AddOpenApi()", program);
        Assert.Contains("MapOpenApi()", program);
        Assert.Contains("MapScalarApiReference()", program);
    }

    [Fact] // DOCS-03: Scalar .NET 9 -> contains Microsoft.AspNetCore.OpenApi and Scalar.AspNetCore
    public void DOCS03_Scalar_Net9_Has_Both_OpenApi_And_Scalar_Packages()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net9, c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.AspNetCore.OpenApi", result);
        Assert.Contains("Scalar.AspNetCore", result);
    }

    // ---- DOCS-04: SwaggerUI ----

    [Fact] // DOCS-04: SwaggerUI .NET 8 -> csproj contains full Swashbuckle.AspNetCore package
    public void DOCS04_SwaggerUI_Net8_Has_Full_Swashbuckle()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Swashbuckle.AspNetCore", result);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 8 -> Program.cs has AddSwaggerGen (classic pattern)
    public void DOCS04_SwaggerUI_Net8_Has_AddSwaggerGen()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddSwaggerGen()", result);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 8 -> Program.cs has UseSwagger (classic pattern)
    public void DOCS04_SwaggerUI_Net8_Has_UseSwagger()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("app.UseSwagger()", result);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 8 -> Program.cs has UseSwaggerUI (classic pattern)
    public void DOCS04_SwaggerUI_Net8_Has_UseSwaggerUI()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("app.UseSwaggerUI()", result);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 9 -> csproj contains Swashbuckle.AspNetCore.SwaggerUI sub-package (NOT full Swashbuckle.AspNetCore)
    public void DOCS04_SwaggerUI_Net9_Has_UI_Only_Package()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net9, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Swashbuckle.AspNetCore.SwaggerUI", result);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 9 -> csproj does NOT contain full Swashbuckle.AspNetCore (only UI sub-package)
    public void DOCS04_SwaggerUI_Net9_No_Full_Swashbuckle()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net9, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = CsprojGenerator.GenerateWebProject(config);
        // The full package line should not appear (the sub-package line does contain "Swashbuckle.AspNetCore" as a substring,
        // so we check specifically for the full package name WITHOUT a period following it)
        var lines = result.Split('\n');
        var fullSwashbuckleLine = lines.FirstOrDefault(l =>
            l.Contains("Swashbuckle.AspNetCore\"") || l.Contains("Swashbuckle.AspNetCore\" "));
        Assert.Null(fullSwashbuckleLine);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 9 -> csproj contains Microsoft.OpenApi pin
    public void DOCS04_SwaggerUI_Net9_Has_OpenApi_Pin()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net9, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.OpenApi", result);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 10 -> csproj contains Microsoft.OpenApi pin
    public void DOCS04_SwaggerUI_Net10_Has_OpenApi_Pin()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.OpenApi", result);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 9 -> Program.cs references /openapi/v1.json endpoint
    public void DOCS04_SwaggerUI_Net9_Points_To_OpenApi_Endpoint()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net9, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("/openapi/v1.json", result);
    }

    [Fact] // DOCS-04: SwaggerUI .NET 10 -> Program.cs references /openapi/v1.json endpoint
    public void DOCS04_SwaggerUI_Net10_Points_To_OpenApi_Endpoint()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("/openapi/v1.json", result);
    }

    // ---- DOCS-05: Redoc ----

    [Fact] // DOCS-05: Redoc .NET 10 -> csproj contains Swashbuckle.AspNetCore.ReDoc
    public void DOCS05_Redoc_Net10_Has_ReDoc_Package()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Redoc);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Swashbuckle.AspNetCore.ReDoc", result);
    }

    [Fact] // DOCS-05: Redoc .NET 10 -> Program.cs contains UseReDoc
    public void DOCS05_Redoc_Net10_Has_UseReDoc()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Redoc);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("UseReDoc", result);
    }

    [Fact] // DOCS-05: Redoc .NET 10 -> csproj contains Microsoft.OpenApi pin
    public void DOCS05_Redoc_Net10_Has_OpenApi_Pin()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Redoc);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.OpenApi", result);
    }

    [Fact] // DOCS-05: Redoc .NET 9 -> csproj contains Microsoft.OpenApi pin
    public void DOCS05_Redoc_Net9_Has_OpenApi_Pin()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net9, c => c.ApiDocsUi = OpenApiUi.Redoc);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.OpenApi", result);
    }

    [Fact] // DOCS-05: Redoc .NET 8 -> csproj does NOT contain Microsoft.OpenApi pin (only Net9/10 need pin)
    public void DOCS05_Redoc_Net8_No_OpenApi_Pin()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.Redoc);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("Microsoft.OpenApi", result);
    }

    [Fact] // DOCS-05: Redoc .NET 8 -> csproj still contains Swashbuckle.AspNetCore.ReDoc
    public void DOCS05_Redoc_Net8_Has_ReDoc_Package()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.Redoc);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Swashbuckle.AspNetCore.ReDoc", result);
    }

    [Fact] // DOCS-05: Redoc .NET 8 -> Program.cs contains UseReDoc
    public void DOCS05_Redoc_Net8_Has_UseReDoc()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.Redoc);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("UseReDoc", result);
    }

    [Fact] // DOCS-05: Redoc .NET 10 -> Program.cs contains /openapi/v1.json spec URL
    public void DOCS05_Redoc_Net10_Has_OpenApi_Spec_Url()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.Redoc);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("/openapi/v1.json", result);
    }

    // ---- DOCS-06: SDK Version Branching ----

    [Fact] // DOCS-06: SwaggerUI .NET 8 -> Program.cs does NOT contain AddOpenApi() (uses classic Swashbuckle)
    public void DOCS06_SwaggerUI_Net8_No_AddOpenApi()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("AddOpenApi()", result);
    }

    [Fact] // DOCS-06: SwaggerUI .NET 8 -> Program.cs emits AddSwaggerGen()
    public void DOCS06_SwaggerUI_Net8_Has_AddSwaggerGen()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net8, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddSwaggerGen()", result);
    }

    [Fact] // DOCS-06: SwaggerUI .NET 10 -> Program.cs does NOT contain AddSwaggerGen() (uses OpenAPI-first)
    public void DOCS06_SwaggerUI_Net10_No_AddSwaggerGen()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net10, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("AddSwaggerGen()", result);
    }

    [Fact] // DOCS-06: SwaggerUI .NET 9 -> Program.cs does NOT contain AddSwaggerGen() (uses OpenAPI-first)
    public void DOCS06_SwaggerUI_Net9_No_AddSwaggerGen()
    {
        var config = CreateWebConfig(DotNetSdkVersion.Net9, c => c.ApiDocsUi = OpenApiUi.SwaggerUI);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("AddSwaggerGen()", result);
    }

    [Theory] // DOCS-06: All non-None OpenApiUi + all SDK versions -> Program.cs contains IsDevelopment() guard
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net8)]
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net10)]
    [InlineData(OpenApiUi.SwaggerUI, DotNetSdkVersion.Net8)]
    [InlineData(OpenApiUi.SwaggerUI, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.SwaggerUI, DotNetSdkVersion.Net10)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net8)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net10)]
    public void DOCS06_All_Middleware_In_IsDevelopment_Guard(OpenApiUi ui, DotNetSdkVersion sdk)
    {
        var config = CreateWebConfig(sdk, c => c.ApiDocsUi = ui);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("IsDevelopment()", result);
    }

    [Fact] // DOCS-06: OpenApiUi.None -> no OpenAPI packages in csproj
    public void DOCS06_None_Produces_No_OpenApi_Packages()
    {
        var config = CreateWebConfig(configure: c => c.ApiDocsUi = OpenApiUi.None);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.DoesNotContain("Microsoft.AspNetCore.OpenApi", result);
        Assert.DoesNotContain("Scalar.AspNetCore", result);
        Assert.DoesNotContain("Swashbuckle", result);
        Assert.DoesNotContain("Microsoft.OpenApi", result);
    }

    [Fact] // DOCS-06: OpenApiUi.None -> no OpenAPI code in Program.cs
    public void DOCS06_None_Produces_No_OpenApi_Code()
    {
        var config = CreateWebConfig(configure: c => c.ApiDocsUi = OpenApiUi.None);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("AddOpenApi()", result);
        Assert.DoesNotContain("AddSwaggerGen()", result);
        Assert.DoesNotContain("MapOpenApi()", result);
        Assert.DoesNotContain("MapScalarApiReference()", result);
        Assert.DoesNotContain("UseSwagger", result);
        Assert.DoesNotContain("UseReDoc", result);
    }

    // ---- Negative: scalar/swagger using not emitted for non-web projects ----

    [Fact] // DOCS-01: Worker project with OpenApiUi.Scalar -> no OpenAPI code in Program.cs
    public void DOCS01_Worker_Scalar_No_OpenApiCode_In_Program()
    {
        var config = CreateWorkerConfig(configure: c => c.ApiDocsUi = OpenApiUi.Scalar);
        var result = ProgramCsGenerator.Generate(config);
        Assert.DoesNotContain("AddOpenApi()", result);
        Assert.DoesNotContain("MapScalarApiReference()", result);
        Assert.DoesNotContain("IsDevelopment()", result);
    }

    [Theory] // DOCS-02/03/04/05: Correct Microsoft.AspNetCore.OpenApi package emitted for all non-Net8-SwaggerUI combos
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net8)]
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net10)]
    [InlineData(OpenApiUi.SwaggerUI, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.SwaggerUI, DotNetSdkVersion.Net10)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net8)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net10)]
    public void DOCS02_AllNonNet8SwaggerUI_Has_OpenApi_Package(OpenApiUi ui, DotNetSdkVersion sdk)
    {
        var config = CreateWebConfig(sdk, c => c.ApiDocsUi = ui);
        var result = CsprojGenerator.GenerateWebProject(config);
        Assert.Contains("Microsoft.AspNetCore.OpenApi", result);
    }

    [Theory] // DOCS-02/03/04/05: All non-None combos emit AddOpenApi() except Net8 SwaggerUI
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net8)]
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.Scalar, DotNetSdkVersion.Net10)]
    [InlineData(OpenApiUi.SwaggerUI, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.SwaggerUI, DotNetSdkVersion.Net10)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net8)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net9)]
    [InlineData(OpenApiUi.Redoc, DotNetSdkVersion.Net10)]
    public void DOCS02_AllNonNet8SwaggerUI_Has_AddOpenApi_In_Program(OpenApiUi ui, DotNetSdkVersion sdk)
    {
        var config = CreateWebConfig(sdk, c => c.ApiDocsUi = ui);
        var result = ProgramCsGenerator.Generate(config);
        Assert.Contains("AddOpenApi()", result);
    }
}
