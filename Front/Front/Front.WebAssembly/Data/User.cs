using System.ComponentModel;

namespace Front.WebAssembly.Data;

public class User
{
    [Description("用户名")]
    public string? UserName { get; set; }

    [Description("头像")]
    public string? Avatar { get; set; }

    [Description("允许最大bit")]
    public int MaxBitRate { get; set; }

    [Description("是否为游客")]
    public bool IsGuest { get; set; }

    [Description("是否为管理员")]
    public bool IsAdmin { get; set; }
}
