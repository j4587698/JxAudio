namespace JxAudio.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ValueAttribute(string configPath) : Attribute
{
    /// <summary>
    /// config文件中的路径，多层用:分隔，如：AppConfig:AppConfigOption:ConfigSearchFolder
    /// </summary>
    public string ConfigPath { get; set; } = configPath;
}