using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AListProviderPlugin;

public class AccountVo
{
    [Required(ErrorMessage = "服务器地址不能为空")]
    [Description("服务器地址")]
    public string? ServerUrl { get; set; }
    
    [Required(ErrorMessage = "用户名不能为空")]
    [Description("用户名")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "密码不能为空")]
    [Description("密码")]
    public string? Password { get; set; }
    
}