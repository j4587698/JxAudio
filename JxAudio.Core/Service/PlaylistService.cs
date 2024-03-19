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

    public async Task<PlaylistWithSongs> GetPlaylistAsync(Guid userId, int playlistId, CancellationToken cancellationToken)
    {
        var playlist = await PlaylistEntity.Where(x => (x.IsPublic || x.UserId == userId) && x.Id == playlistId)
            .Include(x => x.UserEntity)
            .IncludeMany(x => x.TrackEntities,
                then => then.Where(y => y.DirectoryEntity!.IsAccessControlled == false ||
                                        y.DirectoryEntity.UserEntities!.Any(z =>
                                            z.Id == userId)))
            .FirstAsync(cancellationToken);

        if (playlist == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        return new PlaylistWithSongs()
        {
            allowedUser = null,
            id = playlist.Id.ToPlaylistId(),
            name = playlist.Name,
            comment = playlist.Description,
            owner = playlist.UserEntity?.UserName,
            @public = playlist.IsPublic,
            publicSpecified = true,
            songCount = playlist.TrackEntities?.Count ?? 0,
            duration = playlist.TrackEntities?.Sum(x => (int)x.Duration) ?? 0,
            created = playlist.CreateTime,
            changed = playlist.UpdateTime,
            coverArt = null,
            entry = playlist.TrackEntities?.Select(x => x.CreateTrackChild()).ToArray()
        };
    }
}