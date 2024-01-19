using System.ComponentModel;
using FreeSql;

namespace JxAudio.Core.Entity;

[Description("歌曲表")]
public class TrackEntity : BaseEntity<TrackEntity, long>
{
    [Description("音轨序号")]
    public int StreamIndex { get; set; }

    [Description("音频格式")]
    public string? CodecName { get; set; }
    
    [Description("比特率")]
    public int? BitRate { get; set; }
    
    [Description("音频时长")]
    public float? Duration { get; set; }
    
    public string Title { get; set; }
    public string SortTitle { get; set; }
}