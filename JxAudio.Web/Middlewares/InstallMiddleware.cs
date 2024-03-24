using Jx.Toolbox.Extensions;
using JxAudio.Web.Utils;

namespace JxAudio.Web.Middlewares;

/// <summary>
/// 安装服务中间件
/// </summary>
public class InstallMiddleware
{
    private readonly RequestDelegate _next;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    public InstallMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.Value != null && !Util.IsInstalled &&
            !context.Request.Path.Value.Contains(new []{"/Install", ".js", ".css", "_blazor"}))
        {
            context.Response.Redirect(
                $"{context.Request.Scheme}://{context.Request.Host}/Install");
        }
        else
        {
            await _next.Invoke(context);
        }
            
    }
}