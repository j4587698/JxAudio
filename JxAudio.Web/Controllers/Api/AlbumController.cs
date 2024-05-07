using System.Security.Claims;
using BootstrapBlazor.Components;
using JxAudio.Core;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.TransVo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

public class AlbumController(AlbumService albumService): DynamicControllerBase
{
    [Authorize]
    public async Task<object> Post([FromBody]QueryPageOptions options)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        if (options.SortName == "ArtistName")
        {
            options.SortName = "ArtistEntity.Name";
        }
        var queryAsync = await albumService.QueryData(options, Guid.Parse(id));
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
}