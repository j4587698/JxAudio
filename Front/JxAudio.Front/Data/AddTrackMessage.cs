using JxAudio.TransVo;

namespace JxAudio.Front.Data;

public class AddTrackMessage
{
    public string? Type { get; set; }

    public List<TrackVo>? Tracks { get; set; }
}