using System.Security.Claims;
using BootstrapBlazor.Components;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.TransVo;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DynamicFilterInfo = FreeSql.Internal.Model.DynamicFilterInfo;
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

public class ArtistController(ArtistService artistService): DynamicControllerBase
{
    [Authorize]
    public async Task<object> Post([FromBody] QueryOptionsVo queryOptionsVo)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var queryAsync = await artistService.QueryData(queryOptionsVo.Adapt<QueryPageOptions>(),
            queryOptionsVo.DynamicFilterInfo.Adapt<DynamicFilterInfo>(), Guid.Parse(id));
        return ResultVo.Success(data: new QueryData<ArtistVo>()
        {
            Items = queryAsync.Items?.Select(x => x.Adapt<ArtistVo>()),
            TotalCount = queryAsync.TotalCount,
            IsAdvanceSearch = true,
            IsFiltered = true,
            IsSearch = true,
            IsSorted = true
        });
    }
    
    [Authorize]
    public async Task<object> GetAllTracks(int artistId)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var tracks = await artistService.GetTracksByArtistIdAsync(artistId, Guid.Parse(id), HttpContext.RequestAborted);
        return ResultVo.Success(data: tracks.Adapt<List<TrackVo>>());
    }
}