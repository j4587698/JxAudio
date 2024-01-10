using System.Text;
using System.Xml;
using System.Xml.Serialization;
using JxAudio.Enums;
using JxAudio.Extensions;
using JxAudio.Resolver;
using JxAudio.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JxAudio.Controller;

[Route("/rest")]
public partial class AudioController : Microsoft.AspNetCore.Mvc.Controller
{
    private const string _apiVersion = "1.16.0";

    private const int _apiMajorVersion = 1;

    private const int _apiMinorVersion = 16;

    private static readonly object _suffixKey = new object();

    private static readonly object _pathExtensionKey = new object();

    private static readonly object _apiContextKey = new object();

    private static readonly Encoding _encoding = new UTF8Encoding(false);

    private static readonly Lazy<XmlSerializer> _xmlSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Subsonic.Response)));

    private static readonly Lazy<JsonSerializer> _jsonSerializer = new Lazy<JsonSerializer>(() =>
    {
        var serializer = JsonSerializer.Create();
        serializer.ContractResolver = XmlSerializationContractResolver.Instance;
        serializer.Converters.Add(new StringEnumConverter());
#if DEBUG
        serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
#endif
        return serializer;
    });
    
    
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            var resultContext = await next();
        }
        catch (RestApiErrorException ex)
        {
            await WriteErrorResponseAsync(context.HttpContext, ex.Code, ex.Message).ConfigureAwait(false);
        }
        

        // Do something after the action executes.
        // 在动作执行之后做一些处理。
    }
    
    public static async Task WriteResponseAsync(HttpContext context, Subsonic.Response response)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            context.Response.SetDate(now);
            context.Response.SetExpires(now);

            var apiContext = (ApiContext?)context.Items[_apiContextKey];
            if (apiContext == null)
            {
                throw new InvalidOperationException("ApiContext is null!");
            }

            switch (apiContext.Format ?? ResponseFormat.Xml)
            {
                case ResponseFormat.Xml:
                {
                    context.Response.ContentType = "text/xml; charset=utf-8";

                    using (var writer = new XmlTextWriter(context.Response.Body, _encoding))
                    {
                        _xmlSerializer.Value.Serialize(writer, response);
                    }
                    break;
                }
                case ResponseFormat.Json:
                {
                    context.Response.ContentType = "application/json; charset=utf-8";

                    using (var writer = new StreamWriter(context.Response.Body, _encoding))
                    {
                        await writer.WriteAsync(@"{""subsonic-response"":".AsMemory(), context.RequestAborted).ConfigureAwait(false);
                        _jsonSerializer.Value.Serialize(writer, response);
                        await writer.WriteAsync("}".AsMemory(), context.RequestAborted).ConfigureAwait(false);
                    }
                    break;
                }
                case ResponseFormat.JsonPadding:
                {
                    context.Response.ContentType = "application/javascript; charset=utf-8";

                    using (var writer = new StreamWriter(context.Response.Body, _encoding))
                    {
                        await writer.WriteAsync(apiContext.FormatCallback.AsMemory(), context.RequestAborted).ConfigureAwait(false);
                        await writer.WriteAsync(@"({""subsonic-response"":".AsMemory(), context.RequestAborted).ConfigureAwait(false);
                        _jsonSerializer.Value.Serialize(writer, response);
                        await writer.WriteAsync("});".AsMemory(), context.RequestAborted).ConfigureAwait(false);
                    }
                    break;
                }
                default:
                    throw new InvalidOperationException("Unreachable!");
            }
        }

    public static Task WriteResponseAsync(HttpContext context, Subsonic.ItemChoiceType itemType, object? item)
        {
            return WriteResponseAsync(context, new Subsonic.Response()
            {
                status = Subsonic.ResponseStatus.ok,
                version = _apiVersion,
                ItemElementName = itemType,
                Item = item,
            });
        }

    public static Task WriteErrorResponseAsync(HttpContext context, int code, string message)
        {
            return WriteResponseAsync(context, new Subsonic.Response()
            {
                status = Subsonic.ResponseStatus.failed,
                version = _apiVersion,
                ItemElementName = Subsonic.ItemChoiceType.error,
                Item = new Subsonic.Error()
                {
                    code = code,
                    message = message,
                },
            });
        }
}