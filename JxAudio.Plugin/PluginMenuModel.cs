using Microsoft.AspNetCore.Components;

namespace JxAudio.Plugin;


/// <summary>
/// 插件添加菜单
/// </summary>
public class PluginMenuModel
{
    /// <summary>
    /// 菜单Id
    /// </summary>
    public required string MenuId { get; set; }

    /// <summary>
    /// 菜单展示名
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// 菜单图标
    /// </summary>
    public string Icon { get; set; } = "fas fa-fw fa-puzzle-piece";

    /// <summary>
    /// 插件的界面
    /// </summary>
    public required RenderFragment PluginBody { get; set; }
    
}