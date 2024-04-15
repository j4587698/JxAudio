using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JxAudio.Core;

public static class Application
{
    public static IServiceProvider? ServiceProvider { get; internal set; }
    
    [NotNull]
    public static IWebHostEnvironment? WebHostEnvironment { get; internal set; }
    
    [NotNull]
    public static ConfigurationManager? Configuration { get; internal set; }

    public static T? GetService<T>() where T: class
    {
        var service = GetRequiredService<T>();
        if (service != null)
        {
            return service;
        }
        return ServiceProvider?.GetService<T>();
    }

    public static T? GetRequiredService<T>() where T : class
    {
        return ServiceProvider?.GetRequiredService<T>();
    }

    public static string? GetValue(string key)
    {
        return Configuration[key];
    }

    public static T? GetValue<T>(string key) where T: class
    {
        return Configuration.GetSection(key).Get<T>();
    }
}