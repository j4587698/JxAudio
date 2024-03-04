using System.Globalization;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;

namespace JxAudio.Core.Service;

[Transient]
public class GenreService
{
    public async Task<Genres> GetGenresAsync(Guid userId, CancellationToken cancellationToken)
    {
        var genres = await GenreEntity.Where(x => x.TrackEntities!.Any(y => 
            y.DirectoryEntity!.IsAccessControlled == false || y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .OrderBy(x => x.Name)
            .ToListAsync(x => new 
            {
                Text  = x.Name,
                songCount = x.TrackEntities!.Count,
                albumCount = x.TrackEntities.DistinctBy(y => y.AlbumId).Count()
            },cancellationToken);
        return new Genres()
        {
            genre = genres.Select(x => new Genre()
            {
                Text = [x.Text],
                albumCount = x.albumCount,
                songCount = x.songCount
            }).ToArray()
        };
    }
    
}