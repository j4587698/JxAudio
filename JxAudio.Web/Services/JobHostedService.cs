using Jx.Toolbox.Extensions;
using JxAudio.Core.Service;
using JxAudio.Web.Enums;
using JxAudio.Web.Jobs;
using JxAudio.Web.Utils;
using Longbow.Tasks;

namespace JxAudio.Web.Services;

public class JobHostedService(SettingsService settingsService): IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Util.IsInstalled)
        {
            var type = settingsService.GetValue(Constant.SearchTypeKey).ToEnum(SearchType.Interval);
            if (type == SearchType.Interval && int.TryParse(settingsService.GetValue(Constant.ScanIntervalKey), out var scanInterval))
            {
                var timeUnit = settingsService.GetValue(Constant.TimeUnitKey).ToEnum(TimeUnit.Hour);
                switch (timeUnit)
                {
                    case TimeUnit.Second:
                        break;
                    case TimeUnit.Minute:
                        scanInterval *= 60;
                        break;
                    case TimeUnit.Hour:
                        scanInterval *= 60 * 60;
                        break;
                    case TimeUnit.Day:
                        scanInterval *= 60 * 60 * 24;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                TaskServicesManager.GetOrAdd<ScanJob>(TriggerBuilder.Default.WithInterval(scanInterval * 1000).Build());
                return Task.CompletedTask;
            }
            if (type == SearchType.Cron)
            {
                var cron = settingsService.GetValue(Constant.CronExpressKey);
                if (!cron.IsNullOrEmpty())
                {
                    TaskServicesManager.GetOrAdd<ScanJob>(TriggerBuilder.Build(cron!));
                    return Task.CompletedTask;
                }
            }

            throw new Exception("Create Job Error");
        }
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}