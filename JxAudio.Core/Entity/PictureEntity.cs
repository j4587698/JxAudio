using System.ComponentModel;
using FreeSql;

namespace JxAudio.Core.Entity;

[Description("图片表")]
public class PictureEntity: BaseEntity<PictureEntity, int>
{
    [Description("文件路径")]
    public string? Path { get; set; }

    [Description("高度")]
    public int Height { get; set; }

    [Description("宽度")]
    public int Width { get; set; }

    [Description("Mime类型")]
    public string? MimeType { get; set; }
}