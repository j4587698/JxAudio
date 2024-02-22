using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;

namespace JxAudio.Core.Service;

[Transient]
public class ArtistService
{
    public async Task GetArtistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var id3List = await ArtistEntity.Select
            .IncludeMany(x => x.ArtistStarEntities, then => then.Where(y => y.UserId == userId))
            .ToListAsync<ArtistID3>(x => new ArtistID3()
            {
                albumCount = x.AlbumEntities!.Count,
                coverArt = null,
                id = x.Id.ToString(),
                name = x.Name,
                starred = x.ArtistStarEntities == null ? default: x.ArtistStarEntities.First().CreateTime,
                starredSpecified = x.ArtistStarEntities != null
            }, cancellationToken);
        id3List.GroupBy(x =>
        {
            var firstChar = x.name[0];
            if (char.IsLetter(firstChar))
            {
                return firstChar.ToString().ToUpper();
            }

            return "#";
        });
    }
}