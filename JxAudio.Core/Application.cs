using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JxAudio.Core;

public static class Application
{
    public static IServiceProvider? ServiceProvider { get; internal set; }
    
    public static IServiceCollection? Services { get; internal set; }
    
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
    
    public static T? GetSingletonInstanceIfAlreadyCreated<T>()
    {
        // 逆序遍历，后注册的优先级高
        var descriptor = Services?.LastOrDefault(d => 
            d.ServiceType == typeof(T) && 
            d.Lifetime == ServiceLifetime.Singleton);

        // 只返回已存在的实例，不尝试创建新实例
        if (descriptor?.ImplementationInstance is T existingInstance)
        {
            return existingInstance;
        }

        return default;
    }
}