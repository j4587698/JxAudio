using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("音乐流派表")]
public class GenreEntity: BaseEntity<GenreEntity, Guid>
{
    [Description("流派名称")]
    public string? Name { get; set; }
    
    [Navigate(nameof(TrackEntity.GenreId))]
    public ICollection<TrackEntity>? TrackEntities { get; set; }
}