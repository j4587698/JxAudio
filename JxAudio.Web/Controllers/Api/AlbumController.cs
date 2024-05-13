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
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

public class AlbumController(AlbumService albumService): DynamicControllerBase
{
    [Authorize]
    public async Task<object> Post([FromBody]QueryVo queryVo)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var queryAsync = await albumService.QueryData(queryVo.QueryPageOptions!, queryVo.DynamicFilterInfo!, Guid.Parse(id));
        return ResultVo.Success(data: new QueryData<AlbumVo>()
        {
            Items = queryAsync.Items?.Select(x => x.Adapt<AlbumVo>()),
            TotalCount = queryAsync.TotalCount,
            IsAdvanceSearch = queryAsync.IsAdvanceSearch,
            IsFiltered = queryAsync.IsFiltered,
            IsSearch = queryAsync.IsSearch,
            IsSorted = queryAsync.IsSorted
        });
    }

    [Authorize]
    public async Task<object> GetAllTracks(int albumId)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var tracks = await albumService.GetTracksByAlbumIdAsync(albumId, Guid.Parse(id), HttpContext.RequestAborted);
        return ResultVo.Success(data: tracks.Adapt<List<TrackVo>>());
    }
}