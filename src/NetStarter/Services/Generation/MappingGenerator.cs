using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class MappingGenerator
{
    public static string GenerateMappingConfig(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        return
            $"namespace {ns};\n" +
            $"\n" +
            $"using Mapster;\n" +
            $"\n" +
            $"public class MappingConfig : IRegister\n" +
            $"{{\n" +
            $"    public void Register(TypeAdapterConfig config)\n" +
            $"    {{\n" +
            $"        // Configure type mappings here\n" +
            $"        // Example: config.NewConfig<SourceType, DestinationType>();\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GenerateHelloMappingConfig(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        return
            $"namespace {ns};\n" +
            $"\n" +
            $"using Mapster;\n" +
            $"\n" +
            $"public class HelloMappingConfig : IRegister\n" +
            $"{{\n" +
            $"    public void Register(TypeAdapterConfig config)\n" +
            $"    {{\n" +
            $"        // Configure type mappings here\n" +
            $"        // Example: config.NewConfig<SourceType, DestinationType>();\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GetNamespaceSuffix(ArchitecturePattern architecture) => architecture switch
    {
        ArchitecturePattern.CleanArchitecture => "Application.Mapping",
        ArchitecturePattern.VerticalSlice => "Features.Hello",
        _ => "Mapping",
    };
}
