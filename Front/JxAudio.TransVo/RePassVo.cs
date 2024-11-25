using System.ComponentModel.DataAnnotations;

namespace JxAudio.TransVo;

public class RePassVo
{
    [Required]
    public string? OldPassword { get; set; }

    [Required]
    public string? NewPassword { get; set; }

    [Required]
    public string? ReNewPassword { get; set; }
}