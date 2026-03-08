using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class EfCoreGenerator
{
    public static string GenerateDbContext(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        var entityNsSuffix = GetEntityNamespaceSuffix(config.Architecture);
        var entityNs = $"{config.Namespace}.{entityNsSuffix}";
        var entityClass = GetEntityClassName(config.Architecture);
        var dbSetName = GetDbSetPropertyName(config.Architecture);

        var usings = $"using Microsoft.EntityFrameworkCore;\n";
        if (entityNs != ns)
            usings += $"using {entityNs};\n";

        return
            usings +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)\n" +
            $"{{\n" +
            $"    public DbSet<{entityClass}> {dbSetName} => Set<{entityClass}>();\n" +
            $"}}\n";
    }

    public static string GenerateIdentityDbContext(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        var entityNsSuffix = GetEntityNamespaceSuffix(config.Architecture);
        var entityNs = $"{config.Namespace}.{entityNsSuffix}";
        var entityClass = GetEntityClassName(config.Architecture);
        var dbSetName = GetDbSetPropertyName(config.Architecture);

        var usings =
            $"using Microsoft.AspNetCore.Identity;\n" +
            $"using Microsoft.AspNetCore.Identity.EntityFrameworkCore;\n" +
            $"using Microsoft.EntityFrameworkCore;\n";
        if (entityNs != ns)
            usings += $"using {entityNs};\n";

        return
            usings +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public class AppDbContext(DbContextOptions<AppDbContext> options)\n" +
            $"    : IdentityDbContext<IdentityUser>(options)\n" +
            $"{{\n" +
            $"    public DbSet<{entityClass}> {dbSetName} => Set<{entityClass}>();\n" +
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

    public static string GenerateHelloEntity(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        return
            $"namespace {ns};\n" +
            $"\n" +
            $"public class HelloEntity\n" +
            $"{{\n" +
            $"    public int Id {{ get; set; }}\n" +
            $"    public string Name {{ get; set; }} = string.Empty;\n" +
            $"    public DateTime CreatedAt {{ get; set; }} = DateTime.UtcNow;\n" +
            $"}}\n";
    }

    public static string GetEntityClassName(ArchitecturePattern architecture) => architecture switch
    {
        ArchitecturePattern.VerticalSlice => "HelloEntity",
        _ => "SampleEntity",
    };

    public static string GetDbSetPropertyName(ArchitecturePattern architecture) => architecture switch
    {
        ArchitecturePattern.VerticalSlice => "Hellos",
        _ => "Samples",
    };

    public static string GetEntityNamespaceSuffix(ArchitecturePattern architecture) => architecture switch
    {
        ArchitecturePattern.CleanArchitecture => "Domain.Entities",
        ArchitecturePattern.VerticalSlice => "Features.Hello",
        _ => "Data",
    };
}
