using JxAudio.TransVo;

namespace JxAudio.Front.Data;

public class AddTrackMessage
{
    public PlayType Type { get; set; }

    public List<TrackVo>? Tracks { get; set; }
}

public enum PlayType
{
    Replace,
    Add,
    AddAndPlay
}