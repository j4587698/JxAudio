using System.ComponentModel;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("专辑评分表")]
public class AlbumRatingEntity
{
    [Description("用户Id")]
    [Column(IsPrimary = true)]
    public Guid UserId { get; set; }

    [Description("专辑Id")]
    [Column(IsPrimary = true)]
    public int AlbumId { get; set; }

    [Description("创建时间")]
    public DateTime CreateTime { get; set; }

    [Description("评分")]
    public float Rating { get; set; }
    
    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(nameof(AlbumId))]
    public AlbumEntity? AlbumEntity { get; set; }
}