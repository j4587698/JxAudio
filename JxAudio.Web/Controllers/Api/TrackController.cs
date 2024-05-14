using System.Security.Claims;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Api;

public class TrackController(TrackService trackService, UserService userService): DynamicControllerBase
{
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
            
            MemoryStream outputStream = new();
            
            switch (format)
            {
                case "aac":
                    maxBitRate = maxBitRate == 0 ? 160 : Math.Min(Math.Max(maxBitRate.Value, 16), Math.Min(320, track.BitRate ?? 999));
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                        .OutputToPipe(new StreamPipeSink(outputStream), options =>
                        {
                            options.WithAudioCodec(AudioCodec.Aac)
                                .WithAudioBitrate(maxBitRate.Value)
                                .ForceFormat("adts")
                                .WithCustomArgument("-map a"); 
                        }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return File(outputStream, "audio/aac", enableRangeProcessing:true);
                case "mp3":
                    maxBitRate = maxBitRate == 0 ? 256 : Math.Min(Math.Max(maxBitRate.Value, 32), Math.Min(320, track.BitRate ?? 999));
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                        .OutputToPipe(new StreamPipeSink(outputStream), options =>
                        {
                            options.WithAudioCodec(AudioCodec.LibMp3Lame)
                                .WithAudioBitrate(maxBitRate.Value)
                                .ForceFormat("mp3")
                                .WithCustomArgument("-map a"); 
                        }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return File(outputStream, "audio/aac", enableRangeProcessing:true);
                case "oga":
                case "ogg":
                    maxBitRate = maxBitRate == 0 ? 192 : Math.Min(Math.Max(maxBitRate.Value, 45), Math.Min(500, track.BitRate ?? 999));
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                        .OutputToPipe(new StreamPipeSink(outputStream), options =>
                        {
                            options.WithAudioCodec(AudioCodec.LibVorbis)
                                .WithAudioBitrate(maxBitRate.Value)
                                .ForceFormat("ogg")
                                .WithCustomArgument("-map a"); 
                        }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return File(outputStream, "audio/mpeg", enableRangeProcessing:true);
                case "opus":
                    maxBitRate = maxBitRate == 0 ? 128 : Math.Min(Math.Max(maxBitRate.Value, 6), Math.Min(450, track.BitRate ?? 999));
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                        .OutputToPipe(new StreamPipeSink(outputStream), options =>
                        {
                            options.WithAudioCodec(FFMpeg.GetCodec("opus"))
                                .WithAudioBitrate(maxBitRate.Value)
                                .ForceFormat("opus")
                                .WithCustomArgument("-map a -strict -2"); 
                        }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return File(outputStream, "audio/opus", enableRangeProcessing:true);
                case "raw":
                    return File(stream, track.MimeType!, enableRangeProcessing:true);
                default:
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return new NotFoundResult();
            }
    }
}