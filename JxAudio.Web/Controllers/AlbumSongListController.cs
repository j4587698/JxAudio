using System.Diagnostics.CodeAnalysis;
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
    
    [Inject]
    [NotNull]
    private TrackService? TrackService { get; set; }
    
    [HttpGet("/getAlbumList")]
    public async Task GetAlbumList(int? musicFolderId, string? type, int? size, int? offset, int? fromYear, int? toYear, string? genre)
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
                case "byYear":
                    Util.CheckRequiredParameters(nameof(fromYear), fromYear);
                    Util.CheckRequiredParameters(nameof(toYear), toYear);
                    albumList2 = await AlbumService.GetAlbumList2ByYearAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value,fromYear!.Value, toYear!.Value,  HttpContext.RequestAborted);
                    break;
                case "byGenre":
                    Util.CheckRequiredParameters(nameof(genre), genre);
                    albumList2 = await AlbumService.GetAlbumList2ByGenreAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, genre!, HttpContext.RequestAborted);
                    break;
                default:
                    throw RestApiErrorException.InvalidParameterError("type");
            }

            var albumList = new AlbumList()
            {
                album = albumList2?.album.Select(x => x.CreateDirectoryChild()).ToArray()
            };

            await HttpContext.WriteResponseAsync(ItemChoiceType.albumList, albumList);
        }
    }

    [HttpGet("/getAlbumList2")]
    public async Task GetAlbumList2(int? musicFolderId, string? type, int? size, int? offset, int? fromYear, int? toYear, string? genre)
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
                case "byYear":
                    Util.CheckRequiredParameters(nameof(fromYear), fromYear);
                    Util.CheckRequiredParameters(nameof(toYear), toYear);
                    albumList2 = await AlbumService.GetAlbumList2ByYearAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value,fromYear!.Value, toYear!.Value,  HttpContext.RequestAborted);
                    break;
                case "byGenre":
                    Util.CheckRequiredParameters(nameof(genre), genre);
                    albumList2 = await AlbumService.GetAlbumList2ByGenreAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, genre!, HttpContext.RequestAborted);
                    break;
                default:
                    throw RestApiErrorException.InvalidParameterError("type");
            }
            
            await HttpContext.WriteResponseAsync(ItemChoiceType.albumList2, albumList2);
        }
    }

    [HttpGet("/getRandomSongs")]
    public async Task GetRandomSongs(int? musicFolderId, string? type, int? size, int? fromYear, int? toYear, string? genre)
    {
        size ??= 10;
        if (size is < 1 or > 500)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(size));
        }
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var randomSongs = await TrackService.GetRandomSongsAsync(apiUserId.Value, musicFolderId, genre, fromYear, toYear,
                size.Value, HttpContext.RequestAborted);

            await HttpContext.WriteResponseAsync(ItemChoiceType.randomSongs, randomSongs);
        }
    }

    [HttpGet("/getSongsByGenre")]
    public async Task GetSongsByGenre(int? musicFolderId, int? count, int? offset, string? genre)
    {
        Util.CheckRequiredParameters(nameof(genre), genre);
        count ??= 10;
        offset ??= 0;
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var songs = await TrackService.GetSongsByGenreAsync(apiUserId.Value, musicFolderId, genre!, offset.Value, count.Value, HttpContext.RequestAborted);

            await HttpContext.WriteResponseAsync(ItemChoiceType.songsByGenre, songs);
        }
    }
    
}