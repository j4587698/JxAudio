using JxAudio.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Controller;

public class SystemController : AudioController
{
    [HttpPost("/ping")]
    public Task Ping()
    {
        return WriteResponseAsync(HttpContext, 0, null);
    }
}