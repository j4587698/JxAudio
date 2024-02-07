using JxAudio.Web.Jobs;
using Longbow.Tasks;

namespace JxAudio.Web.Services;

public class JobHostedService: IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        TaskServicesManager.GetOrAdd<ScanJob>(TriggerBuilder.Default.Build());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}