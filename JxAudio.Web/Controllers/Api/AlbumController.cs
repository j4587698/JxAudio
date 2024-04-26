using BootstrapBlazor.Components;
using JxAudio.Core;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Api;

public class AlbumController: DynamicControllerBase
{
    public object Get([FromBody]QueryPageOptions options)
    {
        
        switch (options.SortName)
        {
            case "random":
                break;
        }

        return null;
    }
}