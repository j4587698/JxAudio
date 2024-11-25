using System.Security.Claims;
using Jx.Toolbox.Extensions;
using Jx.Toolbox.Utils;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.TransVo;
using JxAudio.Web.Vo;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

public class UserController(IStringLocalizer<UserController> userLocalizer, UserService userService): DynamicControllerBase
{
    public async Task<object> PostLogin([FromBody]LoginVo loginVo)
    {
        if (loginVo.UserName.IsNullOrEmpty() || loginVo.Password.IsNullOrEmpty())
        {
            return ResultVo.Fail(500, userLocalizer["NotEmpty"]);
        }

        var user = await userService.ValidatePasswordAsync(loginVo.UserName!, loginVo.Password!, HttpContext.RequestAborted);
        if (user == null)
        {
            return ResultVo.Fail(500, userLocalizer["Invalid"]);
        }
        
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName!));
        identity.AddClaim(new Claim(ClaimTypes.Sid, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? ""));
        if (user.IsAdmin)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        }
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(identity), 
            new AuthenticationProperties(){IsPersistent = true, ExpiresUtc = loginVo.IsKeep ? DateTimeOffset.Now.AddDays(7) : DateTimeOffset.Now.AddDays(1)});
        return ResultVo.Success();
    }

    [Authorize]
    public async Task<object> GetUserInfo()
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)!.Value;
        var user = await userService.GetUserByUsernameAsync(username);
        return ResultVo.Success(data: new
        {
            user.UserName,
            Avatar = Avatar.GetAvatarUrl(user.Email),
            user.IsAdmin,
            user.IsGuest,
            user.MaxBitRate
        });
    }
    
    [Authorize]
    [HttpGet("/api/User/Logout")]
    public IActionResult Logout()
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("User/Login");
    }

    [Authorize]
    public async Task<object> PostResetPassword([FromBody]RePassVo rePassVo)
    {
        if (rePassVo.NewPassword.IsNullOrEmpty())
        {
            return ResultVo.Fail(500, userLocalizer["NotEmpty"]);
        }

        if (rePassVo.NewPassword != rePassVo.ReNewPassword)
        {
            return ResultVo.Fail(500, userLocalizer["RePassword"]);
        }

        var username = HttpContext.User.FindFirst(ClaimTypes.Name)!.Value;
        var user = await userService.ValidatePasswordAsync(username, rePassVo.OldPassword!, HttpContext.RequestAborted);
        if (user == null)
        {
            return ResultVo.Fail(500, userLocalizer["OldPasswordError"]);
        }

        user.Password = rePassVo.NewPassword;
        userService.EncryptPassword(user);
        await user.SaveAsync();
        return ResultVo.Success(data: "success");
    }
}