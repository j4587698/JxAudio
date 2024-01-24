namespace JxAudio.Plugin;

/// <summary>
/// 文件信息
/// </summary>
public class FsInfo
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 文件全名（包含路径）
    /// </summary>
    public string FullName { get; set; } = "";

    /// <summary>
    /// 是否是目录
    /// </summary>
    public bool IsDir { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    public DateTime ModifyTime { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }
}