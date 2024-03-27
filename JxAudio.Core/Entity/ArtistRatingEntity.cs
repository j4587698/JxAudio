using System.ComponentModel;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("艺术家评分表")]
public class ArtistRatingEntity
{
    [Description("用户Id")]
    [Column(IsPrimary = true)]
    public Guid UserId { get; set; }

    [Description("艺术家Id")]
    [Column(IsPrimary = true)]
    public int ArtistId { get; set; }

    [Description("创建时间")]
    public DateTime CreateTime { get; set; }

    [Description("评分")]
    public float Rating { get; set; }
    
    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(nameof(ArtistId))]
    public ArtistEntity? ArtistEntity { get; set; }
}