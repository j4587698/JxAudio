using System.ComponentModel;
using FreeSql;

namespace JxAudio.Core.Entity;

[Description("设置表")]
public class SettingsEntity: BaseEntity<SettingsEntity, int>
{
    [Description("设置组名称")]
    public string? GroupName { get; set; }

    [Description("设置名")]
    public string? SettingName { get; set; }

    [Description("设置值")]
    public string? SettingValue { get; set; }
}