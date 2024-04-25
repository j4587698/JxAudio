using BootstrapBlazor.Components;
using Front.WebAssembly.Data;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Front.WebAssembly.Handler;

public class HttpHandler(NavigationManager navigationManager, ToastService toastService): DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            navigationManager.NavigateTo("/User/Login", true);
            await toastService.Information("未登录", "将跳转登录页面");
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            navigationManager.NavigateTo("/User/Login", true);
            await toastService.Information("用户被封禁", "将跳转登录页面");
        }
        else if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<ResultVo>(cancellationToken: cancellationToken);
            if (content is not { Code: 200 })
            {
                await toastService.Information("请求失败", content?.Message ?? "系统异常");
            }
            else
            {
                var data = JsonSerializer.Serialize(content.Data);
                response.Content = new StringContent(data, Encoding.UTF8, "application/json");
            }
        }
        else
        {
            await toastService.Information("请求失败", "系统异常");
            navigationManager.NavigateTo("/");
        }
        return response;
    }
}
