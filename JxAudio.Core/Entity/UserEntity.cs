using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("用户表")]
public class UserEntity : BaseEntity<UserEntity, Guid>
{
    [Description("用户名")]
    [Required]
    public string? UserName { get; set; }

    [Description("密码")]
    public string? Password { get; set; }

    [Description("Email")]
    public string? Email { get; set; }

    [Description("允许最大bit")]
    public int MaxBitRate { get; set; }

    [Description("是否为游客")]
    public bool IsGuest { get; set; }

    [Description("是否为管理员")]
    public bool IsAdmin { get; set; }

    public bool CanJukebox { get; set; }

    [Navigate(ManyToMany = typeof(UserDirectoryEntity))]
    public ICollection<DirectoryEntity>? DirectoryEntities { get; set; }
}