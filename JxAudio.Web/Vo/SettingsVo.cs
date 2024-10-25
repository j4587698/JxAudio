using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Jx.Toolbox.Extensions;
using JxAudio.Core.Attributes;
using JxAudio.Core.Service;
using JxAudio.Web.Enums;
using JxAudio.Web.Utils;

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

    public int JobThread { get; set; }

    public static SettingsVo GetSettings(SettingsService settingsService)
    {
        SettingsVo settingsVo = new()
        {
            SearchType = settingsService.GetValue(Constant.SearchTypeKey).ToEnum(SearchType.Interval),
            ScanInterval = settingsService.GetValue(Constant.ScanIntervalKey),
            CronExpress = settingsService.GetValue(Constant.CronExpressKey),
            TimeUnit = settingsService.GetValue(Constant.TimeUnitKey).ToEnum(TimeUnit.Second),
            JobThread = int.Parse(settingsService.GetValue(Constant.JobThreadKey) ?? $"{Environment.ProcessorCount}")
        };
        return settingsVo;
    }

    public void SetSettings(SettingsService settingsService)
    {
        settingsService.SetValue(Constant.SearchTypeKey, SearchType.ToString());
        settingsService.SetValue(Constant.ScanIntervalKey, ScanInterval ?? "");
        settingsService.SetValue(Constant.TimeUnitKey, TimeUnit.ToString());
        settingsService.SetValue(Constant.CronExpressKey, CronExpress ?? "");
        settingsService.SetValue(Constant.JobThreadKey, JobThread.ToString());
    }
}