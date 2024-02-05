using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FreeSql;
using JxAudio.Core.Attributes;

namespace JxAudio.Core.Entity;

[Description("目录信息表")]
public class DirectoryEntity : BaseEntity<DirectoryEntity, long>
{
    [Description("提供器Id")]
    [GuidRequired]
    public Guid Provider { get; set; }

    [Description("目录路径")]
    [Required]
    [NotNull]
    public string? Path { get; set; }

    [Description("目录别名")]
    [Required]
    public string? Name { get; set; }
}