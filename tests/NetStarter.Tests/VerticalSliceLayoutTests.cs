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
    public void VerticalSlice_EfCore_Entity_PlacedInFeaturesSample()
    {
        var config = CreateVerticalSliceConfig(orm: OrmOption.EfCore);
        config.Database = DatabaseOption.PostgreSql;
        var files = _generationService.Generate(config);

        var entityKey = files.Keys.FirstOrDefault(k => k.Contains("Features/Sample/SampleEntity.cs"));
        Assert.NotNull(entityKey);

        var content = files[entityKey];
        Assert.Contains("public class SampleEntity", content);
    }

    [Fact]
    public void VerticalSlice_EfCore_DbContext_ReferencesSampleEntity()
    {
        var config = CreateVerticalSliceConfig(orm: OrmOption.EfCore);
        config.Database = DatabaseOption.PostgreSql;
        var files = _generationService.Generate(config);

        var dbContextKey = files.Keys.First(k => k.Contains("AppDbContext.cs"));
        var content = files[dbContextKey];

        Assert.Contains("SampleEntity", content);
        Assert.Contains("Samples", content);
        Assert.Contains("using " + config.Namespace + ".Features.Sample;", content);
    }

    [Fact]
    public void VerticalSlice_Mapster_MappingConfig_PlacedInFeaturesSample()
    {
        var config = CreateVerticalSliceConfig(mapping: MappingOption.Mapster);
        var files = _generationService.Generate(config);

        var mappingKey = files.Keys.FirstOrDefault(k => k.Contains("SampleMappingConfig.cs"));
        Assert.NotNull(mappingKey);
        Assert.Contains("Features/Sample/SampleMappingConfig.cs", mappingKey);

        // Should NOT be in Mapping/
        Assert.DoesNotContain(files.Keys, k => k.Contains("Mapping/MappingConfig.cs"));

        var content = files[mappingKey];
        Assert.Contains("public class SampleMappingConfig", content);
    }

    [Fact]
    public void VerticalSlice_NoGitkeepInFeatures()
    {
        var config = CreateVerticalSliceConfig();
        var files = _generationService.Generate(config);

        Assert.DoesNotContain(files.Keys, k => k.Contains("Features/.gitkeep"));
    }

    // Regression guards — all architectures should use SampleEntity
    [Fact]
    public void SimpleLayered_EfCore_Entity_UsesSampleEntity()
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
    public void CleanArchitecture_EfCore_Entity_UsesSampleEntity()
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
    public void VerticalSlice_FluentValidation_GeneratesSampleRequestValidator()
    {
        var config = CreateVerticalSliceConfig();
        config.IncludeFluentValidation = true;
        var files = _generationService.Generate(config);

        var validatorKey = files.Keys.FirstOrDefault(k => k.Contains("SampleRequestValidator.cs"));
        Assert.NotNull(validatorKey);
        Assert.Contains("Features/Sample/SampleRequestValidator.cs", validatorKey);

        var content = files[validatorKey];
        Assert.Contains("AbstractValidator<SampleRequest>", content);
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
        var sample = features.Children.First(n => n.Name == "Sample");

        Assert.Contains(sample.Children, n => n.Name == "SampleRequestValidator.cs");
    }

    [Fact]
    public void VerticalSlice_NoFluentValidation_NoValidatorGenerated()
    {
        var config = CreateVerticalSliceConfig();
        config.IncludeFluentValidation = false;
        var files = _generationService.Generate(config);

        Assert.DoesNotContain(files.Keys, k => k.Contains("SampleRequestValidator.cs"));
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

        var validatorKey = files.Keys.FirstOrDefault(k => k.Contains("SampleRequestValidator.cs"));
        Assert.NotNull(validatorKey);
        Assert.Contains("Application/Validation/SampleRequestValidator.cs", validatorKey);

        var content = files[validatorKey];
        Assert.Contains("AbstractValidator<SampleRequest>", content);
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

        var validatorKey = files.Keys.FirstOrDefault(k => k.Contains("SampleRequestValidator.cs"));
        Assert.NotNull(validatorKey);
        Assert.Contains("Validation/SampleRequestValidator.cs", validatorKey);

        var content = files[validatorKey];
        Assert.Contains("AbstractValidator<SampleRequest>", content);
    }

    [Fact]
    public void VerticalSlice_FileTree_HasSampleFolderInFeatures()
    {
        var config = CreateVerticalSliceConfig(
            orm: OrmOption.EfCore,
            mapping: MappingOption.Mapster);
        config.Database = DatabaseOption.PostgreSql;

        var treeService = new FileTreeService();
        var tree = treeService.GenerateTree(config);

        // Navigate: root > src > ProjectName > Features > Sample
        var src = tree[0].Children.First(n => n.Name == "src");
        var proj = src.Children.First(n => n.Name == config.ProjectName);
        var features = proj.Children.First(n => n.Name == "Features");
        var sample = features.Children.FirstOrDefault(n => n.Name == "Sample");

        Assert.NotNull(sample);
        Assert.True(sample.IsFolder);
        Assert.Contains(sample.Children, n => n.Name == "SampleEntity.cs");
        Assert.Contains(sample.Children, n => n.Name == "SampleController.cs");
        Assert.Contains(sample.Children, n => n.Name == "SampleMappingConfig.cs");

        // Standalone Mapping/ folder should NOT exist
        Assert.DoesNotContain(proj.Children, n => n.Name == "Mapping");
    }
}
