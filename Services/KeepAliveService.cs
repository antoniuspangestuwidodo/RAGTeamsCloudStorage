using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class KeepAliveService : BackgroundService
{
    private readonly ILogger<KeepAliveService> _logger;

    public KeepAliveService(ILogger<KeepAliveService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("🟢 KeepAlive tick at {time}", DateTime.Now);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("🔁 KeepAliveService was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Unhandled exception in KeepAliveService");
        }
    }
}

