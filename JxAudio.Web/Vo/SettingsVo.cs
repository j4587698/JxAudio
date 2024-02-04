using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Jx.Toolbox.Extensions;
using JxAudio.Core.Attributes;
using JxAudio.Core.Service;
using JxAudio.Web.Enums;

namespace JxAudio.Web.Vo;

public class SettingsVo
{
    [Description("扫描方式")]
    public SearchType SearchType { get; set; }
    
    [Description("扫描间隔")]
    [Required]
    [Int]
    public string? ScanInterval { get; set; }

    [Cron]
    [Required]
    public string? CronExpress { get; set; }

    public TimeUnit TimeUnit { get; set; }

    public static SettingsVo GetSettings(SettingsService settingsService)
    {
        SettingsVo settingsVo = new()
        {
            SearchType = settingsService.GetValue(nameof(SearchType)).ToEnum(SearchType.Interval),
            ScanInterval = settingsService.GetValue(nameof(ScanInterval)),
            CronExpress = settingsService.GetValue(nameof(CronExpress)),
            TimeUnit = settingsService.GetValue(nameof(TimeUnit)).ToEnum(TimeUnit.Second)
        };
        return settingsVo;
    }

    public void SetSettings(SettingsService settingsService)
    {
        settingsService.SetValue(nameof(SearchType), SearchType.ToString());
        settingsService.SetValue(nameof(ScanInterval), ScanInterval ?? "");
        settingsService.SetValue(nameof(TimeUnit), TimeUnit.ToString());
        settingsService.SetValue(nameof(CronExpress), CronExpress ?? "");
    }
}