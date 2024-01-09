using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Controller;


public class SystemController: ControllerBase
{
    [HttpPost("/ping")]
    public void Ping()
    {
        
    }
}