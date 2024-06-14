namespace JxAudio.TransVo;

public class PlaylistVo
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public List<int>? Songs { get; set; }
}