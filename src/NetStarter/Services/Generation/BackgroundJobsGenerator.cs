using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class BackgroundJobsGenerator
{
    public static string GenerateSampleBackgroundService(ProjectConfiguration config)
    {
        var ns = GetJobsNamespace(config);
        return
            $"namespace {ns};\n" +
            $"\n" +
            $"public class SampleBackgroundService(ILogger<SampleBackgroundService> logger) : BackgroundService\n" +
            $"{{\n" +
            $"    protected override async Task ExecuteAsync(CancellationToken stoppingToken)\n" +
            $"    {{\n" +
            $"        logger.LogInformation(\"SampleBackgroundService running at: {{Time}}\", DateTimeOffset.UtcNow);\n" +
            $"        await Task.CompletedTask;\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GenerateSampleHangfireJob(ProjectConfiguration config)
    {
        var ns = GetJobsNamespace(config);
        return
            $"namespace {ns};\n" +
            $"\n" +
            $"public class SampleHangfireJob(ILogger<SampleHangfireJob> logger)\n" +
            $"{{\n" +
            $"    public void Execute()\n" +
            $"    {{\n" +
            $"        logger.LogInformation(\"SampleHangfireJob executed at {{Time}}\", DateTimeOffset.UtcNow);\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GenerateSampleQuartzJob(ProjectConfiguration config)
    {
        var ns = GetJobsNamespace(config);
        return
            $"using Quartz;\n" +
            $"\n" +
            $"namespace {ns};\n" +
            $"\n" +
            $"public class SampleQuartzJob(ILogger<SampleQuartzJob> logger) : IJob\n" +
            $"{{\n" +
            $"    public Task Execute(IJobExecutionContext context)\n" +
            $"    {{\n" +
            $"        logger.LogInformation(\"SampleQuartzJob executed at {{Time}}\", DateTimeOffset.UtcNow);\n" +
            $"        return Task.CompletedTask;\n" +
            $"    }}\n" +
            $"}}\n";
    }

    public static string GetFilePath(ProjectConfiguration config)
    {
        var sampleClassName = config.BackgroundJobs switch
        {
            BackgroundJobsOption.IHostedService => "SampleBackgroundService",
            BackgroundJobsOption.Hangfire       => "SampleHangfireJob",
            BackgroundJobsOption.Quartz         => "SampleQuartzJob",
            _                                   => "SampleJob",
        };

        return config.Architecture == ArchitecturePattern.CleanArchitecture
            ? $"src/{config.EntryPointProjectName}/Jobs/{sampleClassName}.cs"
            : $"src/{config.ProjectName}/Jobs/{sampleClassName}.cs";
    }

    private static string GetJobsNamespace(ProjectConfiguration config)
    {
        return config.Architecture == ArchitecturePattern.CleanArchitecture
            ? $"{config.Namespace}.{config.EntryPointSuffix}.Jobs"
            : $"{config.Namespace}.Jobs";
    }
}
