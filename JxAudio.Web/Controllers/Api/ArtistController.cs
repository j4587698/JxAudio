using System.Security.Claims;
using BootstrapBlazor.Components;
using JxAudio.Core;
using JxAudio.Core.Entity;
using JxAudio.Core.Service;
using JxAudio.TransVo;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Constants = JxAudio.Core.Constants;
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
    public async Task<object> Get(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var album = await artistService.GetArtistById(id, Guid.Parse(userId), HttpContext.RequestAborted);
        return ResultVo.Success(data: album.Adapt<ArtistVo>());
    }
    
    [Authorize]
    public async Task<object> GetAllTracks(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var tracks = await artistService.GetTracksByArtistIdAsync(id, Guid.Parse(userId), HttpContext.RequestAborted);
        return ResultVo.Success(data: tracks.Adapt<List<TrackVo>>());
    }
    
    [Authorize]
    public async Task<object> GetStar(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        await artistService.StarArtistAsync(Guid.Parse(userId), [id], HttpContext.RequestAborted);
        return ResultVo.Success(data: "s");
    }

    [Authorize]
    public async Task<object> GetUnStar(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        await artistService.UnStarArtistAsync(Guid.Parse(userId), [id], HttpContext.RequestAborted);
        return ResultVo.Success(data: "s");
    }
    
    [Authorize]
    public async Task<IActionResult> GetAvatar(int? id)
    {
        if (id is null or 0)
        {
            return File(Constants.GetDefaultAvatar(), "image/png");
        }
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var artist = await artistService.GetArtistById(id.Value, Guid.Parse(userId), HttpContext.RequestAborted);
        if (artist == null || artist.PictureId == 0)
        {
            return File(Constants.GetDefaultAvatar(), "image/png");
        }
        
        var picture = await PictureEntity.FindAsync(artist.PictureId);
        if (picture == null)
        {
            return File(Constants.GetDefaultCover(), "image/png");
        }
        var coverPath = Path.Combine(AppContext.BaseDirectory, Constants.CoverCachePath, picture.Path!);
        if (!System.IO.File.Exists(coverPath))
        {
            return File(Constants.GetDefaultCover(), "image/png");
        }
        
        return File(await System.IO.File.ReadAllBytesAsync(coverPath), picture.MimeType!);
    }
}