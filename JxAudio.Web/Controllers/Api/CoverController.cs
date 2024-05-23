using JxAudio.Core;
using JxAudio.Core.Entity;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Api;

public class CoverController: DynamicControllerBase
{
    public async Task<IActionResult> Get(int? coverId)
    {
        if (coverId is null or 0)
        {
            return File(Constants.GetDefaultCover(), "image/png");
        }

        var picture = await PictureEntity.FindAsync(coverId.Value);
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