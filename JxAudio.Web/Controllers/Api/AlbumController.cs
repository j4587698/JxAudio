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
            Items = queryAsync.Items?.Select(x => new AlbumVo()
            {
                CoverId = x.PictureId,
                Title = x.Title,
                Id = x.Id,
                ArtistId = x.ArtistId,
                ArtistName = x.ArtistEntity?.Name,
                Count = x.TrackEntities?.Count ?? 0,
                TotalSize = x.TrackEntities?.Sum(y => y.Size) ?? 0
            }),
            TotalCount = queryAsync.TotalCount,
            IsAdvanceSearch = queryAsync.IsAdvanceSearch,
            IsFiltered = queryAsync.IsFiltered,
            IsSearch = queryAsync.IsSearch,
            IsSorted = queryAsync.IsSorted
        });
    }

    public object GetAllTracks(int albumId)
    {
        
    }
}