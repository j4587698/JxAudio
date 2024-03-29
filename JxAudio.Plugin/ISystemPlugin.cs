namespace JxAudio.Plugin;

/// <summary>
/// 后台相关接口
/// </summary>
public interface ISystemPlugin
{
    /// <summary>
    /// 插件被启用时
    /// </summary>
    void PluginEnable()
    {
    }
    
    /// <summary>
    /// 插件被禁用时
    /// </summary>
    void PluginDisable()
    {
    }

    /// <summary>
    /// 插件被删除时
    /// </summary>
    void PluginDeleted()
    {
    }
    
    /// <summary>
    /// 添加后台菜单项
    /// </summary>
    /// <returns></returns>
    List<PluginMenuModel>? AddMenuItem()
    {
        return null;
    }
}