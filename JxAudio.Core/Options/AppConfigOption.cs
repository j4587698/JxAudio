namespace JxAudio.Core.Options;

/// <summary>
/// 配置参数
/// </summary>
public class AppConfigOption
{
    /// <summary>
    /// 扫描目录，多个目录用,分隔
    /// </summary>
    public List<string>? ConfigSearchFolder { get; set; }

    /// <summary>
    /// 是否启用Xml配置文件，默认为false
    /// </summary>
    public bool EnableXmlSearcher { get; set; }

    /// <summary>
    /// 是否在有接口的情况下自动注册自身，默认为false
    /// </summary>
    public bool RegisterSelfIfHasInterface { get; set; }

    public bool EnableController { get; set; }

    /// <summary>
    /// 动态前缀
    /// </summary>
    public string? DynamicPrefix { get; set; }

    /// <summary>
    /// 是否自动移除动态前缀，默认为true
    /// </summary>
    public bool AutoRemoveDynamicPrefix { get; set; } = true;

    /// <summary>
    /// Get方法前缀
    /// </summary>
    public List<string> GetPrefix { get; set; } =
    [
        "Get"
    ];

    /// <summary>
    /// Post方法前缀
    /// </summary>
    public List<string> PostPrefix { get; set; } =
    [
        "Post",
        "Send"
    ];
    
    /// <summary>
    /// Put方法前缀
    /// </summary>
    public List<string> PutPrefix { get; set; } =
    [
        "Put"
    ];
    
    /// <summary>
    /// Delete方法前缀
    /// </summary>
    public List<string> DeletePrefix { get; set; } =
    [
        "Delete"
    ];
}