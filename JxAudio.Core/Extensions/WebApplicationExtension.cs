using System.Reflection;
using System.Text.RegularExpressions;
using Jx.Toolbox.Extensions;
using JxAudio.Core.Attributes;
using JxAudio.Core.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JxAudio.Core.Extensions;

public static class WebApplicationExtension
{
    public static WebApplicationBuilder Inject(this WebApplicationBuilder webApplicationBuilder,
        Action<AppConfigOption>? configOption = null)
    {
        Application.WebHostEnvironment = webApplicationBuilder.Environment;
        Application.Configuration = webApplicationBuilder.Configuration;
        Application.Services = webApplicationBuilder.Services;
        
        var jsonPattern = @"^(?<name>[^.]+)(\.(?<env>[^.]+))?\.json$";
        var xmlPattern = @"^(?<name>[^.]+)(\.(?<env>[^.]+))?\.xml";

        
        AppConfigOption option = new AppConfigOption();
        configOption?.Invoke(option);
        webApplicationBuilder.Services.Configure(configOption ?? (appConfigOption => { }) );
        webApplicationBuilder.Configuration.Sources.Clear();
        LoadConfig(webApplicationBuilder.Configuration, AppContext.BaseDirectory, "*.json", jsonPattern, webApplicationBuilder.Environment);
            
        if (option.EnableXmlSearcher)
        {
            LoadConfig(webApplicationBuilder.Configuration, AppContext.BaseDirectory, "*.xml", xmlPattern, webApplicationBuilder.Environment);
        }

        if (option.ConfigSearchFolder is {Count: > 0})
        {
            foreach (var folder in option.ConfigSearchFolder)
            {
                if (!folder.IsNullOrWhiteSpace())
                {
                    var path = Path.Combine(AppContext.BaseDirectory, folder);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    LoadConfig(webApplicationBuilder.Configuration,  path, "*.json", jsonPattern, webApplicationBuilder.Environment);
                    if (option.EnableXmlSearcher)
                    {
                        LoadConfig(webApplicationBuilder.Configuration, path, "*.xml", xmlPattern, webApplicationBuilder.Environment);
                    }
                }
            }
        }
        
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .ToList();

        foreach (var type in types)
        {
            if (type.GetCustomAttribute<ScopedAttribute>() != null)
            {
                var serviceTypes = GetServiceTypes(type, option);
                foreach (var type1 in serviceTypes)
                {
                    webApplicationBuilder.Services.AddScoped(type1, type);
                }
            }
            else if (type.GetCustomAttribute<SingletonAttribute>() != null)
            {
                var serviceTypes = GetServiceTypes(type, option);
                foreach (var type1 in serviceTypes)
                {
                    webApplicationBuilder.Services.AddSingleton(type1, type);
                }
            }
            else if (type.GetCustomAttribute<TransientAttribute>() != null)
            {
                var serviceTypes = GetServiceTypes(type, option);
                foreach (var type1 in serviceTypes)
                {
                    webApplicationBuilder.Services.AddTransient(type1, type);
                }
            }
        }
        
        return webApplicationBuilder;
    }
    
    private static IEnumerable<Type> GetServiceTypes(Type implementationType, AppConfigOption option)
    {
        if (implementationType.IsGenericType)
        {
            // 开放泛型返回泛型类型定义
            var interfaces = implementationType.GetInterfaces()
                .Where(i => i is { IsGenericType: true }).Select(x =>
                {
                    if (x.IsTypeDefinition)
                    {
                        return x;
                    }

                    return x.GetGenericTypeDefinition();
                }).ToList();
            
            if (option.RegisterSelfIfHasInterface || interfaces.Count == 0)
            {
                interfaces.Add(implementationType);
            }

            return interfaces;
        }
    
        // 非泛型或封闭泛型处理
        var implementedInterfaces = implementationType.GetInterfaces().ToList();
        
        if (option.RegisterSelfIfHasInterface || implementedInterfaces.Count == 0)
        {
            implementedInterfaces.Add(implementationType);
        }
        return implementedInterfaces;
    }

    private static void LoadConfig(IConfigurationBuilder config, string path, string searchPattern, string pattern, IWebHostEnvironment environment)
    {
        var files = Directory.EnumerateFiles(path, searchPattern);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var match = Regex.Match(fileName.ToLower(), pattern);
            if (match.Success)
            {
                var name = match.Groups["name"].Value;
                if (environment.ApplicationName.StartsWith(name))
                {
                    continue;
                }
                var env = match.Groups["env"].Value;
                if (env.IsNullOrEmpty() || env.Equals(environment.EnvironmentName, StringComparison.OrdinalIgnoreCase))
                {
                    if (searchPattern == "*.json")
                    {
                        config.AddJsonFile(file, optional:true, reloadOnChange:true);
                    }
                    else if (searchPattern == "*.xml")
                    {
                        config.AddXmlFile(file, optional: true, reloadOnChange: true);
                    }
                }
            }
        }
    }

    public static WebApplication Use(this WebApplication webApplication)
    {
        Application.ServiceProvider = webApplication.Services;
        return webApplication;
    }
}