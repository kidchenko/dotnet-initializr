using Quartz;
using Microsoft.Extensions.Logging;

namespace Company.ProjectName.Jobs;

public class SampleJob : IJob
{
    private readonly ILogger<SampleJob> _logger;

    public SampleJob(ILogger<SampleJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("SampleJob executed at: {Time}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }
}
