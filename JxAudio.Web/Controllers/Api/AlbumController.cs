using System.Security.Claims;
using BootstrapBlazor.Components;
using JxAudio.Core;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using JxAudio.Web.Vo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Api;

public class AlbumController(AlbumService albumService): DynamicControllerBase
{
    [Authorize]
    public async Task<object> Post([FromBody]QueryPageOptions options)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        QueryData<AlbumEntity> albumQueryData = default!;
        switch (options.SortName)
        {
            case "random":
                albumQueryData = await albumService.QueryAlbumRandomAsync(Guid.Parse(id), null,
                    options.PageItems, options.SearchText, HttpContext.RequestAborted);
                break;
            case "newest":
                albumQueryData = await albumService.QueryAlbumNewestAsync(Guid.Parse(id), null,
                    (options.PageIndex - 1) * options.PageItems, options.PageItems, options.SearchText,
                    HttpContext.RequestAborted);
                break;
            case "frequent":
                albumQueryData = await albumService.QueryAlbumFrequentAsync(Guid.Parse(id), null,
                    (options.PageIndex - 1) * options.PageItems, options.PageItems, options.SearchText,
                    HttpContext.RequestAborted);
                break;
            case "recent":
                albumQueryData = await albumService.QueryAlbumRecentAsync(Guid.Parse(id), null,
                    (options.PageIndex - 1) * options.PageItems, options.PageItems, options.SearchText,
                    HttpContext.RequestAborted);
                break;
            case "alphabeticalByName":
                albumQueryData = await albumService.QueryAlbumOrderedByAlbumTitleAsync(Guid.Parse(id), null,
                    (options.PageIndex - 1) * options.PageItems, options.PageItems, options.SearchText,
                    HttpContext.RequestAborted);
                break;
            case "alphabeticalByArtist":
                albumQueryData = await albumService.QueryAlbumOrderedByArtistNameAsync(Guid.Parse(id), null,
                    (options.PageIndex - 1) * options.PageItems, options.PageItems, options.SearchText,
                    HttpContext.RequestAborted);
                break;
            case "starred":
                albumQueryData = await albumService.QueryAlbumStarredAsync(Guid.Parse(id), null,
                    (options.PageIndex - 1) * options.PageItems, options.PageItems, options.SearchText,
                    HttpContext.RequestAborted);
                break;
            default:
                return ResultVo.Fail(500, "分类错误");
        }

        var test = albumQueryData.Items?.Select(x => x.CreateAlbumId3());
        
        return ResultVo.Success(data: new QueryData<AlbumID3>()
        {
            Items = albumQueryData.Items?.Select(x => x.CreateAlbumId3()),
            TotalCount = albumQueryData.TotalCount
        });
    }
}