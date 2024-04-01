using System.ComponentModel;
using FreeSql;

namespace JxAudio.Core.Entity;

[Description("插件列表")]
public class PluginEntity: BaseEntity<PluginEntity, int>
{
    [Description("插件Id")]
    public string? PluginId { get; set; }
    
    [Description("是否启用")]
    public bool IsEnable { get; set; }
}