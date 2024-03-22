using System.Diagnostics.CodeAnalysis;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using JxAudio.Core;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Extensions;
using JxAudio.Utils;
using JxAudio.Web.Extensions;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class MediaRetrievalController(PictureService pictureService, TrackService trackService): AudioController
{
    [HttpGet("/stream")]
    public async Task Stream(string? id, int? maxBitRate, string? format, string? timeOffset, string? timeEnd, string? size)
    {
        maxBitRate ??= 0;

        var trackId = id.ParseTrackId();
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var track = await trackService.GetSongEntityAsync(apiUserId.Value, trackId, HttpContext.RequestAborted);
            var providerPlugin = Constant.ProviderPlugins.FirstOrDefault(x => x.Id == track.ProviderId);
            if (providerPlugin == null)
            {
                throw RestApiErrorException.GenericError("Provider not found");
            }

            var info = await providerPlugin.GetFileInfoAsync(track.FullName!);

            if (info == null)
            {
                throw RestApiErrorException.GenericError("File not found");
            }

            var stream = await providerPlugin.GetFileAsync(track.FullName!);
            if (stream == null)
            {
                throw RestApiErrorException.GenericError("File not found");
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

    [HttpGet("/download")]
    public async Task Download(string? id)
    {
        var trackId = id.ParseTrackId();
        
        var apiContext = HttpContext.Items[Constant.ApiContextKey] as ApiContext;
        var apiUserId = apiContext?.User?.Id;
        if (apiUserId != null)
        {
            var track = await trackService.GetSongEntityAsync(apiUserId.Value, trackId, HttpContext.RequestAborted);
            var providerPlugin = Constant.ProviderPlugins.FirstOrDefault(x => x.Id == track.ProviderId);
            if (providerPlugin == null)
            {
                throw RestApiErrorException.GenericError("Provider not found");
            }

            var info = await providerPlugin.GetFileInfoAsync(track.FullName!);

            if (info == null)
            {
                throw RestApiErrorException.GenericError("File not found");
            }

            if (info.ModifyTime < HttpContext.Request.GetIfModifiedSince()?.AddSeconds(1))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;
                return;
            }

            var stream = await providerPlugin.GetFileAsync(track.FullName!);
            if (stream == null)
            {
                throw RestApiErrorException.GenericError("File not found");
            }

            var now = DateTime.UtcNow;
            HttpContext.Response.SetDate(now);
            HttpContext.Response.SetLastModified(info.ModifyTime);
            HttpContext.Response.SetExpires(now);
            HttpContext.Response.ContentType = track.MimeType;
            HttpContext.Response.ContentLength = track.Size;
            HttpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{track.Name}\"");
            await stream.CopyToAsync(HttpContext.Response.Body, HttpContext.RequestAborted);
        }
    }

    [HttpGet("/hls")]
    public void Hls()
    {
        throw RestApiErrorException.NotImplemented();
    }

    [HttpGet("/getCaptions")]
    public void GetCaptions()
    {
        throw RestApiErrorException.NotImplemented();
    }

    [HttpGet("/getCoverArt")]
    public async Task GetCoverArt(string? id, int? size)
    {
        Util.CheckRequiredParameters(nameof(id), id);

        var stream = await pictureService.GetPictureAsync(id!, size, HttpContext.RequestAborted);

        if (stream == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        
        HttpContext.Response.ContentType = "image/png";
        await stream.CopyToAsync(HttpContext.Response.Body, HttpContext.RequestAborted);
    }
}