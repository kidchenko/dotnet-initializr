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

    public static string GenerateApiKeyAuthHandler(ProjectConfiguration config, string namespaceSuffix)
    {
        var ns = $"{config.Namespace}.{namespaceSuffix}";
        return
            $"using System.Security.Claims;\n" +
            $"using System.Text.Encodings.Web;\n" +
            $"using Microsoft.AspNetCore.Authentication;\n" +
            $"using Microsoft.Extensions.Options;\n" +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public class ApiKeyAuthenticationHandler(\n" +
            $"    IOptionsMonitor<AuthenticationSchemeOptions> options,\n" +
            $"    ILoggerFactory logger,\n" +
            $"    UrlEncoder encoder,\n" +
            $"    IConfiguration configuration)\n" +
            $"    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)\n" +
            $"{{\n" +
            $"    protected override Task<AuthenticateResult> HandleAuthenticateAsync()\n" +
            $"    {{\n" +
            $"        if (!Request.Headers.TryGetValue(\"X-Api-Key\", out var apiKeyHeader))\n" +
            $"            return Task.FromResult(AuthenticateResult.Fail(\"API Key header missing\"));\n" +
            $"\n" +
            $"        var apiKey = apiKeyHeader.ToString();\n" +
            $"        var expectedKey = configuration[\"ApiKey:Value\"];\n" +
            $"\n" +
            $"        if (apiKey != expectedKey)\n" +
            $"            return Task.FromResult(AuthenticateResult.Fail(\"Invalid API Key\"));\n" +
            $"\n" +
            $"        var claims = new[] {{ new Claim(ClaimTypes.Name, \"ApiKeyUser\") }};\n" +
            $"        var identity = new ClaimsIdentity(claims, Scheme.Name);\n" +
            $"        var principal = new ClaimsPrincipal(identity);\n" +
            $"        var ticket = new AuthenticationTicket(principal, Scheme.Name);\n" +
            $"\n" +
            $"        return Task.FromResult(AuthenticateResult.Success(ticket));\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GetNamespaceSuffix(ArchitecturePattern architecture) => architecture switch
    {
        ArchitecturePattern.CleanArchitecture => "Api.Auth",
        _ => "Auth",
    };
}
