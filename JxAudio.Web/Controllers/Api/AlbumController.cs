using System.Security.Claims;
using BootstrapBlazor.Components;
using FreeSql.Internal.Model;
using JxAudio.Core;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.TransVo;
using JxAudio.Web.Vo;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetTaste;
using DynamicFilterInfo = FreeSql.Internal.Model.DynamicFilterInfo;
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

public class AlbumController(AlbumService albumService): DynamicControllerBase
{
    [Authorize]
    public async Task<object> Post([FromBody]QueryOptionsVo queryOptionsVo)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var queryAsync = await albumService.QueryData(queryOptionsVo.Adapt<QueryPageOptions>()!, queryOptionsVo.DynamicFilterInfo.Adapt<DynamicFilterInfo>()!, Guid.Parse(id));
        return ResultVo.Success(data: new QueryData<AlbumVo>()
        {
            Items = queryAsync.Items?.Select(x => x.Adapt<AlbumVo>()),
            TotalCount = queryAsync.TotalCount,
            IsAdvanceSearch = true,
            IsFiltered = true,
            IsSearch = true,
            IsSorted = true
        });
    }
    
    [Authorize]
    public async Task<object> Get(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var album = await albumService.GetAlbumById(id, Guid.Parse(userId), HttpContext.RequestAborted);
        return ResultVo.Success(data: album.Adapt<AlbumVo>());
    }

    [Authorize]
    public async Task<object> GetAllTracks(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var tracks = await albumService.GetTracksByAlbumIdAsync(id, Guid.Parse(userId), HttpContext.RequestAborted);
        return ResultVo.Success(data: tracks.Adapt<List<TrackVo>>());
    }
    
    [Authorize]
    public async Task<object> GetStar(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        await albumService.StarAlbumAsync(Guid.Parse(userId), [id], HttpContext.RequestAborted);
        return ResultVo.Success(data: "s");
    }

    [Authorize]
    public async Task<object> GetUnStar(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        await albumService.UnStarAlbumAsync(Guid.Parse(userId), [id], HttpContext.RequestAborted);
        return ResultVo.Success(data: "s");
    }
}