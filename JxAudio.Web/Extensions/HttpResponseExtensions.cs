using System.Globalization;
using Microsoft.Net.Http.Headers;

namespace JxAudio.Extensions;

public static class HttpResponseExtensions
{
    internal static void SetDate(this HttpResponse response, DateTimeOffset time)
    {
        response.Headers[HeaderNames.Date] = time.ToString("r", DateTimeFormatInfo.InvariantInfo);
    }

    internal static void SetExpires(this HttpResponse response, DateTimeOffset time)
    {
        response.Headers[HeaderNames.Expires] = time.ToString("r", DateTimeFormatInfo.InvariantInfo);
    }

    internal static void SetLastModified(this HttpResponse response, DateTimeOffset time)
    {
        response.Headers[HeaderNames.LastModified] = time.ToString("r", DateTimeFormatInfo.InvariantInfo);
    }
}