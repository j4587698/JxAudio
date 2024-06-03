using System.Security.Claims;
using BootstrapBlazor.Components;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.Extensions;
using JxAudio.TransVo;
using JxAudio.Web.Utils;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResultVo = JxAudio.Web.Vo.ResultVo;
using DynamicFilterInfo = FreeSql.Internal.Model.DynamicFilterInfo;

namespace JxAudio.Web.Controllers.Api;

public class TrackController(TrackService trackService, UserService userService): DynamicControllerBase
{

    [Authorize]
    public async Task<object> Post([FromBody] QueryOptionsVo queryOptionsVo)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var queryAsync = await trackService.QueryData(queryOptionsVo.Adapt<QueryPageOptions>()!,
            queryOptionsVo.DynamicFilterInfo.Adapt<DynamicFilterInfo>()!, Guid.Parse(id), false);
        return ResultVo.Success(data: new QueryData<TrackVo>()
        {
            Items = queryAsync.Items?.Select(x => x.Adapt<TrackVo>()),
            TotalCount = queryAsync.TotalCount,
            IsAdvanceSearch = true,
            IsFiltered = true,
            IsSearch = true,
            IsSorted = true
        });
    }
    
    [Authorize]
    public async Task<object> PostStar([FromBody] QueryOptionsVo queryOptionsVo)
    {
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var queryAsync = await trackService.QueryData(queryOptionsVo.Adapt<QueryPageOptions>()!,
            queryOptionsVo.DynamicFilterInfo.Adapt<DynamicFilterInfo>()!, Guid.Parse(id), true);
        return ResultVo.Success(data: new QueryData<TrackVo>()
        {
            Items = queryAsync.Items?.Select(x => x.Adapt<TrackVo>()),
            TotalCount = queryAsync.TotalCount,
            IsAdvanceSearch = true,
            IsFiltered = true,
            IsSearch = true,
            IsSorted = true
        });
    }

    [Authorize]
    public async Task<object> GetStar(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        await trackService.StarTrackAsync(Guid.Parse(userId), [id], HttpContext.RequestAborted);
        return ResultVo.Success(data: "s");
    }

    [Authorize]
    public async Task<object> GetUnStar(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        await trackService.UnStarTrackAsync(Guid.Parse(userId), [id], HttpContext.RequestAborted);
        return ResultVo.Success(data: "s");
    }
    
    [Authorize]
    public async Task<IActionResult> GetStream(int trackId, int? maxBitRate, string? format)
    {
        maxBitRate ??= 0;
        var id = HttpContext.User.FindFirst(ClaimTypes.Sid)!.Value;
        var user = await userService.GetUserById(Guid.Parse(id));
        if (user == null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return new NotFoundResult();
        }

        var track = await trackService.GetSongEntityAsync(user.Id, trackId, HttpContext.RequestAborted);
        var providerPlugin = Constant.GetProvider(track.ProviderId);
        if (providerPlugin == null)
        {
            return new NotFoundResult();
        }

        var info = await providerPlugin.GetFileInfoAsync(track.FullName!);

        if (info == null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return new NotFoundResult();
        }

        var stream = await providerPlugin.GetFileAsync(track.FullName!);
        if (stream == null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return new NotFoundResult();
        }

        var now = DateTimeOffset.Now;
        HttpContext.Response.SetDate(now);
        HttpContext.Response.SetExpires(now);
        HttpContext.Response.SetLastModified(info.ModifyTime);

        maxBitRate = maxBitRate == 0 ? user.MaxBitRate
            : user.MaxBitRate == 0 ? maxBitRate : Math.Min(maxBitRate.Value, user.MaxBitRate);

        format ??= "mp3";

        if (string.Equals(Path.GetExtension(track.Name)?.TrimStart('.'), format, StringComparison.CurrentCultureIgnoreCase)
            && maxBitRate >= track.BitRate)
        {
            return File(stream, track.MimeType!, enableRangeProcessing: true);
        }

        switch (format)
        {
            case "aac":
                HttpContext.Response.ContentType = "audio/aac";
                maxBitRate = maxBitRate == 0
                    ? 160
                    : Math.Min(Math.Max(maxBitRate.Value, 16), Math.Min(320, track.BitRate ?? 999));
                await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                    .OutputToPipe(new StreamPipeSink(Response.Body), options =>
                    {
                        options.WithAudioCodec(AudioCodec.Aac)
                            .WithAudioBitrate(maxBitRate.Value)
                            .ForceFormat("adts")
                            .WithoutMetadata()
                            .WithCustomArgument($"-map a");
                    }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                break;
            case "mp3":
                HttpContext.Response.ContentType = "audio/mpeg";
                maxBitRate = maxBitRate == 0
                    ? 256
                    : Math.Min(Math.Max(maxBitRate.Value, 32), Math.Min(320, track.BitRate ?? 999));
                await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                    .OutputToPipe(new StreamPipeSink(HttpContext.Response.Body), options =>
                    {
                        options.WithAudioCodec(AudioCodec.LibMp3Lame)
                            .WithAudioBitrate(maxBitRate.Value)
                            .ForceFormat("mp3")
                            .WithoutMetadata()
                            .WithCustomArgument($"-map a");
                    }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                break;
            case "oga":
            case "ogg":
                HttpContext.Response.ContentType = "audio/ogg";
                maxBitRate = maxBitRate == 0
                    ? 192
                    : Math.Min(Math.Max(maxBitRate.Value, 45), Math.Min(500, track.BitRate ?? 999)); ;
                await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                    .OutputToPipe(new StreamPipeSink(HttpContext.Response.Body), options =>
                    {
                        options.WithAudioCodec(AudioCodec.LibVorbis)
                            .WithAudioBitrate(maxBitRate.Value)
                            .ForceFormat("ogg")
                            .WithoutMetadata()
                            .WithCustomArgument($"-map a");
                    }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                break;
            case "opus":
                HttpContext.Response.ContentType = "audio/opus";
                maxBitRate = maxBitRate == 0
                    ? 128
                    : Math.Min(Math.Max(maxBitRate.Value, 6), Math.Min(450, track.BitRate ?? 999));
                await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                    .OutputToPipe(new StreamPipeSink(HttpContext.Response.Body), options =>
                    {
                        options.WithAudioCodec(FFMpeg.GetCodec("opus"))
                            .WithAudioBitrate(maxBitRate.Value)
                            .ForceFormat("opus")
                            .WithoutMetadata()
                            .WithCustomArgument($"-map a -strict -2");
                    }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                break;
            case "raw":
                return File(stream, track.MimeType!, enableRangeProcessing: true);
            default:
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new NotFoundResult();
        }

        return new EmptyResult();
    }
}