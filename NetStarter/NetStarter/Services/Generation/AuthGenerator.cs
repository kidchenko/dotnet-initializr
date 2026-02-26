using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class AuthGenerator
{
    public static string GenerateJwtSettings(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        return
            $"namespace {ns};\n" +
            $"\n" +
            $"public class JwtSettings\n" +
            $"{{\n" +
            $"    public string Issuer {{ get; set; }} = string.Empty;\n" +
            $"    public string Audience {{ get; set; }} = string.Empty;\n" +
            $"    public string Key {{ get; set; }} = string.Empty;\n" +
            $"}}\n";
    }

    public static string GetNamespaceSuffix(ArchitecturePattern architecture) => architecture switch
    {
        ArchitecturePattern.CleanArchitecture => "Api.Auth",
        _ => "Auth",
    };
}
