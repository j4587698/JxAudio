using System.Globalization;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace JxAudio.Web.Extensions;

public static class HttpRequestExtension
{
    public static DateTimeOffset? GetIfModifiedSince(this HttpRequest request)
    {
        StringValues values = request.Headers[HeaderNames.IfModifiedSince];
        if (values.Count != 1)
            return null;
        if (!DateTimeOffset.TryParseExact(values, "r", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out var time))
            return null;
        if (time > DateTimeOffset.UtcNow)
            return null;
        return time;
    }
}