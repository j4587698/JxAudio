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
    public string? ArtistName { get; set; }

    [Description("歌曲数量")]
    public int Count { get; set; }

    [Description("专辑总大小")]
    public long TotalSize { get; set; }
}