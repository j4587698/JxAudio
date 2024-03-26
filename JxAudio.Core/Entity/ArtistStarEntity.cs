using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("歌手星标表")]
public class ArtistStarEntity
{
    [Description("用户Id")]
    [Column(IsPrimary = true)]
    public Guid UserId { get; set; }

    [Description("歌手Id")]
    [Column(IsPrimary = true)]
    public int ArtistId { get; set; }

    [Description("创建时间")]
    public DateTime CreateTime { get; set; }

    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(nameof(ArtistId))]
    public ArtistEntity? ArtistEntity { get; set; }
}