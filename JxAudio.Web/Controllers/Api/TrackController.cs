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
        
        var track = await trackService.GetSongEntityAsync(Guid.Parse(id), trackId, HttpContext.RequestAborted);
            var providerPlugin = Constant.GetProvider(track.ProviderId);
            if (providerPlugin == null)
            {
                return new NotFoundResult();
            }

            var info = await providerPlugin.GetFileInfoAsync(track.FullName!);

            if (info == null)
            {
                return new NotFoundResult();
            }

            var stream = await providerPlugin.GetFileAsync(track.FullName!);
            if (stream == null)
            {
                return new NotFoundResult();
            }

            var now = DateTimeOffset.Now;
            HttpContext.Response.SetDate(now);
            HttpContext.Response.SetExpires(now);
            HttpContext.Response.SetLastModified(info.ModifyTime);

            maxBitRate = maxBitRate == 0 ? apiContext!.User!.MaxBitRate
                : apiContext!.User!.MaxBitRate == 0 ? maxBitRate : Math.Min(maxBitRate.Value, apiContext.User.MaxBitRate);

            format ??= apiContext.Suffix;
            
            switch (format)
            {
                case "aac":
                    HttpContext.Response.ContentType = "audio/aac";
                    maxBitRate = maxBitRate == 0 ? 160 : Math.Min(Math.Max(maxBitRate.Value, 16), Math.Min(320, track.BitRate ?? 999));
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                        .OutputToPipe(new StreamPipeSink(HttpContext.Response.Body), options =>
                        {
                            options.WithAudioCodec(AudioCodec.Aac)
                                .WithAudioBitrate(maxBitRate.Value)
                                .ForceFormat("adts")
                                .WithCustomArgument("-map a -map_metadata -1"); 
                        }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                    break;
                case "mp3":
                    HttpContext.Response.ContentType = "audio/mpeg";
                    maxBitRate = maxBitRate == 0 ? 256 : Math.Min(Math.Max(maxBitRate.Value, 32), Math.Min(320, track.BitRate ?? 999));
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                        .OutputToPipe(new StreamPipeSink(HttpContext.Response.Body), options =>
                        {
                            options.WithAudioCodec(AudioCodec.LibMp3Lame)
                                .WithAudioBitrate(maxBitRate.Value)
                                .ForceFormat("mp3")
                                .WithCustomArgument("-map a -map_metadata -1"); 
                        }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                    break;
                case "oga":
                case "ogg":
                    HttpContext.Response.ContentType = "audio/ogg";
                    maxBitRate = maxBitRate == 0 ? 192 : Math.Min(Math.Max(maxBitRate.Value, 45), Math.Min(500, track.BitRate ?? 999));
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                        .OutputToPipe(new StreamPipeSink(HttpContext.Response.Body), options =>
                        {
                            options.WithAudioCodec(AudioCodec.LibVorbis)
                                .WithAudioBitrate(maxBitRate.Value)
                                .ForceFormat("ogg")
                                .WithCustomArgument("-map a -map_metadata -1"); 
                        }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                    break;
                case "opus":
                    HttpContext.Response.ContentType = "audio/opus";
                    maxBitRate = maxBitRate == 0 ? 128 : Math.Min(Math.Max(maxBitRate.Value, 6), Math.Min(450, track.BitRate ?? 999));
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                        .OutputToPipe(new StreamPipeSink(HttpContext.Response.Body), options =>
                        {
                            options.WithAudioCodec(FFMpeg.GetCodec("opus"))
                                .WithAudioBitrate(maxBitRate.Value)
                                .ForceFormat("opus")
                                .WithCustomArgument("-map a -map_metadata -1 -strict -2"); 
                        }).CancellableThrough(HttpContext.RequestAborted).ProcessAsynchronously();
                    break;
                case "raw":
                    HttpContext.Response.ContentType = track.MimeType;
                    HttpContext.Response.ContentLength = track.Size;
                    await stream.CopyToAsync(HttpContext.Response.Body, HttpContext.RequestAborted);
                    break;
                default:
                    throw RestApiErrorException.GenericError("Specified value for 'format' is not supported.");
            }
    }
}