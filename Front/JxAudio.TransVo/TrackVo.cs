﻿using System.ComponentModel;

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
    public string? ArtistName { get; set; }
}