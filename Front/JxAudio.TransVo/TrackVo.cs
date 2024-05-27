using System.ComponentModel;

namespace JxAudio.TransVo;

public class TrackVo
{
    public int Id { get; set; }
    
    [Description("音轨大小")]
    public long Size { get; set; }

    [Description("音轨名")]
    public string? Name { get; set; }
    
    [Description("音轨序号")]
    public int? TrackNumber { get; set; }

    [Description("专辑序号")]
    public int? DiscNumber { get; set; }

    [Description("音频格式")]
    public string? CodecName { get; set; }

    [Description("MIME类型")]
    public string? MimeType { get; set; }
    
    [Description("比特率")]
    public int? BitRate { get; set; }
    
    [Description("音频时长")]
    public float Duration { get; set; }
    
    [Description("音轨标题")]
    public string? Title { get; set; }

    [Description("歌手名")]
    public List<ArtistVo>? Artists { get; set; }

    [Description("专辑名")]
    public AlbumVo? Album { get; set; }

    [Description("歌词Id")]
    public int LrcId { get; set; }

    [Description("封面Id")]
    public int CoverId { get; set; }

    public List<LrcVo>? Lrc { get; set; }
}
