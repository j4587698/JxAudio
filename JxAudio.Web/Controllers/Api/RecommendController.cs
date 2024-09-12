using System.Security.Claims;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.TransVo;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

[Authorize]
public class RecommendController(
    AlbumService albumService,
    TrackService trackService
    ): DynamicControllerBase
{
    public async Task<object> GetNewestAlbum(int offset = 0, int count = 10)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var songs = await albumService.QueryAlbumNewestAsync(Guid.Parse(userId), null, 
            offset, count, HttpContext.RequestAborted);
        return ResultVo.Success(data: songs.Items?.Select(x => x.Adapt<AlbumVo>()));
    }
    
    public async Task<object> GetFrequentAlbum(int offset = 0, int count = 10)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var songs = await albumService.QueryAlbumFrequentAsync(Guid.Parse(userId), null, 
            offset, count, HttpContext.RequestAborted);
        return ResultVo.Success(data: songs.Items?.Select(x => x.Adapt<AlbumVo>()));
    }
    
    public async Task<object> GetRecentAlbum(int offset = 0, int count = 10)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var songs = await albumService.QueryAlbumRecentAsync(Guid.Parse(userId), null, 
            offset, count, HttpContext.RequestAborted);
        return ResultVo.Success(data: songs.Items?.Select(x => x.Adapt<AlbumVo>()));
    }
    
    public async Task<object> GetRandomAlbum(int count = 10)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var songs = await albumService.QueryAlbumRandomAsync(Guid.Parse(userId), null,
            count, HttpContext.RequestAborted);
        return ResultVo.Success(data: songs.Items?.Select(x => x.Adapt<AlbumVo>()));
    }
    
    public async Task<object> GetNewestTrack(int offset = 0, int count = 10)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var songs = await trackService.QueryNewestTrackAsync(Guid.Parse(userId), null, 
            offset, count, HttpContext.RequestAborted);
        return ResultVo.Success(data: songs.Items?.Select(x => x.Adapt<TrackVo>()));
    }
    
    public async Task<object> GetFrequentTrack(int offset = 0, int count = 10)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var songs = await trackService.QueryFrequentTrackAsync(Guid.Parse(userId), null, 
            offset, count, HttpContext.RequestAborted);
        return ResultVo.Success(data: songs.Items?.Select(x => x.Adapt<TrackVo>()));
    }
    
    public async Task<object> GetRecentTrack(int offset = 0, int count = 10)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var songs = await trackService.QueryRecentTrackAsync(Guid.Parse(userId), null, 
            offset, count, HttpContext.RequestAborted);
        return ResultVo.Success(data: songs.Items?.Select(x => x.Adapt<TrackVo>()));
    }
    
    public async Task<object> GetRandomTrack(int count = 10)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var songs = await trackService.QueryRandomTrackAsync(Guid.Parse(userId), null, 
            count, HttpContext.RequestAborted);
        return ResultVo.Success(data: songs.Items?.Select(x => x.Adapt<TrackVo>()));
    }
}