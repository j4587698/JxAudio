using System.Reflection;
using FreeSql;

namespace JxAudio.Core;

public static class Constants
{
    public static readonly List<string> AudioExtensions =
    [
        ".aac",
        ".ac3",
        ".aif", ".aiff", ".aifc",
        ".ape",
        ".asf", ".wma", ".wmv",
        ".au", ".snd",
        ".avi",
        ".flac",
        ".flv",
        ".mkv", ".mka", ".mk3d", ".mks",
        ".mov", ".qt",
        ".m4v", ".m4a",
        ".mp3",
        ".mpc",
        ".ogg", ".ogv", ".oga", ".ogx", ".spx", ".opus",
        ".ra", ".rm",
        ".shn",
        ".tak",
        ".tta",
        ".wav",
        ".wv",
        ".xwma",
        ".dsf"
    ];

    public const string CoverCachePath = "config/cache/";

    public static readonly string PluginPath = Path.Combine(Directory.GetCurrentDirectory(), "config/plugins/");

    public static string? AesKey { get; set; }
    
    public static readonly AsyncLocal<IUnitOfWork> AsyncUow = new();
    
    public static Stream GetDefaultCover()
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceStream("JxAudio.Core.Assets.NoCover.png")!;
    }
    
    public static Stream GetDefaultAvatar()
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceStream("JxAudio.Core.Assets.avatar.png")!;
    }
}