using Jx.Toolbox.Utils;
using JxAudio.Core;
using JxAudio.Core.Entity;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Admin;

public class CoverController: ControllerBase
{
    
    public IActionResult GetCover(long? coverId)
    {
        if (coverId is null or 0)
        {
            return File(Path.Combine("Images", "NoCover.png"), "image/png");
        }

        var picture = PictureEntity.Find(coverId.Value);
        if (picture == null)
        {
            return File(Path.Combine("Images", "NoCover.png"), "image/png");
        }
        var coverPath = Path.Combine(AppContext.BaseDirectory, Constants.CoverCachePath, picture.Path!);
        return File(System.IO.File.ReadAllBytes(coverPath), picture.MimeType!);
    }
}