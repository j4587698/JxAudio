using System.Globalization;
using System.Text;
using Jx.Toolbox.Extensions;
using JxAudio.Core;
using JxAudio.Web.Enums;
using JxAudio.Web.Jobs;
using JxAudio.Web.Vo;
using Longbow.Tasks;

namespace JxAudio.Web.Utils;

public static class Util
{
    public static bool IsInstalled = false;

    static Util()
    {
        IsInstalled = File.Exists(Path.Combine(AppContext.BaseDirectory, "config", "install.lock"));
    }
    
    public static string HexDecodePassword(string password)
    {
        if (password.StartsWith("enc:", StringComparison.Ordinal))
            if (TryParseHexBytes(password.AsSpan(4), out var bytes))
                return Encoding.UTF8.GetString(bytes ?? Array.Empty<byte>());

        return password;
    }

    private static bool TryParseBoolean(ReadOnlySpan<char> span, out bool value)
    {
        switch (span)
        {
            case "false":
                value = false;
                return true;
            case "true":
                value = true;
                return true;
            default:
                value = default;
                return false;
        }
    }

    private static bool TryParseHexByte(ReadOnlySpan<char> span, out byte value)
    {
        return byte.TryParse(span, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseInt32(ReadOnlySpan<char> span, out int value)
    {
        return int.TryParse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseInt64(ReadOnlySpan<char> span, out long value)
    {
        return long.TryParse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseHexInt64(ReadOnlySpan<char> span, out long value)
    {
        return long.TryParse(span, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseSingle(ReadOnlySpan<char> span, out float value)
    {
        return float.TryParse(span, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseHexBytes(ReadOnlySpan<char> hex, out byte[]? bytes)
    {
        if (hex.Length % 2 != 0)
        {
            bytes = null;
            return false;
        }

        bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i += 1)
            if (!TryParseHexByte(hex.Slice(i * 2, 2), out bytes[i]))
                return false;
        return true;
    }

    public static bool TryParseVersion(ReadOnlySpan<char> span, out int majorVersion, out int minorVersion)
    {
        var separatorSpan = ".".AsSpan();

        int index = span.IndexOf(separatorSpan, StringComparison.Ordinal);
        if (index == -1)
        {
            majorVersion = default;
            minorVersion = default;
            return false;
        }

        var majorVersionSpan = span.Slice(0, index);

        span = span.Slice(index + separatorSpan.Length);

        index = span.IndexOf(separatorSpan, StringComparison.Ordinal);
        if (index == -1)
            index = span.Length;
        var minorVersionSpan = span.Slice(0, index);

        if (int.TryParse(majorVersionSpan, NumberStyles.None, CultureInfo.InvariantCulture, out majorVersion) &&
            int.TryParse(minorVersionSpan, NumberStyles.None, CultureInfo.InvariantCulture, out minorVersion))
        {
            return true;
        }

        majorVersion = default;
        minorVersion = default;
        return false;
    }

    public static async Task SerializeToHttpResponseAsync(HttpResponse response, object value)
    {
        using var memoryStream = new MemoryStream();
        // Serialize to memory stream synchronously (no I/O happening here, just in-memory processing)
        Constant.LazyXmlSerializer.Value.Serialize(memoryStream, value);
        memoryStream.Seek(0, SeekOrigin.Begin); // Reset memory stream position to the beginning

        // Now write the memory stream to the response body asynchronously
        await memoryStream.CopyToAsync(response.Body);
    }
    
    public static void CheckRequiredParameters<T>(string name, T? value)
    {
        if (value == null)
        {
            throw RestApiErrorException.RequiredParameterMissingError(name);
        }
    }

    public static void StartJob(SettingsVo settingsVo)
    {
        if (settingsVo.SearchType == SearchType.Interval && int.TryParse(settingsVo.ScanInterval, out var scanInterval))
        {
            switch (settingsVo.TimeUnit)
            {
                case TimeUnit.Second:
                    break;
                case TimeUnit.Minute:
                    scanInterval *= 60;
                    break;
                case TimeUnit.Hour:
                    scanInterval *= 60 * 60;
                    break;
                case TimeUnit.Day:
                    scanInterval *= 60 * 60 * 24;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            TaskServicesManager.GetOrAdd<ScanJob>("歌曲扫描任务", TriggerBuilder.Default.WithInterval(scanInterval * 1000).Build());
            return;
        }
        if (settingsVo.SearchType == SearchType.Cron)
        {
            TaskServicesManager.GetOrAdd<ScanJob>("歌曲扫描任务", TriggerBuilder.Build(settingsVo.CronExpress!));
            return;
        }

        throw new Exception("Create Job Error");
    }
    
}