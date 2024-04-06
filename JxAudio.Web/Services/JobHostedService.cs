using Jx.Toolbox.Extensions;
using JxAudio.Core.Service;
using JxAudio.Web.Enums;
using JxAudio.Web.Jobs;
using JxAudio.Web.Utils;
using JxAudio.Web.Vo;
using Longbow.Tasks;

namespace JxAudio.Web.Services;

public class JobHostedService(SettingsService settingsService): IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Util.IsInstalled)
        {
            var settingsVo = SettingsVo.GetSettings(settingsService);
            Util.StartJob(settingsVo);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}