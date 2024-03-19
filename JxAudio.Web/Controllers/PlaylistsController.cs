﻿using System.Diagnostics.CodeAnalysis;
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
}