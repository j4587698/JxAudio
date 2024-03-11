using System.Diagnostics.CodeAnalysis;
using Jx.Toolbox.Extensions;
using JxAudio.Core;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Directory = JxAudio.Core.Subsonic.Directory;
using Index = JxAudio.Core.Subsonic.Index;

namespace JxAudio.Web.Controllers;

public class BrowsingController : AudioController
{
    [Inject]
    [NotNull]
    private DirectoryService? DirectoryService { get; set; }

    [Inject]
    [NotNull]
    private ArtistService? ArtistService { get; set; }
    
    [Inject]
    [NotNull]
    private GenreService? GenreService { get; set; }
    
    [Inject]
    [NotNull]
    private AlbumService? AlbumService { get; set; }

    [Inject]
    [NotNull]
    private TrackService? TrackService { get; set; }
    
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
    
    [HttpGet("/getIndexes")]
    public async Task GetIndexes(int? musicFolderId, long? ifModifiedSince)
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var id3 = await ArtistService.GetArtistsAsync(apiUserId.Value, musicFolderId, ifModifiedSince, HttpContext.RequestAborted);
            var index = new Indexes()
            {
                ignoredArticles = id3.ignoredArticles,
                index = id3.index.Select(x => new Index()
                {
                    name = x.name,
                    artist = x.artist.Select(y => new Artist()
                    {
                        id = y.id,
                        name = y.name,
                        starred = y.starred,
                        starredSpecified = y.starredSpecified,
                        userRating = default,
                        userRatingSpecified = false,
                        averageRating = default,
                        averageRatingSpecified = false,
                    }).ToArray()
                }).ToArray()
            };
            await HttpContext.WriteResponseAsync(ItemChoiceType.indexes, index);
        }
    }

    [HttpGet("/getMusicDirectory")]
    public async Task GetMusicDirectory(string? id)
    {
        if (id.IsNullOrEmpty())
        {
            throw RestApiErrorException.RequiredParameterMissingError("id");
        }
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            Directory? directory = null;
            if (id!.TryParseArtistId(out var artistId))
            {
                var id3 = await ArtistService.GetArtistAsync(apiUserId.Value, artistId, HttpContext.RequestAborted);
                directory = new Directory()
                {
                    id = id3.id,
                    parent = null,
                    name = id3.name,
                    starred = id3.starred,
                    starredSpecified = id3.starredSpecified,
                    child = id3.album.Select(x => x.CreateDirectoryChild()).ToArray()
                };
            }
            else if (id!.TryParseAlbumId(out var albumId))
            {
                var album = await AlbumService.GetAlbumAsync(apiUserId.Value, albumId, HttpContext.RequestAborted);
                directory = new Directory()
                {
                    id = album.id,
                    parent = null,
                    name = album.name,
                    starred = album.starred,
                    starredSpecified = album.starredSpecified,
                    child = album.song
                };
            }
            else
            {
                throw RestApiErrorException.GenericError($"Invalid id for {id}.");
            }
            
            await HttpContext.WriteResponseAsync(ItemChoiceType.directory, directory);
        }
    }
    

    [HttpGet("/getGenres")]
    public async Task GetGenres()
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var genres = await GenreService.GetGenresAsync(apiUserId.Value, HttpContext.RequestAborted);
            await HttpContext.WriteResponseAsync(ItemChoiceType.genres, genres);
        }
    }
    
    [HttpGet("/getArtists")]
    public async Task GetArtists(int? musicFolderId)
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var id3 = await ArtistService.GetArtistsAsync(apiUserId.Value, musicFolderId, null, HttpContext.RequestAborted);
            await HttpContext.WriteResponseAsync(ItemChoiceType.artists, id3);
        }
    }

    [HttpGet("/getArtist")]
    public async Task GetArtist(string? id)
    {
        if (id.IsNullOrEmpty())
        {
            throw RestApiErrorException.RequiredParameterMissingError("id");
        }
        var artistId = id!.ParseArtistId();
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var id3 = await ArtistService.GetArtistAsync(apiUserId.Value, artistId, HttpContext.RequestAborted);
            await HttpContext.WriteResponseAsync(ItemChoiceType.artist, id3);
        }
    }
    
    [HttpGet("/getAlbum")]
    public async Task GetAlbum(string? id)
    {
        if (id.IsNullOrEmpty())
        {
            throw RestApiErrorException.RequiredParameterMissingError("id");
        }
        var albumId = id!.ParseAlbumId();
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var id3 = await AlbumService.GetAlbumAsync(apiUserId.Value, albumId, HttpContext.RequestAborted);
            await HttpContext.WriteResponseAsync(ItemChoiceType.album, id3);
        }
    }

    [HttpGet("/getSong")]
    public async Task GetSong(string? id)
    {
        if (id.IsNullOrEmpty())
        {
            throw RestApiErrorException.RequiredParameterMissingError("id");
        }

        var trackId = id!.ParseTrackId();
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var track = await TrackService.GetSongAsync(apiUserId.Value, trackId, HttpContext.RequestAborted);
            await HttpContext.WriteResponseAsync(ItemChoiceType.song, track); 
        }
    }
    
    [HttpGet("/getVideos")]
    public void GetVideos()
    {
        throw RestApiErrorException.GenericError("Not implemented.");
    }
    
    [HttpGet("/getVideoInfo")]
    public void GetVideoInfo()
    {
        throw RestApiErrorException.GenericError("Not implemented.");
    }
}