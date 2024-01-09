using System.Xml;
using JxAudio.Utils;

namespace JxAudio.Extensions;

public static class HttpContextExtension
{
    internal static Task WriteResponseAsync(this HttpContext context, Subsonic.ItemChoiceType itemType, object item)
    {
        
    }
    
    internal static async Task WriteResponseAsync(this HttpContext context, Subsonic.Response response)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            context.Response.SetDate(now);
            context.Response.SetExpires(now);

            var apiContext = (ApiContext)context.Items[Constant.ApiContextKey];

            switch (apiContext?.Format ?? ResponseFormat.Xml)
            {
                case ResponseFormat.Xml:
                {
                    context.Response.ContentType = "text/xml; charset=utf-8";

                    using (var writer = new XmlTextWriter(context.Response.Body, _encoding))
                    {
                        Constant.LazyXmlSerializer.Value.Serialize(writer, response);
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
}