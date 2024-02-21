using JxAudio.Extensions;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JxAudio.Web.Controllers;

public class AudioController : Microsoft.AspNetCore.Mvc.Controller
{

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.Request.Path.Value?.Length >= 1 &&
            context.HttpContext.Request.Path.Value.IndexOf('/', 1) > 0)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        
        if (context.HttpContext.Request.Method != "GET")
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return;
        }
        
        string[]? splitPath = context.HttpContext.Request.Path.Value?.Split('.', 3);

        if (splitPath?.Length > 2)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (splitPath?.Length == 2)
        {
            context.HttpContext.Request.Path = splitPath[0];
            context.HttpContext.Items[Constant.PathExtensionKey] = splitPath[1];
        }
        
        try
        {
            context.HttpContext.Items[Constant.ApiContextKey] = await context.HttpContext.CreateApiContextAsync().ConfigureAwait(false);
            
            var resultContext = await next();
        }
        catch (RestApiErrorException ex)
        {
            await context.HttpContext.WriteErrorResponseAsync(ex.Code, ex.Message).ConfigureAwait(false);
        }


        // Do something after the action executes.
        // 在动作执行之后做一些处理。
    }

}