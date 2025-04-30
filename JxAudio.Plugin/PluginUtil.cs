using System.Reflection;
using BootstrapBlazor.Components;
using Jx.Toolbox.Extensions;
using JxAudio.Core;
using JxAudio.Core.Entity;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Mono.Cecil;
using Serilog;
using Constants = JxAudio.Core.Constants;

namespace JxAudio.Plugin;

public static class PluginUtil
{
    /// <summary>
    /// 已挂载插件列表
    /// </summary>
    private static readonly Dictionary<string, PluginLoaderInfo> Plugins = new();
    
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
            var filename = Path.GetFileName(dir);
            var dllPath = Path.Combine(dir, filename + ".dll");
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
                                var configPlugin = ToPluginConfig(attr, filename);
                                if (configPlugin == null)
                                {
                                    break;
                                }
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
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e,"Load Plugin Error");
                }
            }
        }

        return pluginConfigs;
    }

    public static PluginConfig? ToPluginConfig(CustomAttribute customAttribute, string id)
    {
        if (customAttribute.AttributeType.Name != nameof(PluginInfoAttribute))
        {
            return null;
        }
        
        var pluginConfig = new PluginConfig();
        pluginConfig.Id = id;
        foreach (var property in customAttribute.Properties)
        {
            switch (property.Name)
            {
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

    /// <summary>
    /// 切换插件的启用状态
    /// </summary>
    /// <param name="pluginId"></param>
    /// <param name="isEnable"></param>
    /// <returns></returns>
    public static bool ChangePluginStatus(string pluginId, out bool isEnable)
    {
        isEnable = false;
        var plugin = GetAllPlugins().FirstOrDefault(x => x.Id == pluginId);
        if (plugin == null)
        {
            return false;
        }
        if (plugin.IsEnable)
        {
            if (Plugins.TryGetValue(pluginId, out var pluginLoaderInfo))
            {
                if (pluginLoaderInfo.SystemPlugins != null)
                {
                    foreach (var systemPlugin in pluginLoaderInfo.SystemPlugins)
                    {
                        systemPlugin.PluginDisable();
                    }
                }
                UnloadPlugin(plugin);
                isEnable = false;
                PluginEntity.Select.Where(x => x.PluginId == pluginId).ToUpdate()
                    .Set(x => x.IsEnable, false)
                    .ExecuteAffrows();
                return true;
            }
        }
        else
        {
            if (LoadPlugin(plugin))
            {
                if (Plugins.TryGetValue(pluginId, out var pluginLoaderInfo))
                {
                    if (pluginLoaderInfo.SystemPlugins != null)
                    {
                        foreach (var systemPlugin in pluginLoaderInfo.SystemPlugins)
                        {
                            systemPlugin.PluginEnable();
                        }
                    }
                }

                PluginEntity.Select.Where(x => x.PluginId == pluginId).ToUpdate()
                    .Set(x => x.IsEnable, true)
                    .ExecuteAffrows();
                isEnable = true;
                return true;
            }
        }

        return false;
    }
    
    public static Assembly? GetAssemblyByPluginId(string pluginId)
    {
        return Plugins.TryGetValue(pluginId, out var plugin) ? plugin.Assembly : null;
    }
    
    /// <summary>
    /// 挂载插件
    /// </summary>
    /// <param name="pluginConfig">插件信息</param>
    public static bool LoadPlugin(PluginConfig pluginConfig)
    {
        if (pluginConfig.Id == null || pluginConfig.Path == null)
        {
            return false;
        }
        if (Plugins.ContainsKey(pluginConfig.Id))
        {
            ReLoadPlugin(pluginConfig);
            return false;
        }

        var plugin = PluginLoader.CreateFromAssemblyFile(pluginConfig.Path,
            isUnloadable:true,
            sharedTypes: [typeof(ISystemPlugin), typeof(IProviderPlugin)],
            config =>
            {
                config.IsUnloadable = true;
                config.LoadInMemory = true;
            });
        var assembly = plugin.LoadDefaultAssembly();
        var systemTypes = assembly.GetTypes()
            .Where(x => typeof(ISystemPlugin).IsAssignableFrom(x) && !x.IsAbstract)
            .Select(x => (ISystemPlugin)Activator.CreateInstance(x)!);
        var providerTypes =
            assembly.GetTypes().Where(x => typeof(IProviderPlugin).IsAssignableFrom(x) && !x.IsAbstract)
                .Select(x => (IProviderPlugin)Activator.CreateInstance(x)!);
        AddToPartManager(plugin, assembly);
        var pluginLoaderInfo = new PluginLoaderInfo
        {
            Assembly = assembly,
            PluginLoader = plugin,
            SystemPlugins = systemTypes,
            ProviderPlugins = providerTypes
        };
        Plugins.Add(pluginConfig.Id, pluginLoaderInfo);
        return true;
    }

    private static void AddToPartManager(PluginLoader pluginLoader, Assembly pluginAssembly)
    {
        var partManager = Application.GetSingletonInstanceIfAlreadyCreated<ApplicationPartManager>();
        if (partManager == null)
        {
            return;
        }
        var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
        foreach (var applicationPart in partFactory.GetApplicationParts(pluginAssembly))
        {
            partManager.ApplicationParts.Add(applicationPart);
        }
        var relatedAssembliesAttrs = pluginAssembly.GetCustomAttributes<RelatedAssemblyAttribute>();
        foreach (var attr in relatedAssembliesAttrs)
        {
            var assembly = pluginLoader.LoadAssembly(attr.AssemblyFileName);
            partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
            foreach (var part in partFactory.GetApplicationParts(assembly))
            {
                partManager.ApplicationParts.Add(part);
            }
        }
        var controllerFeature = new ControllerFeature();
        partManager.PopulateFeature(controllerFeature);
        MyActionDescriptorChangeProvider.Instance.HasChanged = true;
        MyActionDescriptorChangeProvider.Instance.TokenSource?.Cancel();
    }
    
    /// <summary>
    /// 卸载插件
    /// </summary>
    /// <param name="pluginConfig"></param>
    public static void UnloadPlugin(PluginConfig pluginConfig)
    {
        if (pluginConfig.Id == null)
        {
            return;
        }
        if (Plugins.TryRemove(pluginConfig.Id, out var plugin))
        {
            RemoveFromPartManager(plugin.PluginLoader!, plugin.Assembly!);
            plugin.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
    
    private static void RemoveFromPartManager(PluginLoader pluginLoader, Assembly pluginAssembly)
    {
        var partManager = Application.GetSingletonInstanceIfAlreadyCreated<ApplicationPartManager>();
        if (partManager == null)
        {
            return;
        }
        var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
        foreach (var applicationPart in partFactory.GetApplicationParts(pluginAssembly))
        {
            var parts = partManager.ApplicationParts.Where(x => x.Name == applicationPart.Name).ToArray();
            foreach (var part in parts)
            {
                partManager.ApplicationParts.Remove(part);
            }
        }
        var relatedAssembliesAttrs = pluginAssembly.GetCustomAttributes<RelatedAssemblyAttribute>();
        foreach (var attr in relatedAssembliesAttrs)
        {
            var assembly = pluginLoader.LoadAssembly(attr.AssemblyFileName);
            partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
            foreach (var applicationPart in partFactory.GetApplicationParts(assembly))
            {
                var parts = partManager.ApplicationParts.Where(x => x.Name == applicationPart.Name).ToArray();
                foreach (var part in parts)
                {
                    partManager.ApplicationParts.Remove(part);
                }
            }
        }
        MyActionDescriptorChangeProvider.Instance.HasChanged = true;
        MyActionDescriptorChangeProvider.Instance.TokenSource?.Cancel();
    }
    
    /// <summary>
    /// 重新加载插件
    /// </summary>
    /// <param name="pluginConfig"></param>
    public static void ReLoadPlugin(PluginConfig pluginConfig)
    {
        if (pluginConfig.Id == null)
        {
            return;
        }
        if (Plugins.TryGetValue(pluginConfig.Id, out var plugin))
        {
            plugin.PluginLoader?.Reload();
        }
    }

    public static void LoadPluginOnStartup()
    {
        var pluginConfigs = GetAllPlugins().Where(x => x.IsEnable);
        foreach (var pluginConfig in pluginConfigs)
        {
            LoadPlugin(pluginConfig);
        }
    }
    
    
    public static IEnumerable<ISystemPlugin> GetSystemPlugins()
    {
        return Plugins.Values.SelectMany(x => x.SystemPlugins ?? Array.Empty<ISystemPlugin>());
    }
    
    public static IEnumerable<IProviderPlugin> GetProviderPlugins()
    {
        return Plugins.Values.SelectMany(x => x.ProviderPlugins ?? Array.Empty<IProviderPlugin>());
    }
    
    public static PluginMenuModel? GetPluginMenuModel(string id)
    {
        var pluginMenuModels = GetSystemPlugins().SelectMany(x => x.AddMenuItem() ?? new List<PluginMenuModel>());
        return pluginMenuModels.FirstOrDefault(x => x.MenuId == id);
    }
}