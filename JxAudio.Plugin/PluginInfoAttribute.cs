namespace JxAudio.Plugin;

[AttributeUsage(AttributeTargets.Class)]
public class PluginInfoAttribute: Attribute
{
    public required string Name { get; set; }

    public required string Id { get; set; }

    public required string Description { get; set; }

    public required string Author { get; set; }

    public required string Version { get; set; }
}