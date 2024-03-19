using System.Diagnostics.CodeAnalysis;
using Jx.Toolbox.Extensions;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class PlaylistsController: AudioController
{
    [Inject]
    [NotNull]
    private PlaylistService? PlaylistService { get; set; }
    
    [HttpGet("/getPlaylists")]
    public async Task GetPlaylists(string? username)
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var playlists = await PlaylistService.GetPlaylistsAsync(apiUserId.Value, HttpContext.RequestAborted);

            await HttpContext.WriteResponseAsync(ItemChoiceType.playlists, playlists);
        }
    }

    [HttpGet("/getPlaylist")]
    public async Task GetPlaylist(string? id)
    {
        Util.CheckRequiredParameters(nameof(id), id);
        var playlistId = id!.ParsePlaylistId();
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var playlist = await PlaylistService.GetPlaylistAsync(apiUserId.Value, playlistId, HttpContext.RequestAborted);
            
            await HttpContext.WriteResponseAsync(ItemChoiceType.playlist, playlist);
        }
    }

    [HttpGet("/createPlaylist")]
    public async Task CreatePlaylist(string? playlistId, string? name, string[]? songId)
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            if (playlistId.IsNullOrEmpty())
            {
                Util.CheckRequiredParameters(nameof(name), name);

                playlistId = await PlaylistService.CreatePlaylistAsync(apiUserId.Value, name!,
                    songId?.Select(x => x.ParseTrackId()).ToList(), HttpContext.RequestAborted);
            }
            else
            {
                await PlaylistService.RecreatePlaylistAsync(apiUserId.Value, playlistId!.ParsePlaylistId(), name,
                    songId?.Select(x => x.ParseTrackId()).ToList(), HttpContext.RequestAborted);
            }
            
            var playlist = await PlaylistService.GetPlaylistAsync(apiUserId.Value, playlistId!.ParsePlaylistId(), HttpContext.RequestAborted);
            
            await HttpContext.WriteResponseAsync(ItemChoiceType.playlist, playlist);
        }
    }

    [HttpGet("/updatePlaylist")]
    public async Task UpdatePlaylist(string? playlistId, string? name, string? comment, bool? @public, string[]? songIdToAdd, int[]? songIndexToRemove)
    {
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            Util.CheckRequiredParameters(nameof(playlistId), playlistId);

            await PlaylistService.UpdatePlaylistAsync(apiUserId.Value, playlistId!.ParsePlaylistId(), name, comment, @public,
                songIdToAdd?.Select(x => x.ParseTrackId()).ToList(), songIndexToRemove, HttpContext.RequestAborted);
        
            var playlist = await PlaylistService.GetPlaylistAsync(apiUserId.Value, playlistId!.ParsePlaylistId(), HttpContext.RequestAborted);
            
            await HttpContext.WriteResponseAsync(ItemChoiceType.playlist, playlist);
        }
    }

    [HttpGet("/deletePlaylist")]
    public async Task DeletePlaylist(string? id)
    {
        Util.CheckRequiredParameters(nameof(id), id);
        var playlistId = id!.ParsePlaylistId();
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            await PlaylistService.DeletePlaylistAsync(apiUserId.Value, playlistId, HttpContext.RequestAborted);
            
            await HttpContext.WriteResponseAsync(0, null);
        }
    }
}