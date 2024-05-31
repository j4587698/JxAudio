using System.ComponentModel;

namespace JxAudio.TransVo;

public class AlbumVo
{
    [Description("标题")]
    public string? Title { get; set; }

    public int Id { get; set; }
    
    [Description("封面")]
    public int? CoverId { get; set; }

    public int? ArtistId { get; set; }

    [Description("艺术家")]
    public ArtistVo? Artist { get; set; }

    [Description("歌曲数量")]
    public int Count { get; set; }

    [Description("专辑总大小")]
    public long TotalSize { get; set; }

    [Description("专辑总时长")]
    public long TotalTime { get; set; }

    [Description("是否Star")]
    public bool Star { get; set; }
}