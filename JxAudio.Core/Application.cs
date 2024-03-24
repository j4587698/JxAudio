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
        return ServiceProvider?.GetService<T>();
    }

    public static T? GetRequiredService<T>() where T : class
    {
        return ServiceProvider?.GetRequiredService<T>();
    }
}