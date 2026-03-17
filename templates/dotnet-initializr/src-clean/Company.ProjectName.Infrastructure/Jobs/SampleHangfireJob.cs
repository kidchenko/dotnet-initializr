using Microsoft.Extensions.Logging;

namespace Company.ProjectName.Infrastructure.Jobs;

public class SampleHangfireJob(ILogger<SampleHangfireJob> logger)
{
    public void Execute()
    {
        logger.LogInformation("SampleHangfireJob executed at {Time}", DateTimeOffset.UtcNow);
    }
}
