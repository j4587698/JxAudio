using System.ComponentModel;
using FreeSql;

namespace JxAudio.Core.Entity;

[Description("图片表")]
public class PictureEntity: BaseEntity<PictureEntity, Guid>
{
    [Description("文件路径")]
    public string? Path { get; set; }

    [Description("Mime类型")]
    public string? MimeType { get; set; }
}