using System.Xml.Serialization;
using JxAudio.Resolver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JxAudio.Utils;

public class Constant
{
    
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