namespace JxAudio.Plugin;

/// <summary>
/// 文件上传类型
/// </summary>
public class UploadInfo(string fullPath, Stream fileStream)
{

    /// <summary>
    /// 全路径
    /// </summary>
    public string FullPath { get; set; } = fullPath;

    /// <summary>
    /// 文件流
    /// </summary>
    public Stream FileStream { get; set; } = fileStream;
}