using NetStarter.Models;
using NetStarter.Services;
using NetStarter.Services.Generation;

namespace NetStarter.Tests;

public class VerticalSliceLayoutTests
{
    private readonly ProjectGenerationService _generationService = new();

    private static ProjectConfiguration CreateVerticalSliceConfig(
        OrmOption orm = OrmOption.None,
        MappingOption mapping = MappingOption.None,
        ProjectType projectType = ProjectType.WebApi) => new()
    {
        Architecture = ArchitecturePattern.VerticalSlice,
        Orm = orm,
        Mapping = mapping,
        ProjectType = projectType,
    };

    [Fact]
    public void VerticalSlice_EfCore_Entity_PlacedInFeaturesHello()
    {
        var config = CreateVerticalSliceConfig(orm: OrmOption.EfCore);
        config.Database = DatabaseOption.PostgreSql;
        var files = _generationService.Generate(config);

        // Entity should be in Features/Hello/
        var entityKey = files.Keys.FirstOrDefault(k => k.Contains("HelloEntity.cs"));
        Assert.NotNull(entityKey);
        Assert.Contains("Features/Hello/HelloEntity.cs", entityKey);

        // Should NOT be in Data/Entities/
        Assert.DoesNotContain(files.Keys, k => k.Contains("Data/Entities/SampleEntity.cs"));

        // Content should declare HelloEntity class
        var content = files[entityKey];
        Assert.Contains("public class HelloEntity", content);
    }

    [Fact]
    public void VerticalSlice_EfCore_DbContext_ReferencesHelloEntity()
    {
        var config = CreateVerticalSliceConfig(orm: OrmOption.EfCore);
        config.Database = DatabaseOption.PostgreSql;
        var files = _generationService.Generate(config);

        var dbContextKey = files.Keys.First(k => k.Contains("AppDbContext.cs"));
        var content = files[dbContextKey];

        Assert.Contains("HelloEntity", content);
        Assert.Contains("Hellos", content);
        Assert.Contains("using " + config.Namespace + ".Features.Hello;", content);
    }

    [Fact]
    public void VerticalSlice_Mapster_MappingConfig_PlacedInFeaturesHello()
    {
        var config = CreateVerticalSliceConfig(mapping: MappingOption.Mapster);
        var files = _generationService.Generate(config);

        var mappingKey = files.Keys.FirstOrDefault(k => k.Contains("HelloMappingConfig.cs"));
        Assert.NotNull(mappingKey);
        Assert.Contains("Features/Hello/HelloMappingConfig.cs", mappingKey);

        // Should NOT be in Mapping/
        Assert.DoesNotContain(files.Keys, k => k.Contains("Mapping/MappingConfig.cs"));

        // Content should declare HelloMappingConfig class
        var content = files[mappingKey];
        Assert.Contains("public class HelloMappingConfig", content);
    }

    [Fact]
    public void VerticalSlice_NoGitkeepInFeatures()
    {
        var config = CreateVerticalSliceConfig();
        var files = _generationService.Generate(config);

        Assert.DoesNotContain(files.Keys, k => k.Contains("Features/.gitkeep"));
    }

    // Regression guards — SimpleLayered and CleanArchitecture should still use SampleEntity
    [Fact]
    public void SimpleLayered_EfCore_Entity_StillUsesSampleEntity()
    {
        var config = new ProjectConfiguration
        {
            Architecture = ArchitecturePattern.SimpleLayered,
            Orm = OrmOption.EfCore,
            Database = DatabaseOption.PostgreSql,
        };
        var files = _generationService.Generate(config);

        var entityKey = files.Keys.FirstOrDefault(k => k.Contains("SampleEntity.cs"));
        Assert.NotNull(entityKey);
        Assert.Contains("Data/Entities/SampleEntity.cs", entityKey);

        var dbContextKey = files.Keys.First(k => k.Contains("AppDbContext.cs"));
        Assert.Contains("SampleEntity", files[dbContextKey]);
        Assert.Contains("Samples", files[dbContextKey]);
    }

    [Fact]
    public void CleanArchitecture_EfCore_Entity_StillUsesSampleEntity()
    {
        var config = new ProjectConfiguration
        {
            Architecture = ArchitecturePattern.CleanArchitecture,
            Orm = OrmOption.EfCore,
            Database = DatabaseOption.PostgreSql,
        };
        var files = _generationService.Generate(config);

        var entityKey = files.Keys.FirstOrDefault(k => k.Contains("SampleEntity.cs"));
        Assert.NotNull(entityKey);
        Assert.Contains("Domain/Entities/SampleEntity.cs", entityKey);

        var dbContextKey = files.Keys.First(k => k.Contains("AppDbContext.cs"));
        Assert.Contains("SampleEntity", files[dbContextKey]);
        Assert.Contains("Samples", files[dbContextKey]);
    }

    [Fact]
    public void VerticalSlice_FluentValidation_GeneratesHelloRequestValidator()
    {
        var config = CreateVerticalSliceConfig();
        config.IncludeFluentValidation = true;
        var files = _generationService.Generate(config);

        var validatorKey = files.Keys.FirstOrDefault(k => k.Contains("HelloRequestValidator.cs"));
        Assert.NotNull(validatorKey);
        Assert.Contains("Features/Hello/HelloRequestValidator.cs", validatorKey);

        var content = files[validatorKey];
        Assert.Contains("AbstractValidator<HelloRequest>", content);
        Assert.Contains("RuleFor(x => x.Name)", content);
    }

    [Fact]
    public void VerticalSlice_FluentValidation_FileTreeHasValidatorNode()
    {
        var config = CreateVerticalSliceConfig();
        config.IncludeFluentValidation = true;

        var treeService = new FileTreeService();
        var tree = treeService.GenerateTree(config);

        var src = tree[0].Children.First(n => n.Name == "src");
        var proj = src.Children.First(n => n.Name == config.ProjectName);
        var features = proj.Children.First(n => n.Name == "Features");
        var hello = features.Children.First(n => n.Name == "Hello");

        Assert.Contains(hello.Children, n => n.Name == "HelloRequestValidator.cs");
    }

    [Fact]
    public void VerticalSlice_NoFluentValidation_NoValidatorGenerated()
    {
        var config = CreateVerticalSliceConfig();
        config.IncludeFluentValidation = false;
        var files = _generationService.Generate(config);

        Assert.DoesNotContain(files.Keys, k => k.Contains("HelloRequestValidator.cs"));
    }

    [Fact]
    public void CleanArchitecture_FluentValidation_GeneratesValidatorInApplication()
    {
        var config = new ProjectConfiguration
        {
            Architecture = ArchitecturePattern.CleanArchitecture,
            ProjectType = ProjectType.WebApi,
            IncludeFluentValidation = true,
        };
        var files = _generationService.Generate(config);

        var validatorKey = files.Keys.FirstOrDefault(k => k.Contains("HelloRequestValidator.cs"));
        Assert.NotNull(validatorKey);
        Assert.Contains("Application/Validation/HelloRequestValidator.cs", validatorKey);

        var content = files[validatorKey];
        Assert.Contains("AbstractValidator<HelloRequest>", content);
    }

    [Fact]
    public void SimpleLayered_FluentValidation_GeneratesValidatorInValidationFolder()
    {
        var config = new ProjectConfiguration
        {
            Architecture = ArchitecturePattern.SimpleLayered,
            ProjectType = ProjectType.MinimalApi,
            IncludeFluentValidation = true,
        };
        var files = _generationService.Generate(config);

        var validatorKey = files.Keys.FirstOrDefault(k => k.Contains("HelloRequestValidator.cs"));
        Assert.NotNull(validatorKey);
        Assert.Contains("Validation/HelloRequestValidator.cs", validatorKey);

        var content = files[validatorKey];
        Assert.Contains("AbstractValidator<HelloRequest>", content);
    }

    [Fact]
    public void VerticalSlice_FileTree_HasHelloFolderInFeatures()
    {
        var config = CreateVerticalSliceConfig(
            orm: OrmOption.EfCore,
            mapping: MappingOption.Mapster);
        config.Database = DatabaseOption.PostgreSql;

        var treeService = new FileTreeService();
        var tree = treeService.GenerateTree(config);

        // Navigate: root > src > ProjectName > Features > Hello
        var src = tree[0].Children.First(n => n.Name == "src");
        var proj = src.Children.First(n => n.Name == config.ProjectName);
        var features = proj.Children.First(n => n.Name == "Features");
        var hello = features.Children.FirstOrDefault(n => n.Name == "Hello");

        Assert.NotNull(hello);
        Assert.True(hello.IsFolder);
        Assert.Contains(hello.Children, n => n.Name == "HelloEntity.cs");
        Assert.Contains(hello.Children, n => n.Name == "HelloController.cs");
        Assert.Contains(hello.Children, n => n.Name == "HelloMappingConfig.cs");

        // Standalone Mapping/ folder should NOT exist
        Assert.DoesNotContain(proj.Children, n => n.Name == "Mapping");
    }
}
