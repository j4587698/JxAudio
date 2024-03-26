using JxAudio.Core;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class MediaAnnotationController(ArtistService artistService, AlbumService albumService, TrackService trackService): AudioController
{
    [HttpGet("/star")]
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
}