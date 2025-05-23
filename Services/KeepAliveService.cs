using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public class KeepAliveService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"🟢 KeepAlive tick at {DateTime.UtcNow}");
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
