using Microsoft.Extensions.Logging;

namespace Company.ProjectName.Infrastructure.Jobs;

public class SampleJob
{
    private readonly ILogger<SampleJob> _logger;

    public SampleJob(ILogger<SampleJob> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SampleJob executed at: {Time}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }
}
