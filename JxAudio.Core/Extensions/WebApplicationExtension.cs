using System.Text.RegularExpressions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Jx.Toolbox.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace JxAudio.Core.Extensions;

public static class WebApplicationExtension
{
    public static WebApplicationBuilder Inject(this WebApplicationBuilder webApplicationBuilder, Action<ContainerBuilder>? containerBuilder = null)
    {
        var jsonPattern = @"^(?<name>[^.]+)(\.(?<env>[^.]+))?\.json$";
        var xmlPattern = @"^(?<name>[^.]+)(\.(?<env>[^.]+))?\.xml";
        
        webApplicationBuilder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(containerBuilder));
        webApplicationBuilder.Host.ConfigureContainer<ContainerBuilder>((context, builder) =>
        {
            
        }).ConfigureAppConfiguration((context, builder) =>
        {
            builder.Sources.Clear();
            
            LoadConfig(builder, AppContext.BaseDirectory, "*.json", jsonPattern, webApplicationBuilder.Environment);

        });
        return webApplicationBuilder;
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
                if (fileName.StartsWith(name))
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