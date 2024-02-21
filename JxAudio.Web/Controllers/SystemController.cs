using JxAudio.Core.Subsonic;
using JxAudio.Extensions;
using JxAudio.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class SystemController : AudioController
{
    [HttpGet("/ping")]
    public Task Ping()
    {
        return HttpContext.WriteResponseAsync(0, null);
    }

    [HttpGet("/getLicense")]
    public Task GetLicense()
    {
        var license = new License()
        {
            valid = true,
        };

        return HttpContext.WriteResponseAsync(ItemChoiceType.license, license);
    }
}