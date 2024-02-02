using System.ComponentModel;
using JxAudio.Web.Enums;

namespace JxAudio.Web.Vo;

public class SettingsVo
{
    [Description("扫描方式")]
    public SearchType SearchType { get; set; }
    
    [Description("扫描间隔")]
    public string? ScanInterval { get; set; }

    public TimeUnit TimeUnit { get; set; }
}