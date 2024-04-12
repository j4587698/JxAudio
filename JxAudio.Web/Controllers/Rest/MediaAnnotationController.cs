using JxAudio.Core;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Rest;

public class MediaAnnotationController(ArtistService artistService, AlbumService albumService, TrackService trackService): AudioController
{
    [HttpGet("star")]
    public async Task Star(string[]? id, string[]? albumId, string[]? artistId)
    {
        var artistIds = new List<int>();
        var albumIds = new List<int>();
        var trackIds = new List<int>();

        if (id != null)
        {
            foreach (var s in id)
            {
                if (s.TryParseArtistId(out var artId))
                {
                    artistIds.Add(artId);
                }
                else if (s.TryParseAlbumId(out var albId))
                {
                    albumIds.Add(albId);
                }
                else if (s.TryParseTrackId(out var tId))
                {
                    trackIds.Add(tId);
                }
                else
                {
                    throw RestApiErrorException.InvalidParameterError(nameof(id));
                }
            }
        }

        if (artistId != null)
        {
            artistIds.AddRange(artistId.Select(x => x.ParseArtistId()));
        }

        if (albumId != null)
        {
            albumIds.AddRange(albumId.Select(x => x.ParseAlbumId()));
        }
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            await artistService.StarArtistAsync(apiUserId.Value, artistIds, HttpContext.RequestAborted);
            await albumService.StarAlbumAsync(apiUserId.Value, albumIds, HttpContext.RequestAborted);
            await trackService.StarTrackAsync(apiUserId.Value, trackIds, HttpContext.RequestAborted);
        }

        await HttpContext.WriteResponseAsync(0, null);
    }

    [HttpGet("unstar")]
    public async Task UnStar(string[]? id, string[]? albumId, string[]? artistId)
    {
        var artistIds = new List<int>();
        var albumIds = new List<int>();
        var trackIds = new List<int>();

        if (id != null)
        {
            foreach (var s in id)
            {
                if (s.TryParseArtistId(out var artId))
                {
                    artistIds.Add(artId);
                }
                else if (s.TryParseAlbumId(out var albId))
                {
                    albumIds.Add(albId);
                }
                else if (s.TryParseTrackId(out var tId))
                {
                    trackIds.Add(tId);
                }
                else
                {
                    throw RestApiErrorException.InvalidParameterError(nameof(id));
                }
            }
        }

        if (artistId != null)
        {
            artistIds.AddRange(artistId.Select(x => x.ParseArtistId()));
        }

        if (albumId != null)
        {
            albumIds.AddRange(albumId.Select(x => x.ParseAlbumId()));
        }
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            await artistService.UnStarArtistAsync(apiUserId, artistIds, HttpContext.RequestAborted);
            await albumService.UnStarAlbumAsync(apiUserId, albumIds, HttpContext.RequestAborted);
            await trackService.UnStarTrackAsync(apiUserId, trackIds, HttpContext.RequestAborted);
        }
        
        await HttpContext.WriteResponseAsync(0, null);
    }

    [HttpGet("setRating")]
    public async Task SetRating(string? id, float? rating)
    {
        Util.CheckRequiredParameters(nameof(id), id);
        Util.CheckRequiredParameters(nameof(rating), rating);

        if (rating is < 0 or > 5)
        {
            throw RestApiErrorException.InvalidParameterError(nameof(rating));
        }
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            if (id!.TryParseAlbumId(out var albumId))
            {
                await albumService.SetAlbumRatingAsync(apiUserId.Value, albumId, rating!.Value, HttpContext.RequestAborted);
            }
            else if (id!.TryParseArtistId(out var artistId))
            {
                await artistService.SetArtistRatingAsync(apiUserId.Value, artistId, rating!.Value,
                    HttpContext.RequestAborted);
            }
            else if (id!.TryParseTrackId(out var trackId))
            {
                await trackService.SetTrackRatingAsync(apiUserId.Value, trackId, rating!.Value, HttpContext.RequestAborted);
            }
            else
            {
                throw RestApiErrorException.InvalidParameterError(nameof(id));
            }
            
            await HttpContext.WriteResponseAsync(0, null);
        }
    }

    [HttpGet("scrobble")]
    public async Task Scrobble(string? id, long? time, bool submission = true)
    {
        Util.CheckRequiredParameters(nameof(id), id);

        if (submission)
        {
            var trackId = id.ParseTrackId();
            await trackService.UpdatePlayCountAsync(trackId, HttpContext.RequestAborted);
        }
        
        await HttpContext.WriteResponseAsync(0, null);
    }
}