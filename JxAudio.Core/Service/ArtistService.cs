using System.Globalization;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;

namespace JxAudio.Core.Service;

[Transient]
public class ArtistService
{
    public async Task<ArtistsID3> GetArtistsAsync(Guid userId, int? musicFolderId, long? ifModifiedSince, CancellationToken cancellationToken)
    {
        var artist = await ArtistEntity.Select
            .WhereIf(musicFolderId != null, x => x.TrackEntities!.Any(y => y.DirectoryId == musicFolderId))
            .WhereIf(ifModifiedSince != null, x => x.CreateTime > DateTimeOffset.FromUnixTimeMilliseconds(ifModifiedSince!.Value))
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
        })
        .GroupBy(x =>
        {
            string name = x.name;
            if (name != null)
            {
                string t = StringInfo.GetNextTextElement(name).Normalize();
                if (t.Length > 0 && char.IsLetter(t, 0))
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t);
            }
            return "#";
        })
        .OrderBy(x => x.Key.ToString(), CultureInfo.CurrentCulture.CompareInfo.GetStringComparer(CompareOptions.IgnoreCase))
        .Select(x => new IndexID3()
        {
            name = x.Key,
            artist = x.ToArray()
        })
        .ToArray(); 
        return new ArtistsID3()
        {
            index = id3List,
            ignoredArticles = string.Empty
        };
    }
}