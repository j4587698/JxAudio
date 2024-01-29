using System.Reflection;
using System.Text.RegularExpressions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Jx.Toolbox.Extensions;
using JxAudio.Core.Attributes;
using JxAudio.Core.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JxAudio.Core.Extensions;

public static class WebApplicationExtension
{
    public static WebApplicationBuilder Inject(this WebApplicationBuilder webApplicationBuilder,
        Action<AppConfigOption>? configOption = null,
        Action<ContainerBuilder>? containerBuilder = null)
    {
        var jsonPattern = @"^(?<name>[^.]+)(\.(?<env>[^.]+))?\.json$";
        var xmlPattern = @"^(?<name>[^.]+)(\.(?<env>[^.]+))?\.xml";


        AppConfigOption option = new AppConfigOption();
        configOption?.Invoke(option);
        
        webApplicationBuilder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(containerBuilder));
        webApplicationBuilder.Host.ConfigureContainer<ContainerBuilder>((context, builder) =>
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
            var scopedTypes = types.Where(x => x is { IsClass: true, IsAbstract: false } && x.GetCustomAttributes(typeof(ScopedAttribute), false).Length != 0);
            foreach (var scopedType in scopedTypes)
            {
                Register(builder, scopedType, option, "scoped", webApplicationBuilder);
            }
        }).ConfigureAppConfiguration((context, builder) =>
        {
            builder.Sources.Clear();
            
            LoadConfig(builder, AppContext.BaseDirectory, "*.json", jsonPattern, webApplicationBuilder.Environment);
            
            if (option.EnableXmlSearcher)
            {
                LoadConfig(builder, AppContext.BaseDirectory, "*.xml", xmlPattern, webApplicationBuilder.Environment);
            }

            if (option.ConfigSearchFolder is {Count: > 0})
            {
                foreach (var folder in option.ConfigSearchFolder)
                {
                    if (!folder.IsNullOrWhiteSpace())
                    {
                        LoadConfig(builder, Path.Combine(AppContext.BaseDirectory, folder), "*.json", jsonPattern, webApplicationBuilder.Environment);
                        if (option.EnableXmlSearcher)
                        {
                            LoadConfig(builder, Path.Combine(AppContext.BaseDirectory, folder), "*.xml", xmlPattern, webApplicationBuilder.Environment);
                        }
                    }
                }
            }
            
            
        });
        return webApplicationBuilder;
    }

    private static void Register(ContainerBuilder builder, Type type, AppConfigOption option, string scope,  WebApplicationBuilder context)
    {
        if (type.IsGenericType)
        {
            var register = builder.RegisterGeneric(type);
            var interfaces = type.GetInterfaces().Where(x => x.IsGenericType);
            foreach (var @interface in interfaces)
            {
                register.As(@interface);
            }

            if (option.RegisterSelfIfHasInterface || !interfaces.Any())
            {
                register.AsSelf();
            }

            register.PropertiesAutowired((info, o) =>
                info.GetCustomAttributes(typeof(InjectAttribute), false).Length > 0);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes(typeof(ValueAttribute), false).Length > 0);
            foreach (var property in properties)
            {
                var valueAttribute = property.GetCustomAttribute<ValueAttribute>();
                var value = context.Configuration[valueAttribute!.ConfigPath];
                if (value != null)
                {
                    register.WithProperty(property.Name, value);
                }
            }

            switch (scope)
            {
                case "singleton":
                    register.SingleInstance();
                    break;
                case "transient":
                    register.InstancePerDependency();
                    break;
                case "scoped":
                    register.InstancePerLifetimeScope();
                    break;
            }
        }
        else
        {
            var register = builder.RegisterType(type);
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                register.As(@interface);
            }

            if (option.RegisterSelfIfHasInterface || !interfaces.Any())
            {
                register.AsSelf();
            }
            
            register.PropertiesAutowired((info, o) =>
                info.GetCustomAttributes(typeof(InjectAttribute), false).Length > 0);

            var properties = type.GetProperties().Where(x => Attribute.IsDefined(x, typeof(ValueAttribute)));
            foreach (var property in properties)
            {
                var valueAttribute = property.GetCustomAttribute<ValueAttribute>();
                var value = context.Configuration[valueAttribute!.ConfigPath];
                if (value != null)
                {
                    register.WithProperty(property.Name, value);
                }
            }

            switch (scope)
            {
                case "singleton":
                    register.SingleInstance();
                    break;
                case "transient":
                    register.InstancePerDependency();
                    break;
                case "scoped":
                    register.InstancePerLifetimeScope();
                    break;
            }
        }
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
}