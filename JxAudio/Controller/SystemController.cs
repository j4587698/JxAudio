using JxAudio.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Controller;

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
        var license = new Subsonic.License()
        {
            valid = true,
        };

        return HttpContext.WriteResponseAsync(Subsonic.ItemChoiceType.license, license);
    }
}