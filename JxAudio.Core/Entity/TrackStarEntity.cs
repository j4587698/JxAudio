using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("歌曲星标表")]
public class TrackStarEntity: BaseEntity<TrackStarEntity, int>
{
    [Description("用户Id")]
    public Guid UserId { get; set; }

    [Description("歌曲Id")]
    public int TrackId { get; set; }

    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(nameof(TrackId))]
    public TrackEntity? TrackEntity { get; set; }
}