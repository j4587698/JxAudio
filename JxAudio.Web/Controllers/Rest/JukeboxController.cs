using JxAudio.Core;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Rest;

public class JukeboxController: AudioController
{
    [HttpGet("jukeboxControl")]
    public void JukeboxControl()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
}