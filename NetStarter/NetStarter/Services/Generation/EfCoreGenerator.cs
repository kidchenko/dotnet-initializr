using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class EfCoreGenerator
{
    public static string GenerateDbContext(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        return
            $"using Microsoft.EntityFrameworkCore;\n" +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)\n" +
            $"{{\n" +
            $"    public DbSet<SampleEntity> Samples => Set<SampleEntity>();\n" +
            $"}}\n";
    }

    public static string GenerateSampleEntity(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        return
            $"namespace {ns};\n" +
            $"\n" +
            $"public class SampleEntity\n" +
            $"{{\n" +
            $"    public int Id {{ get; set; }}\n" +
            $"    public string Name {{ get; set; }} = string.Empty;\n" +
            $"    public DateTime CreatedAt {{ get; set; }} = DateTime.UtcNow;\n" +
            $"}}\n";
    }

    public static string GetDbContextNamespaceSuffix(ArchitecturePattern architecture) => architecture switch
    {
        ArchitecturePattern.CleanArchitecture => "Infrastructure.Data",
        _ => "Data",
    };

    public static string GetEntityNamespaceSuffix(ArchitecturePattern architecture) => architecture switch
    {
        ArchitecturePattern.CleanArchitecture => "Domain.Entities",
        ArchitecturePattern.VerticalSlice => "Data.Entities",
        _ => "Data",
    };
}
