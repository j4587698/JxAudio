using FreeSql;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using JxAudio.Core.Subsonic;

namespace JxAudio.Core.Service;

[Transient]
public class PlaylistService
{
    private ISelect<PlaylistEntity> GetPlaylistBase(Guid userId)
    {
        return PlaylistEntity.Where(x => x.TrackEntities!.Any(y =>
            y.DirectoryEntity!.IsAccessControlled == false ||
            y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId))
        && (x.UserId == userId || x.IsPublic));
    }

    public async Task<Playlists> GetPlaylistsAsync(Guid apiUserId, CancellationToken cancellationToken)
    {
        var playlists = await GetPlaylistBase(apiUserId)
            .Include(x => x.UserEntity)
            .IncludeMany(x => x.TrackEntities!.Select(y => new TrackEntity()
            {
                Id = y.Id,
                Duration = y.Duration
            }))
            .ToListAsync(cancellationToken);

        return new Playlists()
        {
            playlist = playlists.Select(x => x.CreatePlaylist()).ToArray()
        };
    }
}