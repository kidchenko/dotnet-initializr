using NetStarter.Models;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

public class CsprojPackageDeduplicationTests
{
    private static ProjectConfiguration CreateConfig(
        ArchitecturePattern architecture,
        OrmOption orm = OrmOption.EfCore,
        MappingOption mapping = MappingOption.None,
        DatabaseOption database = DatabaseOption.PostgreSql)
    {
        return new ProjectConfiguration
        {
            ProjectName = "TestApp",
            Namespace = "TestApp",
            SdkVersion = DotNetSdkVersion.Net9,
            ProjectType = ProjectType.WebApi,
            Architecture = architecture,
            Orm = orm,
            Database = database,
            Mapping = mapping,
        };
    }

    [Fact]
    public void CleanArchitecture_WebProject_DoesNotDuplicateEfCorePackages()
    {
        var config = CreateConfig(ArchitecturePattern.CleanArchitecture, orm: OrmOption.EfCore);
        var csproj = CsprojGenerator.GenerateWebProject(config);

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", csproj);
        Assert.DoesNotContain("Npgsql.EntityFrameworkCore.PostgreSQL", csproj);
    }

    [Fact]
    public void CleanArchitecture_WebProject_DoesNotDuplicateDapperPackages()
    {
        var config = CreateConfig(ArchitecturePattern.CleanArchitecture, orm: OrmOption.Dapper);
        var csproj = CsprojGenerator.GenerateWebProject(config);

        Assert.DoesNotContain("Dapper", csproj);
        Assert.DoesNotContain("Npgsql", csproj);
    }

    [Fact]
    public void CleanArchitecture_WebProject_DoesNotDuplicateMapsterPackages()
    {
        var config = CreateConfig(ArchitecturePattern.CleanArchitecture, mapping: MappingOption.Mapster);
        var csproj = CsprojGenerator.GenerateWebProject(config);

        Assert.DoesNotContain("Mapster", csproj);
    }

    [Fact]
    public void CleanArchitecture_Infrastructure_HasEfCorePackages()
    {
        var config = CreateConfig(ArchitecturePattern.CleanArchitecture, orm: OrmOption.EfCore);
        var csproj = CsprojGenerator.GenerateClassLibrary(config, "Infrastructure");

        Assert.Contains("Microsoft.EntityFrameworkCore", csproj);
        Assert.Contains("Npgsql.EntityFrameworkCore.PostgreSQL", csproj);
    }

    [Fact]
    public void CleanArchitecture_Application_HasMapsterPackages()
    {
        var config = CreateConfig(ArchitecturePattern.CleanArchitecture, mapping: MappingOption.Mapster);
        var csproj = CsprojGenerator.GenerateClassLibrary(config, "Application");

        Assert.Contains("Mapster", csproj);
        Assert.Contains("Mapster.DependencyInjection", csproj);
    }

    [Fact]
    public void SimpleLayered_WebProject_HasEfCorePackages()
    {
        var config = CreateConfig(ArchitecturePattern.SimpleLayered, orm: OrmOption.EfCore);
        var csproj = CsprojGenerator.GenerateWebProject(config);

        Assert.Contains("Microsoft.EntityFrameworkCore", csproj);
        Assert.Contains("Npgsql.EntityFrameworkCore.PostgreSQL", csproj);
    }

    [Fact]
    public void VerticalSlice_WebProject_HasMapsterPackages()
    {
        var config = CreateConfig(ArchitecturePattern.VerticalSlice, mapping: MappingOption.Mapster);
        var csproj = CsprojGenerator.GenerateWebProject(config);

        Assert.Contains("Mapster", csproj);
        Assert.Contains("Mapster.DependencyInjection", csproj);
    }
}
