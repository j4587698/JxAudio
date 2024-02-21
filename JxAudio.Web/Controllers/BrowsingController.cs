using System.Diagnostics.CodeAnalysis;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class BrowsingController : AudioController
{
    [Inject]
    [NotNull]
    private DirectoryService? DirectoryService { get; set; }
    
    [HttpGet("/getMusicFolders")]
    public async Task GetMusicFolders()
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var folders = await DirectoryService.GetMusicFoldersAsync(apiUserId.Value, HttpContext.RequestAborted);
            await HttpContext.WriteResponseAsync(ItemChoiceType.musicFolders, folders);
        }
    }
}