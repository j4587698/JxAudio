using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;

namespace JxAudio.Core.Service;

[Transient]
public class ArtistService
{
    public async Task<IndexID3> GetArtistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var artist = await ArtistEntity.Select
            .IncludeMany(x => x.ArtistStarEntities, then => then.Where(y => y.UserId == userId))
            .ToListAsync(cancellationToken);
        var id3List = artist.Select(x => new ArtistID3()
        {
            albumCount = (int)AlbumEntity.Select.Where(y => y.ArtistId == x.Id).Count(),
            coverArt = null,
            id = x.Id.ToString(),
            name = x.Name,
            starred = x.ArtistStarEntities?.Count > 0 ? x.ArtistStarEntities.First().CreateTime : default,
            starredSpecified = x.ArtistStarEntities?.Count > 0
        }).ToArray(); 
        return new IndexID3()
        {
            artist = id3List,
            name = "#"
        };
    }
}