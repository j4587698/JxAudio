using FreeSql;
using Jx.Toolbox.Extensions;
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
        var playlists = await PlaylistEntity.Where(x => x.UserId == apiUserId || x.IsPublic)
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
                                            z.Id == userId))
                    .Include(y => y.AlbumEntity)
                    .IncludeMany(y => y.ArtistEntities))
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

    public async Task<string> CreatePlaylistAsync(Guid userId, string name, List<int>? songId, CancellationToken cancellationToken)
    {
        if (songId != null)
        {
            if (await TrackEntity.Where(x => songId.Contains(x.Id)).CountAsync(cancellationToken) != songId.Count)
            {
                throw RestApiErrorException.DataNotFoundError();
            }
        }
        
        var playlist = new PlaylistEntity()
        {
            Name = name,
            UserId = userId,
            IsPublic = false,
            TrackEntities = songId?.Select(x => new TrackEntity()
            {
                Id = x
            }).ToList()
        };
        await playlist.SaveAsync();
        await playlist.SaveManyAsync(nameof(playlist.TrackEntities));

        return playlist.Id.ToPlaylistId();
    }

    public async Task RecreatePlaylistAsync(Guid userId, int playlistId, string? name, List<int>? songId, CancellationToken cancellationToken)
    {
        if (songId != null)
        {
            if (await TrackEntity.Where(x => songId.Contains(x.Id)).CountAsync(cancellationToken) != songId.Count)
            {
                throw RestApiErrorException.DataNotFoundError();
            }
        }
        
        var playlist = await PlaylistEntity.Where(x => (x.UserId == userId || x.IsPublic) && x.Id == playlistId).FirstAsync(cancellationToken);
        if (playlist == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        if (playlist.UserId != userId)
        {
            throw RestApiErrorException.UserNotAuthorizedError();
        }

        if (!name.IsNullOrEmpty())
        {
            playlist.Name = name;
        }
        
        playlist.TrackEntities = songId?.Select(x => new TrackEntity()
        {
            Id = x
        }).ToList();
        await playlist.SaveAsync();
        await playlist.SaveManyAsync(nameof(playlist.TrackEntities));
    }

    public async Task UpdatePlaylistAsync(Guid userId, int playlistId, string? name, string? comment, bool? @public, List<int>? songId, int[]? songIndexToRemove, CancellationToken cancellationToken)
    {
        if (songId != null)
        {
            if (await TrackEntity.Where(x => songId.Contains(x.Id)).CountAsync(cancellationToken) != songId.Count)
            {
                throw RestApiErrorException.DataNotFoundError();
            }
        }
        
        var playlist = await PlaylistEntity
            .Where(x => (x.UserId == userId || x.IsPublic) && x.Id == playlistId)
            .IncludeMany(x => x.TrackEntities!.Select(y => new TrackEntity(){Id = y.Id}),
                then => then.Where(y => y.DirectoryEntity!.IsAccessControlled == false ||
                                        y.DirectoryEntity.UserEntities!.Any(z =>
                                            z.Id == userId)))
            .FirstAsync(cancellationToken);
        if (playlist == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        
        if (playlist.UserId != userId)
        {
            throw RestApiErrorException.UserNotAuthorizedError();
        }

        if (songIndexToRemove is { Length: > 0 } && playlist.TrackEntities is List<TrackEntity> trackEntities)
        {
            foreach (var i in songIndexToRemove.OrderDescending())
            {
                if (trackEntities.Count > i)
                {
                    trackEntities.RemoveAt(i);
                }
            }
        }

        if (!name.IsNullOrEmpty())
        {
            playlist.Name = name;
        }

        if (!comment.IsNullOrEmpty())
        {
            playlist.Description = comment;
        }
        
        if (@public != null)
        {
            playlist.IsPublic = @public.Value;
        }

        playlist.TrackEntities ??= new List<TrackEntity>();

        if (playlist.TrackEntities is List<TrackEntity> trackEntities1)
        {
            if (songId != null)
            {
                trackEntities1.AddRange(songId.Select(x => new TrackEntity()
                {
                    Id = x
                }));
            }
        }
        
        await playlist.SaveAsync();
        await playlist.SaveManyAsync(nameof(playlist.TrackEntities));
    }
    
    public async Task DeletePlaylistAsync(Guid userId, int playlistId, CancellationToken cancellationToken)
    {
        var playlistRepository = BaseEntity.Orm.GetRepository<PlaylistEntity>();
        var playlist = await playlistRepository.DeleteCascadeByDatabaseAsync(x => x.Id == playlistId && x.UserId == userId, cancellationToken: cancellationToken);
        if (playlist is {Count:0})
        {
            throw RestApiErrorException.DataNotFoundError();
        }
    }
}