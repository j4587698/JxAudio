using System.ComponentModel.DataAnnotations;
using JxAudio.Core.Entity;

namespace JxAudio.Web.Vo;

public class ResetPasswordVo
{
    public UserEntity? UserEntity { get; set; }
    
    [Required]
    public string? NewPassword { get; set; }

    [Required]
    public string? RePassword { get; set; }
}