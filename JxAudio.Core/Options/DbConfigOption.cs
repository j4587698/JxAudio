using System.ComponentModel.DataAnnotations;

namespace JxAudio.Core.Options;

public class DbConfigOption
{
    /// <summary>
    /// 数据库类型，目前支持sqlite,mysql,sqlserver,oracle,postgresql
    /// </summary>
    [Required]
    public string? DbType { get; set; }

    /// <summary>
    /// 数据库URL
    /// </summary>
    [Required]
    public string? DbUrl { get; set; }

    /// <summary>
    /// 数据库端口号
    /// </summary>
    [Range(1, 65535)]
    [Required]
    public string? DbPort { get; set; }

    /// <summary>
    /// 数据库名
    /// </summary>
    [Required]
    public string? DbName { get; set; }

    /// <summary>
    /// 数据库用户名
    /// </summary>
    [Required]
    public string? Username { get; set; }

    /// <summary>
    /// 数据库密码
    /// </summary>
    [Required]
    public string? Password { get; set; }

    /// <summary>
    /// 表前缀
    /// </summary>
    [Required]
    public string? Prefix { get; set; }
}