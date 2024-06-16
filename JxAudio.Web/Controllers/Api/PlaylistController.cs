using System.Security.Claims;
using Jx.Toolbox.Extensions;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.TransVo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

[Authorize]
public class PlaylistController(PlaylistService playlistService): DynamicControllerBase
{

    public async Task<object> Get()
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var playlists = await playlistService.GetPlaylistsAsync(Guid.Parse(id), HttpContext.RequestAborted);
        return ResultVo.Success(data: playlists);
    }
    
    public async Task<object> CreatePlayList([FromBody]PlaylistVo? playlist)
    {
        if (playlist == null || playlist.Name.IsNullOrEmpty())
        {
            return ResultVo.Fail(500, "Playlist name is required");
        }
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        await playlistService.CreatePlaylistAsync(Guid.Parse(id), playlist.Name!, playlist.Description, playlist.IsPublic,
            playlist.Songs?.Select(x => x.Id).ToList(), HttpContext.RequestAborted);
        return ResultVo.Success();
    }

    public async Task<object> UpdatePlaylist([FromBody]PlaylistVo? playlist)
    {
        if (playlist?.Id is null or 0)
        {
            return ResultVo.Fail(500, "Playlist id is required");
        }
        
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        await playlistService.UpdatePlaylistAsync(Guid.Parse(id),playlist.Id.Value, playlist.Name, playlist.Description, playlist.IsPublic,
            playlist.Songs?.Select(x => x.Id).ToList(), [4587698], HttpContext.RequestAborted);
        return ResultVo.Success();
    }
    
}