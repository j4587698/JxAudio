using JxAudio.Core;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;
using JxAudio.Web.Extensions;
using JxAudio.Web.Jobs;
using Longbow.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class MediaLibraryScanningController: AudioController
{
    [HttpGet("/getScanStatus")]
    public async Task GetScanStatus()
    {
        var count = await TrackEntity.Select.CountAsync(HttpContext.RequestAborted);
        var scanStatus = new ScanStatus()
        {
            scanning = TaskServicesManager.Get(nameof(ScanJob))?.LastRunResult == TriggerResult.Running,
            count = count,
            countSpecified = true,
        };
        await HttpContext.WriteResponseAsync(ItemChoiceType.scanStatus, scanStatus);
    }

    [HttpGet("/startScan")]
    public void StartScan()
    {
        throw RestApiErrorException.UserNotAuthorizedError();
    }
}