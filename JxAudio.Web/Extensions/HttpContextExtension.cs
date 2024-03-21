using System.Text;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;
using JxAudio.Extensions;
using JxAudio.Utils;
using JxAudio.Web.Enums;
using JxAudio.Web.Utils;
using JxAudio.Core;

namespace JxAudio.Web.Extensions;

public static class HttpContextExtension
{
    
    public static string? GetOptionalStringParameterValue(this HttpContext context, string name)
    {
        var values = context.Request.Query[name];
        if (values.Count == 0)
            return null;
        if (values.Count > 1)
            throw RestApiErrorException.GenericError($"Specified multiple values for '{name}'.");
        return values;
    }
    
    public static string? GetRequiredStringParameterValue(this HttpContext context, string name)
    {
        var values = context.Request.Query[name];
        if (values.Count == 0)
            throw RestApiErrorException.RequiredParameterMissingError(name);
        if (values.Count > 1)
            throw RestApiErrorException.GenericError($"Specified multiple values for '{name}'.");
        return values;
    }
    
    public static async Task<ApiContext> CreateApiContextAsync(this HttpContext context)
        {
            var apiContext = new ApiContext();

            string? format = GetOptionalStringParameterValue(context, "f");
            switch (format)
            {
                case null:
                    break;
                case "xml":
                {
                    apiContext.Format = ResponseFormat.Xml;
                    break;
                }
                case "json":
                {
                    apiContext.Format = ResponseFormat.Json;
                    break;
                }
                case "jsonp":
                {
                    string? callback = GetOptionalStringParameterValue(context, "callback");
                    if (string.IsNullOrEmpty(callback))
                        callback = "callback";

                    apiContext.Format = ResponseFormat.JsonPadding;
                    apiContext.FormatCallback = callback;
                    break;
                }
                default:
                    throw RestApiErrorException.GenericError("Unknown response format requested.");
            }

            apiContext.Client = GetRequiredStringParameterValue(context, "c");

            apiContext.Version = GetRequiredStringParameterValue(context, "v");

            if (!Util.TryParseVersion(apiContext.Version, out int majorVersion, out int minorVersion))
            {
                throw RestApiErrorException.GenericError("Invalid value for 'v'.");
            }
            else
            {
                apiContext.MajorVersion = majorVersion;
                apiContext.MinorVersion = minorVersion;
            }

            if (apiContext.MajorVersion < Constant.ApiMajorVersion)
                throw RestApiErrorException.ClientMustUpgradeError();
            if (apiContext.MajorVersion > Constant.ApiMajorVersion)
                throw RestApiErrorException.ServerMustUpgradeError();
            if (apiContext.MinorVersion > Constant.ApiMinorVersion)
                throw RestApiErrorException.ServerMustUpgradeError();

            var username = GetRequiredStringParameterValue(context, "u");
            var password = GetOptionalStringParameterValue(context, "p");
            var passwordToken = GetOptionalStringParameterValue(context, "t");
            var passwordSalt = GetOptionalStringParameterValue(context, "s");

            var user = await UserEntity
                .Where(u => u.UserName == username)
                .FirstAsync().ConfigureAwait(false);

            bool passwordIsWrong;

            if (password != null)
            {
                password = Util.HexDecodePassword(password);

                if (passwordToken != null || passwordSalt != null)
                    throw RestApiErrorException.GenericError("Specified values for both 'p' and 't' and/or 's'.");

                string? userPassword = user != null ? user.Password : string.Empty;

                passwordIsWrong = !ConstantTimeComparisons.ConstantTimeEquals(password, userPassword);
            }
            else
            {
                if (passwordToken == null)
                    throw RestApiErrorException.RequiredParameterMissingError("t");

                if (!Util.TryParseHexBytes(passwordToken, out byte[]? passwordTokenBytes))
                    throw RestApiErrorException.GenericError("Invalid value for 't'.");
                if (passwordTokenBytes?.Length != 16)
                    throw RestApiErrorException.GenericError("Invalid value for 't'.");

                if (passwordSalt == null)
                    throw RestApiErrorException.RequiredParameterMissingError("s");

                string? userPassword = user != null ? user.Password : string.Empty;

                // This security mechanism is pretty terrible.  It is vulnerable to both
                // timing and replay attacks.

#pragma warning disable CA5351 // Do not use insecure cryptographic algorithm MD5.
                using var md5 = System.Security.Cryptography.MD5.Create();
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(userPassword + passwordSalt));
                passwordIsWrong = !ConstantTimeComparisons.ConstantTimeEquals(passwordTokenBytes, hash);
            }

            // Check if user exists after checking password to prevent discovery of existing users
            // by timing attack.
            if (user == null || (!user.IsGuest && passwordIsWrong))
                throw RestApiErrorException.WrongUsernameOrPassword();

            apiContext.User = user;

            apiContext.Suffix = context.Items.TryGetValue(Constant.SuffixKey, out object? suffix) ? suffix?.ToString() : "mp3";

            return apiContext;
        }
    
    internal static Task WriteResponseAsync(this HttpContext context, ItemChoiceType itemType, object? item)
    {
        return WriteResponseAsync(context, new Response()
        {
            status = ResponseStatus.ok,
            version = Constant.ApiVersion,
            ItemElementName = itemType,
            Item = item,
        });
    }

    public static async Task WriteResponseAsync(this HttpContext context, Response response)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        context.Response.SetDate(now);
        context.Response.SetExpires(now);

        var apiContext = (ApiContext?)context.Items[Constant.ApiContextKey];

        switch (apiContext?.Format ?? ResponseFormat.Xml)
        {
            case ResponseFormat.Xml:
            {
                context.Response.ContentType = "text/xml; charset=utf-8";

                await Util.SerializeToHttpResponseAsync(context.Response, response);

                break;
            }
            case ResponseFormat.Json:
            {
                context.Response.ContentType = "application/json; charset=utf-8";

                await using var writer = new StreamWriter(context.Response.Body, Encoding.UTF8);
                await writer.WriteAsync(@"{""subsonic-response"":".AsMemory(), context.RequestAborted)
                    .ConfigureAwait(false);
                Constant.LazyJsonSerializer.Value.Serialize(writer, response);
                await writer.WriteAsync("}".AsMemory(), context.RequestAborted).ConfigureAwait(false);

                break;
            }
            case ResponseFormat.JsonPadding:
            {
                context.Response.ContentType = "application/javascript; charset=utf-8";

                await using var writer = new StreamWriter(context.Response.Body, Encoding.UTF8);
                await writer.WriteAsync(apiContext!.FormatCallback!.AsMemory(), context.RequestAborted)
                    .ConfigureAwait(false);
                await writer.WriteAsync(@"({""subsonic-response"":".AsMemory(), context.RequestAborted)
                    .ConfigureAwait(false);
                Constant.LazyJsonSerializer.Value.Serialize(writer, response);
                await writer.WriteAsync("});".AsMemory(), context.RequestAborted).ConfigureAwait(false);

                break;
            }
            default:
                throw new InvalidOperationException("Unreachable!");
        }
    }
    
    public static Task WriteErrorResponseAsync(this HttpContext context, int code, string message)
    {
        return WriteResponseAsync(context, new Response()
        {
            status = ResponseStatus.failed,
            version = Constant.ApiVersion,
            ItemElementName = ItemChoiceType.error,
            Item = new Error()
            {
                code = code,
                message = message,
            },
        });
    }
}