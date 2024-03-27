using System.ComponentModel;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("歌曲评分表")]
public class TrackRatingEntity
{
    [Description("用户Id")]
    [Column(IsPrimary = true)]
    public Guid UserId { get; set; }

    [Description("歌曲Id")]
    [Column(IsPrimary = true)]
    public int TrackId { get; set; }
    
    [Description("创建时间")]
    public DateTime CreateTime { get; set; }

    [Description("评分")]
    public float Rating { get; set; }

    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(nameof(TrackId))]
    public TrackEntity? TrackEntity { get; set; }
}