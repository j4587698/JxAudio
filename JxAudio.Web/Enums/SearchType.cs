using System.ComponentModel;

namespace JxAudio.Web.Enums;

public enum SearchType
{
    [Description("时间间隔")]
    Interval,
    [Description("Cron表达式")]
    Cron
}