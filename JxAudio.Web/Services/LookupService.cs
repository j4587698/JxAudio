using BootstrapBlazor.Components;
using JxAudio.Core.Attributes;
using JxAudio.Plugin;
using JxAudio.Web.Utils;

namespace JxAudio.Web.Services;

[Singleton]
public class LookupService : ILookupService
{
    public IEnumerable<SelectedItem>? GetItemsByKey(string? key)
    {
        if (key == "plugin")
        {
            var selectedItems = PluginUtil.GetProviderPlugins().Where(x => x.Ready)
                .Select(x => new SelectedItem(x.Id.ToString(), x.Name!)).ToList();
            selectedItems.Insert(0, new SelectedItem("00000000-0000-0000-0000-000000000000", "请选择 ..."));
            return selectedItems;
        }
        return null;
    }

    public IEnumerable<SelectedItem>? GetItemsByKey(string? key, object? data)
    {
        return GetItemsByKey(key);
    }

    public Task<IEnumerable<SelectedItem>?> GetItemsByKeyAsync(string? key, object? data)
    {
        return Task.FromResult(GetItemsByKey(key));
    }
}