using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("歌手表")]
public class ArtistEntity: BaseEntity<ArtistEntity, long>
{
    [Description("歌手名")]
    public string? Name { get; set; }

    [Column(IsIgnore = true)]
    public long Count { get; set; }
    
    [Navigate(ManyToMany = typeof(TrackArtistEntity))]
    public ICollection<TrackEntity>? TrackEntities { get; set; }
}