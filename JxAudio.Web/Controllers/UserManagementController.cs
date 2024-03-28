using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class UserManagementController(UserService userService): AudioController
{
    [HttpGet("/getUser")]
    public async Task GetUser(string? username)
    {
        Util.CheckRequiredParameters(nameof(username), username);
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;

        if (username != apiContext?.User?.UserName)
        {
            throw RestApiErrorException.UserNotAuthorizedError();
        }
        
        var user = await userService.GetUserAsync(apiContext!.User!, HttpContext.RequestAborted);

        await HttpContext.WriteResponseAsync(ItemChoiceType.user, user);
    }

    [HttpGet("/getUsers")]
    public void GetUsers()
    {
        throw RestApiErrorException.UserNotAuthorizedError();
    }

    [HttpGet("/createUser")]
    public void CreateUser()
    {
        throw RestApiErrorException.UserNotAuthorizedError();
    }
    
    [HttpGet("/updateUser")]
    public void UpdateUser()
    {
        throw RestApiErrorException.UserNotAuthorizedError();
    }
    
    [HttpGet("/deleteUser")]
    public void DeleteUser()
    {
        throw RestApiErrorException.UserNotAuthorizedError();
    }

    [HttpGet("/changePassword")]
    public async Task ChangePassword(string? username, string? password)
    {
        Util.CheckRequiredParameters(nameof(username), username);
        Util.CheckRequiredParameters(nameof(password), password);
        
        var decodePassword = Util.HexDecodePassword(password!);
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;

        if (username != apiContext?.User?.UserName)
        {
            throw RestApiErrorException.UserNotAuthorizedError();
        }
        
        await userService.UpdatePassword(apiContext!.User!, decodePassword);
        await HttpContext.WriteResponseAsync(0, null);
    }
}