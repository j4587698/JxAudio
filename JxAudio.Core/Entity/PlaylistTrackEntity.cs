using System.ComponentModel;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("播放列表音轨中间表")]
public class PlaylistTrackEntity
{
    [Description("播放列表ID")]
    [Column(IsPrimary = true)]
    public int PlaylistId { get; set; }

    [Navigate(nameof(PlaylistId))]
    public PlaylistEntity? PlaylistEntity { get; set; }

    [Description("音轨ID")]
    [Column(IsPrimary = true)]
    public int TrackId { get; set; }

    [Navigate(nameof(TrackId))]
    public TrackEntity? TrackEntity { get; set; }
}