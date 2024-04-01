namespace JxAudio.Plugin;

/// <summary>
/// 插件设置类
/// </summary>
public class PluginConfig
{
    /// <summary>
    /// 插件Id，唯一
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 插件名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 插件作者
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 插件描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 插件版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 插件路径
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnable { get; set; }
}