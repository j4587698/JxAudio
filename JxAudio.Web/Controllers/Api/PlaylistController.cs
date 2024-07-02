using System.Security.Claims;
using BootstrapBlazor.Components;
using Jx.Toolbox.Extensions;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.TransVo;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DynamicFilterInfo = FreeSql.Internal.Model.DynamicFilterInfo;
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
    
    public async Task<object> Post([FromBody] QueryOptionsVo queryOptionsVo)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var queryAsync = await playlistService.QueryData(queryOptionsVo.Adapt<QueryPageOptions>(),
            queryOptionsVo.DynamicFilterInfo.Adapt<DynamicFilterInfo>(), Guid.Parse(id), false);
        return ResultVo.Success(data: new QueryData<PlaylistVo>()
        {
            Items = queryAsync.Items?.Select(x => x.Adapt<PlaylistVo>()),
            TotalCount = queryAsync.TotalCount,
            IsAdvanceSearch = true,
            IsFiltered = true,
            IsSearch = true,
            IsSorted = true
        });
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