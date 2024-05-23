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
}