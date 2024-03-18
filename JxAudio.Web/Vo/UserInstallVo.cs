using System.ComponentModel.DataAnnotations;

namespace JxAudio.Web.Vo;

/// <summary>
/// 安装信息
/// </summary>
public class UserInstallVo
{
    /// <summary>
    /// 管理员用户名
    /// </summary>
    [Required]
    public string? UserName { get; set; }

    /// <summary>
    /// 管理员密码
    /// </summary>
    [Required]
    public string? Password { get; set; }

    /// <summary>
    /// 管理员密码重复
    /// </summary>
    [Required]
    public string? RePassword { get; set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// 最大Bit数
    /// </summary>
    public int MaxBitRate { get; set; }
    
}