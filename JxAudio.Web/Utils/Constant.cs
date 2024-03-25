using System.Xml.Serialization;
using JxAudio.Core.Subsonic;
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
    
    public const string SearchTypeKey = "SearchType";
    
    public const string ScanIntervalKey = "ScanInterval";
    
    public const string CronExpressKey = "CronExpress";
    
    public const string TimeUnitKey = "TimeUnit";
    
    
    
    public const string SuffixKey = "SuffixKey";
    
    public const string PathExtensionKey = "PathExtensionKey";
    
    public const string ApiContextKey = "ApiContextKey";
    
    public const string ApiVersion = "1.16.1";

    public const int ApiMajorVersion = 1;

    public const int ApiMinorVersion = 16;

    public static readonly Lazy<XmlSerializer> LazyXmlSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Response)));

    public static readonly Lazy<JsonSerializer> LazyJsonSerializer = new Lazy<JsonSerializer>(() =>
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