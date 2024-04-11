using Jx.Toolbox.Utils;
using JxAudio.Core;
using JxAudio.Core.Entity;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Admin;

public class CoverController: ControllerBase
{
    
    public IActionResult GetCover(int? coverId)
    {
        if (coverId is null or 0)
        {
            return File(Constants.GetDefaultCover(), "image/png");
        }

        var picture = PictureEntity.Find(coverId.Value);
        if (picture == null)
        {
            return File(Constants.GetDefaultCover(), "image/png");
        }
        var coverPath = Path.Combine(AppContext.BaseDirectory, Constants.CoverCachePath, picture.Path!);
        if (!System.IO.File.Exists(coverPath))
        {
            return File(Constants.GetDefaultCover(), "image/png");
        }
        
        return File(System.IO.File.ReadAllBytes(coverPath), picture.MimeType!);
    }
}