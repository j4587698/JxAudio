using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers.Admin;

[Route("/Admin/[controller]/[action]")]
public class CultureController : Controller
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="redirectUri"></param>
    /// <returns></returns>
    public IActionResult SetCulture(string culture, string redirectUri)
    {
        if (string.IsNullOrEmpty(culture))
        {
            HttpContext.Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
        }
        else
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)), new CookieOptions()
                {
                    Expires = DateTimeOffset.Now.AddYears(1)
                });
        }

        return LocalRedirect(redirectUri);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="redirectUri"></param>
    /// <returns></returns>
    public IActionResult ResetCulture(string redirectUri)
    {
        HttpContext.Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);

        return LocalRedirect(redirectUri);
    }
}