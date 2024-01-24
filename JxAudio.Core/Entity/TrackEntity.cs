using System.ComponentModel;
using FreeSql;

namespace JxAudio.Core.Entity;

[Description("歌曲表")]
public class TrackEntity : BaseEntity<TrackEntity, long>
{
    [Description("提供器Id")]
    public Guid ProviderId { get; set; }

    [Description("音轨路径")]
    public string? FullName { get; set; }

    [Description("音轨名")]
    public string? Name { get; set; }
    
    [Description("音轨序号")]
    public int StreamIndex { get; set; }

    [Description("音频格式")]
    public string? CodecName { get; set; }
    
    [Description("比特率")]
    public int? BitRate { get; set; }
    
    [Description("音频时长")]
    public float? Duration { get; set; }
    
    [Description("音轨标题")]
    public string? Title { get; set; }
    
    
}