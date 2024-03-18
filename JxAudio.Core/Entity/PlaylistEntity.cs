using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("播放列表表")]
public class PlaylistEntity: BaseEntity<PlaylistEntity, int>
{
    [Description("播放列表名称")]
    public string? Name { get; set; }

    [Description("播放列表描述")]
    public string? Description { get; set; }

    [Description("是否公开")]
    public bool IsPublic { get; set; }

    [Description("用户ID")]
    public Guid? UserId { get; set; }

    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Navigate(ManyToMany = typeof(PlaylistTrackEntity))]
    public ICollection<TrackEntity>? TrackEntities { get; set; }
    
    
}