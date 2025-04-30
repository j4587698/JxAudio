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

namespace JxAudio.Web.Controllers.Rest;

public class SearchingController(ArtistService artistService, AlbumService albumService, TrackService trackService): AudioController
{
    
    [HttpGet("search")]
    public void Search()
    {
        throw RestApiErrorException.NotImplemented();
    }

    [HttpGet("search2")]
    public async Task Search2(string? query, int? artistCount, int? artistOffset, int? albumCount, int? albumOffset, int? songCount, int? songOffset, int? musicFolderId)
    {
        Util.CheckRequiredParameters(nameof(query), query);

        if (query == "\"\"")
        {
            query = "";
        }

        artistCount ??= 20;
        if (artistCount is < 0 or > 500)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(artistCount));
        }
        
        artistOffset ??= 0;
        if (artistOffset < 0)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(artistOffset));
        }
        
        albumCount ??= 20;
        if (albumCount is < 0 or > 500)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(albumCount));
        }
        
        albumOffset ??= 0;
        if (albumOffset < 0)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(albumOffset));
        }
        
        songCount ??= 20;
        if (songCount is < 0 or > 500)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(songCount));
        }
        
        songOffset ??= 0;
        if (songOffset < 0)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(songOffset));
        }
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var artistsId3 = await artistService.GetSearch3ArtistId3(apiUserId.Value, musicFolderId, query!, artistCount.Value, artistOffset.Value, HttpContext.RequestAborted);
            var albumsId3 = await albumService.GetSearch3AlbumId3(apiUserId.Value, musicFolderId, query!, albumCount.Value, albumOffset.Value, HttpContext.RequestAborted);
            var tracks = await trackService.GetSearch3Songs(apiUserId.Value, musicFolderId, query!, songCount.Value, songOffset.Value, HttpContext.RequestAborted);
        
            var searchResult2 = new SearchResult2()
            {
                artist = artistsId3.Select(x => x.CreateArtistId3().CreateArtist()).ToArray(),
                album = albumsId3.Select(x => x.CreateAlbumId3().CreateDirectoryChild()).ToArray(),
                song = tracks.Select(x => x.CreateTrackChild()).ToArray(),
            };

            await HttpContext.WriteResponseAsync(ItemChoiceType.searchResult2, searchResult2);
        }
    }
    
    [HttpGet("search3")]
    public async Task Search3(string? query, int? artistCount, int? artistOffset, int? albumCount, int? albumOffset, int? songCount, int? songOffset, int? musicFolderId)
    {
        Util.CheckRequiredParameters(nameof(query), query);

        if (query == "\"\"")
        {
            query = "";
        }
        
        artistCount ??= 20;
        if (artistCount is < 0 or > 500)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(artistCount));
        }
        
        artistOffset ??= 0;
        if (artistOffset < 0)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(artistOffset));
        }
        
        albumCount ??= 20;
        if (albumCount is < 0 or > 500)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(albumCount));
        }
        
        albumOffset ??= 0;
        if (albumOffset < 0)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(albumOffset));
        }
        
        songCount ??= 20;
        if (songCount is < 0 or > 500)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(songCount));
        }
        
        songOffset ??= 0;
        if (songOffset < 0)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(songOffset));
        }
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var artistsId3 = await artistService.GetSearch3ArtistId3(apiUserId.Value, musicFolderId, query!, artistCount.Value, artistOffset.Value, HttpContext.RequestAborted);
            var albumsId3 = await albumService.GetSearch3AlbumId3(apiUserId.Value, musicFolderId, query!, albumCount.Value, albumOffset.Value, HttpContext.RequestAborted);
            var tracks = await trackService.GetSearch3Songs(apiUserId.Value, musicFolderId, query!, songCount.Value, songOffset.Value, HttpContext.RequestAborted);
        
            var searchResult3 = new SearchResult3()
            {
                artist = artistsId3.Select(x => x.CreateArtistId3()).ToArray(),
                album = albumsId3.Select(x => x.CreateAlbumId3()).ToArray(),
                song = tracks.Select(x => x.CreateTrackChild()).ToArray(),
            };

            await HttpContext.WriteResponseAsync(ItemChoiceType.searchResult3, searchResult3);
        }
    } 
}