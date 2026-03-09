using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Company.ProjectName.Infrastructure.Jobs;

public sealed class SampleBackgroundService : BackgroundService
{
    private readonly ILogger<SampleBackgroundService> _logger;

    public SampleBackgroundService(ILogger<SampleBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("SampleBackgroundService running at: {Time}", DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
