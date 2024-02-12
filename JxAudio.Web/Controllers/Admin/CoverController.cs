using Jx.Toolbox.Utils;
using JxAudio.Core;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Admin;

public class CoverController: ControllerBase
{
    public IActionResult GetCover(string path)
    {
        var coverPath = Path.Combine(AppContext.BaseDirectory, Constants.CoverCachePath, path);
        return File(System.IO.File.ReadAllBytes(coverPath), Mime.GetMimeFromExtension(Path.GetExtension(path)));
    }
}