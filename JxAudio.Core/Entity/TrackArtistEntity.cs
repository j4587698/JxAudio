using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

public class TrackArtistEntity
{
    [Column(IsPrimary = true)]
    public int TrackId { get; set; }

    [Navigate(nameof(TrackId))]
    public TrackEntity? TrackEntity { get; set; }

    [Column(IsPrimary = true)]
    public int ArtistId { get; set; }
    
    [Navigate(nameof(ArtistId))]
    public ArtistEntity? ArtistEntity { get; set; }
}