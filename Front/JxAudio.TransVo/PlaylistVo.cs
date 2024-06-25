namespace JxAudio.TransVo;

public class PlaylistVo
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool IsPublic { get; set; }
    
    public int Count { get; set; }
    
    public long TotalSize { get; set; }
    
    public long TotalTime { get; set; }

    public List<TrackVo>? Songs { get; set; }
}