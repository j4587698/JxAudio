﻿using System.Diagnostics.CodeAnalysis;
using JxAudio.Core;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class AlbumSongListController : AudioController
{
    [Inject]
    [NotNull]
    private AlbumService? AlbumService { get; set; }
    
    [HttpGet("/getAlbumList")]
    public async Task GetAlbumList(int? musicFolderId, string? type, int? size, int? offset)
    {
        Util.CheckRequiredParameters(nameof(type), type);
        
        size ??= 10;
        if (size is < 1 or > 500)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(size));
        }
        
        offset ??= 0;
        if (offset < 0)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(offset));
        }
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            AlbumList2? albumList2 = null;
            switch (type)
            {
                case "random":
                    albumList2 = await AlbumService.GetAlbumList2RandomAsync(apiUserId.Value, musicFolderId, size.Value, HttpContext.RequestAborted);
                    break;
                case "newest":
                    albumList2 = await AlbumService.GetAlbumList2NewestAsync(apiUserId.Value, musicFolderId,  offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "highest": // 与最高播放一致
                case "frequent":
                    albumList2 = await AlbumService.GetAlbumList2FrequentAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "recent":
                    albumList2 = await AlbumService.GetAlbumList2RecentAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "alphabeticalByName":
                    albumList2 = await AlbumService.GetAlbumList2OrderedByAlbumTitleAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "alphabeticalByArtist":
                    albumList2 = await AlbumService.GetAlbumList2OrderedByArtistNameAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "starred":
                    albumList2 = await AlbumService.GetAlbumList2StarredAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
            }

            var albumList = new AlbumList()
            {
                album = albumList2?.album.Select(x => x.CreateDirectoryChild()).ToArray()
            };

            await HttpContext.WriteResponseAsync(ItemChoiceType.albumList, albumList);
        }
    }
}