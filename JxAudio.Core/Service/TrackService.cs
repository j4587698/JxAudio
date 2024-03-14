using System.Diagnostics.CodeAnalysis;
using FreeSql;
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

    private ISelect<TrackEntity> GetTrackBase(Guid userId)
    {
        return TrackEntity.Where(x => x.DirectoryEntity!.IsAccessControlled == false || 
                                      x.DirectoryEntity!.UserEntities!.Any(z => z.Id == userId))
            .Include(x => x.AlbumEntity)
            .Include(x => x.GenreEntity)
            .IncludeMany(x => x.ArtistEntities)
            .IncludeMany(x => x.TrackStarEntities, 
                then => then.Where(y => y.UserId == userId));
    }
    
    public async Task<Child> GetSongAsync(Guid userId, int trackId, CancellationToken cancellationToken)
    {
        var track = await GetTrackBase(userId)
            .Where(x => x.Id == trackId)
            .FirstAsync(cancellationToken);
        if (track == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        return track.CreateTrackChild();
    }

    public async Task<Songs> GetRandomSongsAsync(Guid userId, int? musicFolderId, string? genre, int? fromYear, int? toYear, int count, CancellationToken cancellationToken)
    {
        var songs = await GetTrackBase(userId)
            .WhereIf(musicFolderId != null, x => x.DirectoryId == musicFolderId)
            .WhereIf(genre != null, x => x.GenreEntity!.Name == genre)
            .WhereIf(fromYear != null, x => x.AlbumEntity!.Year >= fromYear)
            .WhereIf(toYear != null, x => x.AlbumEntity!.Year <= toYear)
            .OrderByRandom()
            .Take(count)
            .ToListAsync(cancellationToken);

        if (songs == null || songs.Count == 0)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        return new Songs()
        {
            song = songs.Select(x => x.CreateTrackChild()).ToArray()
        };
    }
}