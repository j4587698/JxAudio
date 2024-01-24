using System.ComponentModel;
using FreeSql;

namespace JxAudio.Core.Entity;

[Description("目录信息表")]
public class DirectoryEntity : BaseEntity<DirectoryEntity, long>
{
    [Description("提供器Id")]
    public Guid Provider { get; set; }

    [Description("目录路径")]
    public string? Path { get; set; }
}