using System.ComponentModel.DataAnnotations;

namespace JxAudio.TransVo;

public class PlaylistVo
{
    public int? Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Description { get; set; }

    [Required]
    public bool IsPublic { get; set; }
    
    public int Count { get; set; }
    
    public long TotalSize { get; set; }
    
    public long TotalTime { get; set; }

    public List<TrackVo>? Songs { get; set; }
}