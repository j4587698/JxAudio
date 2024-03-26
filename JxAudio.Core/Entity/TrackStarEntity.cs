using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("歌曲星标表")]
public class TrackStarEntity
{
    [Description("用户Id")]
    [Column(IsPrimary = true)]
    public Guid UserId { get; set; }

    [Description("歌曲Id")]
    [Column(IsPrimary = true)]
    public int TrackId { get; set; }
    
    [Description("创建时间")]
    public DateTime CreateTime { get; set; }

    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(nameof(TrackId))]
    public TrackEntity? TrackEntity { get; set; }
}