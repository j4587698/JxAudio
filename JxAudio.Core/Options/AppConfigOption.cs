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
}