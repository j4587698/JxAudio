using BootstrapBlazor.Components;
using JxAudio.Plugin;

namespace AListProviderPlugin;

[PluginInfo(Id = "D1D8F1CD-E92B-46DE-902F-128A00495DFD", Name = "AList提供器", Author = "JxAudio", Description = "AList提供器", Version = "1.0")]
public class SystemInstance: ISystemPlugin
{
    public List<PluginMenuModel>? AddMenuItem()
    {
        List<PluginMenuModel> list = new List<PluginMenuModel>();
        list.Add(new PluginMenuModel()
        {
            DisplayName = "配置AList",
            MenuId = "AListProviderPlugin",
            PluginBody = BootstrapDynamicComponent.CreateComponent<AlistManager>().Render()
        });
        return list;
    }
}