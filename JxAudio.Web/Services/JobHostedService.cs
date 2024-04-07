using JxAudio.Core.Service;
using JxAudio.Plugin;
using JxAudio.Web.Utils;
using JxAudio.Web.Vo;

namespace JxAudio.Web.Services;

public class JobHostedService(SettingsService settingsService): IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Util.IsInstalled)
        {
            var settingsVo = SettingsVo.GetSettings(settingsService);
            Util.StartJob(settingsVo);
            PluginUtil.LoadPluginOnStartup();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}