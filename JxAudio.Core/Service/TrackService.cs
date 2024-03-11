using System.Diagnostics.CodeAnalysis;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using JxAudio.Core.Subsonic;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace JxAudio.Core.Service;

[Transient]
public class TrackService
{
    
    [Inject]
    [NotNull]
    private IStringLocalizer<ArtistService>? ArtistServiceLocalizer { get; set; }
    
    public async Task<Child> GetSongAsync(Guid userId, int trackId, CancellationToken cancellationToken)
    {
        var track = await TrackEntity.Where(x => x.Id == trackId && (x.DirectoryEntity!.IsAccessControlled == false || x.DirectoryEntity!.UserEntities!.Any(z => z.Id == userId)))
            .Include(x => x.AlbumEntity)
            .Include(x => x.GenreEntity)
            .IncludeMany(x => x.ArtistEntities)
            .IncludeMany(x => x.TrackStarEntities, then => then.Where(y => y.UserId == userId))
            .FirstAsync(cancellationToken);
        if (track == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        return track.CreateTrackChild(ArtistServiceLocalizer);
    }
}