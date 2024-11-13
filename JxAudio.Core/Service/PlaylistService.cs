using BootstrapBlazor.Components;
using FreeSql;
using FreeSql.Internal.Model;
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
        return PlaylistEntity.Where(x => x.UserId == userId || x.IsPublic);
    }
    
    public async Task<QueryData<PlaylistEntity>> QueryData(QueryPageOptions options, DynamicFilterInfo dynamicFilterInfo,
        Guid userId, bool starOnly)
    {
        var select = GetPlaylistBase(userId)
            .IncludeMany(x => x.TrackEntities!.Select(y => new TrackEntity()
                {
                    Id = y.Id,
                    Duration = y.Duration,
                    Size = y.Size
                }),
                then => then.Where(y =>
                    y.DirectoryEntity!.IsAccessControlled == false ||
                    y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .WhereDynamicFilter(dynamicFilterInfo)
            .OrderByPropertyNameIf(options.SortOrder != SortOrder.Unset, options.SortName,
                options.SortOrder == SortOrder.Asc)
            .Count(out var count);
        if (options.IsPage)
        {
            select.Page(options.PageIndex, options.PageItems);
        }

        var data = await select.ToListAsync();
        
        return new QueryData<PlaylistEntity>()
        {
            TotalCount = (int)count,
            Items = data,
            IsSorted = options.SortOrder != SortOrder.Unset,
            IsFiltered = true,
            IsAdvanceSearch = true,
            IsSearch = true
        };
    }

    public async Task<List<PlaylistEntity>> GetPlaylistsAsync(Guid apiUserId, CancellationToken cancellationToken)
    {
        return await PlaylistEntity.Where(x => x.UserId == apiUserId || x.IsPublic)
            .Include(x => x.UserEntity)
            .IncludeMany(x => x.TrackEntities!.Select(y => new TrackEntity()
            {
                Id = y.Id,
                Duration = y.Duration
            }))
            .ToListAsync(cancellationToken);
    }

    public async Task<PlaylistEntity> GetPlaylistAsync(Guid userId, int playlistId, CancellationToken cancellationToken)
    {
        return await PlaylistEntity.Where(x => (x.IsPublic || x.UserId == userId) && x.Id == playlistId)
            .Include(x => x.UserEntity)
            .IncludeMany(x => x.TrackEntities,
                then => then.Where(y => y.DirectoryEntity!.IsAccessControlled == false ||
                                        y.DirectoryEntity.UserEntities!.Any(z =>
                                            z.Id == userId))
                    .Include(y => y.AlbumEntity)
                    .IncludeMany(y => y.ArtistEntities))
            .FirstAsync(cancellationToken);
    }

    public async Task<int> CreatePlaylistAsync(Guid userId, string name, string? description, 
        bool isPublic, List<int>? songId, CancellationToken cancellationToken)
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
            Description = description,
            UserId = userId,
            IsPublic = isPublic,
            TrackEntities = songId?.Select(x => new TrackEntity()
            {
                Id = x
            }).ToList()
        };
        await playlist.SaveAsync();
        BaseEntity.Orm.GetRepository<PlaylistEntity>().SaveMany(playlist, nameof(playlist.TrackEntities));
        //await playlist.SaveManyAsync(nameof(playlist.TrackEntities));

        return playlist.Id;
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
        BaseEntity.Orm.GetRepository<PlaylistEntity>().SaveMany(playlist, nameof(playlist.TrackEntities));
        //await playlist.SaveManyAsync(nameof(playlist.TrackEntities));
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
            if (songIndexToRemove is [4587698])
            {
                trackEntities.Clear();
            }
            else
            {
                foreach (var i in songIndexToRemove.OrderDescending())
                {
                    if (trackEntities.Count > i)
                    {
                        trackEntities.RemoveAt(i);
                    }
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
        BaseEntity.Orm.GetRepository<PlaylistEntity>().SaveMany(playlist, nameof(playlist.TrackEntities));
        //await playlist.SaveManyAsync(nameof(playlist.TrackEntities));
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