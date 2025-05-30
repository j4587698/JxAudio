﻿using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using JxAudio.Core;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Rest;

public class AlbumSongListController(ArtistService artistService, AlbumService albumService, TrackService trackService) : AudioController
{
    [HttpGet("getAlbumList")]
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
            QueryData<AlbumEntity>? albumQuery = null;
            switch (type)
            {
                case "random":
                    albumQuery = await albumService.QueryAlbumRandomAsync(apiUserId.Value, musicFolderId, size.Value, HttpContext.RequestAborted);
                    break;
                case "newest":
                    albumQuery = await albumService.QueryAlbumNewestAsync(apiUserId.Value, musicFolderId,  offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "highest": // 与最高播放一致
                case "frequent":
                    albumQuery = await albumService.QueryAlbumFrequentAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "recent":
                    albumQuery = await albumService.QueryAlbumRecentAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "alphabeticalByName":
                    albumQuery = await albumService.QueryAlbumOrderedByAlbumTitleAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "alphabeticalByArtist":
                    albumQuery = await albumService.QueryAlbumOrderedByArtistNameAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "starred":
                    albumQuery = await albumService.QueryAlbumStarredAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "byYear":
                    Util.CheckRequiredParameters(nameof(fromYear), fromYear);
                    Util.CheckRequiredParameters(nameof(toYear), toYear);
                    albumQuery = await albumService.QueryAlbumByYearAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value,fromYear!.Value, toYear!.Value, HttpContext.RequestAborted);
                    break;
                case "byGenre":
                    Util.CheckRequiredParameters(nameof(genre), genre);
                    albumQuery = await albumService.QueryAlbumByGenreAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, genre!, HttpContext.RequestAborted);
                    break;
                default:
                    throw RestApiErrorException.InvalidParameterError("type");
            }

            if (albumQuery.Items == null)
            {
                throw RestApiErrorException.DataNotFoundError();
            }

            var albumList = new AlbumList()
            {
                album = albumQuery.Items.Select(x => x.CreateAlbumId3().CreateDirectoryChild()).ToArray()
            };

            await HttpContext.WriteResponseAsync(ItemChoiceType.albumList, albumList);
        }
    }

    [HttpGet("getAlbumList2")]
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
            QueryData<AlbumEntity>? albumQuery = null;
            switch (type)
            {
                case "random":
                    albumQuery = await albumService.QueryAlbumRandomAsync(apiUserId.Value, musicFolderId, size.Value, HttpContext.RequestAborted);
                    break;
                case "newest":
                    albumQuery = await albumService.QueryAlbumNewestAsync(apiUserId.Value, musicFolderId,  offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "highest": // 与最高播放一致
                case "frequent":
                    albumQuery = await albumService.QueryAlbumFrequentAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "recent":
                    albumQuery = await albumService.QueryAlbumRecentAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "alphabeticalByName":
                    albumQuery = await albumService.QueryAlbumOrderedByAlbumTitleAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "alphabeticalByArtist":
                    albumQuery = await albumService.QueryAlbumOrderedByArtistNameAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "starred":
                    albumQuery = await albumService.QueryAlbumStarredAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, HttpContext.RequestAborted);
                    break;
                case "byYear":
                    Util.CheckRequiredParameters(nameof(fromYear), fromYear);
                    Util.CheckRequiredParameters(nameof(toYear), toYear);
                    albumQuery = await albumService.QueryAlbumByYearAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value,fromYear!.Value, toYear!.Value, HttpContext.RequestAborted);
                    break;
                case "byGenre":
                    Util.CheckRequiredParameters(nameof(genre), genre);
                    albumQuery = await albumService.QueryAlbumByGenreAsync(apiUserId.Value, musicFolderId, offset.Value, size.Value, genre!, HttpContext.RequestAborted);
                    break;
                default:
                    throw RestApiErrorException.InvalidParameterError("type");
            }

            if (albumQuery.Items == null)
            {
                throw RestApiErrorException.DataNotFoundError();
            }
            
            await HttpContext.WriteResponseAsync(ItemChoiceType.albumList2, new AlbumList2() {album = albumQuery.Items.Select(x => x.CreateAlbumId3()).ToArray()});
        }
    }

    [HttpGet("getRandomSongs")]
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
            var randomSongs = await trackService.GetRandomSongsAsync(apiUserId.Value, musicFolderId, genre, fromYear, toYear,
                size.Value, HttpContext.RequestAborted);

            await HttpContext.WriteResponseAsync(ItemChoiceType.randomSongs, randomSongs);
        }
    }

    [HttpGet("getSongsByGenre")]
    public async Task GetSongsByGenre(int? musicFolderId, int? count, int? offset, string? genre)
    {
        Util.CheckRequiredParameters(nameof(genre), genre);
        count ??= 10;
        offset ??= 0;
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var songs = await trackService.GetSongsByGenreAsync(apiUserId.Value, musicFolderId, genre!, offset.Value, count.Value, HttpContext.RequestAborted);

            await HttpContext.WriteResponseAsync(ItemChoiceType.songsByGenre, songs);
        }
    }

    [HttpGet("getNowPlaying")]
    public void GetNowPlaying()
    {
        throw RestApiErrorException.NotImplemented();
    }

    [HttpGet("getStarred")]
    public async Task GetStarred(int? musicFolderId)
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var artistsId3 = await artistService.GetStar2ArtistsId3(apiUserId.Value, musicFolderId, HttpContext.RequestAborted);
            var albumsId3 = await albumService.GetStar2AlbumsId3(apiUserId.Value, musicFolderId, HttpContext.RequestAborted);
            var tracks = await trackService.GeStar2Songs(apiUserId.Value, musicFolderId, HttpContext.RequestAborted);

            var starred = new Starred()
            {
                artist = artistsId3.Select(x => x.CreateArtist()).ToArray(),
                album = albumsId3.Select(x => x.CreateDirectoryChild()).ToArray(),
                song = tracks
            };

            await HttpContext.WriteResponseAsync(ItemChoiceType.starred, starred);
        }
    }
    
    [HttpGet("getStarred2")]
    public async Task GetStarred2(int? musicFolderId)
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var artistsId3 = await artistService.GetStar2ArtistsId3(apiUserId.Value, musicFolderId, HttpContext.RequestAborted);
            var albumsId3 = await albumService.GetStar2AlbumsId3(apiUserId.Value, musicFolderId, HttpContext.RequestAborted);
            var tracks = await trackService.GeStar2Songs(apiUserId.Value, musicFolderId, HttpContext.RequestAborted);

            var starred = new Starred2()
            {
                artist = artistsId3,
                album = albumsId3,
                song = tracks
            };

            await HttpContext.WriteResponseAsync(ItemChoiceType.starred2, starred);
        }
    }
}