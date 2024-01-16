using JxAudio.Core.Entity;
using JxAudio.Enums;

namespace JxAudio.Utils;

public sealed class ApiContext
{
    public ResponseFormat? Format;
    public string? FormatCallback;

    public string? Version;
    public int MajorVersion;
    public int MinorVersion;

    public string? Client;

    public UserEntity? User;

    public string? Suffix;
}