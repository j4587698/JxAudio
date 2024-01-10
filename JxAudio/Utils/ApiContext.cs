using JxAudio.Enums;
using JxAudio.Subsonic;

namespace JxAudio.Utils;

public sealed class ApiContext
{
    public ResponseFormat? Format;
    public string? FormatCallback;

    public string? Version;
    public int MajorVersion;
    public int MinorVersion;

    public string? Client;

    public User? User;

    public string? Suffix;
}