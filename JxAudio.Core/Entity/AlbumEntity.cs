using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("专辑表")]
public class AlbumEntity : BaseEntity<AlbumEntity, int>
{
    [Description("专辑名")]
    public string? Title { get; set; }

    [Description("发布时间")]
    public int? Year { get; set; }

    [Description("播放次数")]
    public long PlayCount { get; set; }

    [Description("歌手Id")]
    public int ArtistId { get; set; }

    [Navigate(nameof(ArtistId))]
    public ArtistEntity? ArtistEntity { get; set; }

    [Description("封面Id")]
    public int? PictureId { get; set; }

    [Navigate(nameof(PictureId))]
    public PictureEntity? PictureEntity { get; set; }
    
    [Description("流派Id")]
    public int? GenreId { get; set; }
    
    [Navigate(nameof(GenreId))]
    public GenreEntity? GenreEntity { get; set; }

    [Column(IsIgnore = true)]
    public long Count { get; set; }

    [Navigate(nameof(TrackEntity.AlbumId))]
    public ICollection<TrackEntity>? TrackEntities { get; set; }

    [Navigate(nameof(AlbumStarEntity.AlbumId))]
    public ICollection<AlbumStarEntity>? AlbumStarEntities { get; set; }
}