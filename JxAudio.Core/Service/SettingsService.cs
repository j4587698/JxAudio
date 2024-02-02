using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;

namespace JxAudio.Core.Service;

[Transient]
public class SettingsService
{
    public const string SystemGroupName = "System";
    
    public string? GetValue(string groupName, string name)
    {
        return SettingsEntity.Where(x => x.SettingName == name && x.GroupName == groupName).First()?.SettingValue;
    }

    public string? GetValue(string name)
    {
        return GetValue(SystemGroupName, name);
    }

    public void SetValue(string groupName, string name, string value)
    {
        var settings = SettingsEntity.Where(x => x.SettingName == name && x.GroupName == groupName).First()
                       ?? new SettingsEntity();
        settings.SettingName = name;
        settings.GroupName = groupName;
        settings.SettingValue = value;
        settings.Save();
    }

    public void SetValue(string name, string value)
    {
        SetValue(SystemGroupName, name, value);
    }

    public Dictionary<string, string?> GetAllValues(string groupName = SystemGroupName)
    {
        return SettingsEntity.Select.Where(x => x.GroupName == groupName)
            .ToDictionary(x => x.SettingName!, y => y.SettingValue);
    }

    public Dictionary<string, string?> GetValuesByNames(IEnumerable<string> names)
    {
        return GetValuesByNames(SystemGroupName, names);
    }

    public Dictionary<string, string?> GetValuesByNames(string groupName, IEnumerable<string> names)
    {
        return SettingsEntity.Where(x => x.GroupName == groupName && names.Contains(x.SettingName))
            .ToDictionary(x => x.SettingName!, y => y.SettingValue);
    }
}