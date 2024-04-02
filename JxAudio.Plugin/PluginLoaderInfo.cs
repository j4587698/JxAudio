using System.Reflection;
using McMaster.NETCore.Plugins;

namespace JxAudio.Plugin;

public class PluginLoaderInfo: IDisposable
{
    public PluginLoader? PluginLoader { get; set; }

    public Assembly? Assembly { get; set; }

    public IEnumerable<ISystemPlugin>? SystemPlugins { get; set; }

    public IEnumerable<IProviderPlugin>? ProviderPlugins { get; set; }
    public void Dispose()
    {
        SystemPlugins = null;
        ProviderPlugins = null;
        Assembly = null;
        PluginLoader?.Dispose();
    }
}