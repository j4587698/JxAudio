namespace JxAudio.Web.Vo;

public class LoginVo
{
    public string? UserName { get; set; }

    public string? PasswordHex { get; set; }

    public string? Salt { get; set; }
}