using System.Diagnostics.CodeAnalysis;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
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
}