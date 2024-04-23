using System.ComponentModel.DataAnnotations;

namespace JxAudio.Web.Vo;

public class LoginVo
{
    [Required]
    public string? UserName { get; set; }

    [Required]
    public string? Password { get; set; }
}