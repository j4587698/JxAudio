using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("歌手星标表")]
public class ArtistStarEntity: BaseEntity<ArtistStarEntity, long>
{
    [Description("用户Id")]
    public Guid UserId { get; set; }

    [Description("歌手Id")]
    public long ArtistId { get; set; }

    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(nameof(ArtistId))]
    public ArtistEntity? ArtistEntity { get; set; }
}