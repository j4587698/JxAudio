using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("专辑星标表")]
public class AlbumStarEntity: BaseEntity<AlbumEntity, Guid>
{
    [Description("用户Id")]
    [Column(IsPrimary = true)]
    public Guid UserId { get; set; }

    [Description("专辑Id")]
    [Column(IsPrimary = true)]
    public int AlbumId { get; set; }

    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(nameof(AlbumId))]
    public AlbumEntity? AlbumEntity { get; set; }
}