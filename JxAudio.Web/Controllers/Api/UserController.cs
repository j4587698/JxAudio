using System.Security.Claims;
using Jx.Toolbox.Extensions;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.Web.Vo;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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
        if (user.IsAdmin)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        }
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(identity), 
            new AuthenticationProperties(){IsPersistent = true, ExpiresUtc = DateTimeOffset.Now.AddDays(1)});
        return ResultVo.Success();
    }

    [Authorize]
    public object GetUserInfo()
    {
        return ResultVo.Success();
    }
}