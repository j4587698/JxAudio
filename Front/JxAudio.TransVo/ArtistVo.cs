using System.ComponentModel;

namespace JxAudio.TransVo;

public class ArtistVo
{
    public int Id { get; set; }

    [Description("歌手名")]
    public string? Name { get; set; }

    [Description("歌曲数量")]
    public int Count { get; set; }

    [Description("总大小")]
    public long TotalSize { get; set; }

    [Description("总时长")]
    public long TotalTime { get; set; }
    
    [Description("封面")]
    public int? CoverId { get; set; }
}