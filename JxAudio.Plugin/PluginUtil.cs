using JxAudio.Core;
using JxAudio.Core.Entity;
using Mono.Cecil;
using Serilog;

namespace JxAudio.Plugin;

public static class PluginUtil
{
    public static List<PluginConfig> GetAllPlugins()
    {
        var dbPlugins = PluginEntity.Select.ToList();
        var pluginConfigs = new List<PluginConfig>();
        if (!Directory.Exists(Constants.PluginPath))
        {
            Directory.CreateDirectory(Constants.PluginPath);
            return pluginConfigs;
        }
        
        var dirs = Directory.GetDirectories(Constants.PluginPath);
        foreach (var dir in dirs)
        {
            var dllPath = Path.Combine(dir, Path.GetFileName(dir) + ".dll");
            if (File.Exists(dllPath))
            {
                try
                {
                    using (var assembly = AssemblyDefinition.ReadAssembly(dllPath))
                    {
                        foreach (var type in assembly.MainModule.Types)
                        {
                            var attr = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(PluginInfoAttribute));
                            if (attr != null)
                            {
                                var configPlugin = ToPluginConfig(attr);
                                if (dbPlugins.Any(x => x.PluginId == configPlugin.Id))
                                {
                                    configPlugin.IsEnable = dbPlugins.First(x => x.PluginId == configPlugin.Id).IsEnable;
                                }
                                else
                                {
                                    new PluginEntity {IsEnable = false, PluginId = configPlugin.Id}.Save();
                                    configPlugin.IsEnable = false;
                                }
                                configPlugin.Path = dllPath;
                                pluginConfigs.Add(configPlugin);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Load Plugin Error", e);
                }
            }
        }

        return pluginConfigs;
    }

    public static PluginConfig ToPluginConfig(CustomAttribute customAttribute)
    {
        if (customAttribute.AttributeType.Name != nameof(PluginInfoAttribute))
        {
            return new PluginConfig();
        }
        
        var pluginConfig = new PluginConfig();
        foreach (var property in customAttribute.Properties)
        {
            switch (property.Name)
            {
                case nameof(PluginInfoAttribute.Id):
                    pluginConfig.Id = property.Argument.Value as string;
                    break;
                case nameof(PluginInfoAttribute.Name):
                    pluginConfig.Name = property.Argument.Value as string;
                    break;
                case nameof(PluginInfoAttribute.Description):
                    pluginConfig.Description = property.Argument.Value as string;
                    break;
                case nameof(PluginInfoAttribute.Author):
                    pluginConfig.Author = property.Argument.Value as string;
                    break;
                case nameof(PluginInfoAttribute.Version):
                    pluginConfig.Version = property.Argument.Value as string;
                    break;
            }
        }

        return pluginConfig;
    }
}