using System.Xml.Serialization;
using JxAudio.Plugin;
using JxAudio.Resolver;
using LocalFileProviderPlugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JxAudio.Web.Utils;

public static class Constant
{
    public static List<IProviderPlugin> ProviderPlugins = new List<IProviderPlugin>()
    {
        new LocalFileProvider()
    };
    
    public static readonly List<string> AudioExtensions = new List<string>
    {
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
        ".m4v",
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
    };
    
    
    public const string SuffixKey = "SuffixKey";
    
    public const string PathExtensionKey = "PathExtensionKey";
    
    public const string ApiContextKey = "ApiContextKey";
    
    public const string ApiVersion = "1.16.0";

    public const int ApiMajorVersion = 1;

    public const int ApiMinorVersion = 16;

    public static readonly Lazy<XmlSerializer> LazyXmlSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Subsonic.Response)));

    public static Lazy<JsonSerializer> LazyJsonSerializer = new Lazy<JsonSerializer>(() =>
    {
        var serializer = JsonSerializer.Create();
        serializer.ContractResolver = XmlSerializationContractResolver.Instance;
        serializer.Converters.Add(new StringEnumConverter());
#if DEBUG
        serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
#endif
        return serializer;
    });
    
}