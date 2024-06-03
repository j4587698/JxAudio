using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using FreeSql;
using FreeSql.Internal.Model;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using JxAudio.Core.Subsonic;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace JxAudio.Core.Service;

[Transient]
public class AlbumService
{
    [Inject]
    [NotNull]
    private IStringLocalizer<ArtistService>? ArtistServiceLocalizer { get; set; }

    private ISelect<AlbumEntity> GetAlbumBase(Guid? userId, int? musicFolderId)
    {
        return AlbumEntity.Where(x => x.TrackEntities!.Any(y =>
                y.DirectoryEntity!.IsAccessControlled == false ||
                y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .WhereIf(musicFolderId != null, x => x.TrackEntities!.Any(y => y.DirectoryId == musicFolderId))
            .Include(x => x.ArtistEntity)
            .Include(x => x.GenreEntity)
            .IncludeMany(x => x.AlbumStarEntities, then => then.Where(y => y.UserId == userId))
            .IncludeMany(x => x.TrackEntities!.Select(y => new TrackEntity()
                {
                    Id = y.Id,
                    Duration = y.Duration,
                    Size = y.Size
                }),
                then => then.Where(y =>
                    y.DirectoryEntity!.IsAccessControlled == false ||
                    y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)));
    }
    
    
    public async Task<AlbumWithSongsID3> GetAlbumAsync(Guid userId, int albumId, CancellationToken cancellationToken)
    {
        var album = await GetAlbumBase(userId, null).FirstAsync(cancellationToken);

        if (album == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        var tracks = await TrackEntity.Where(x => x.AlbumId == albumId && (x.DirectoryEntity!.IsAccessControlled == false ||
                                                        x.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .IncludeMany(x => x.TrackStarEntities, then => then.Where(y => y.UserId == userId))
            .IncludeMany(x => x.ArtistEntities)
            .Include(x => x.AlbumEntity)
            .Include(x => x.GenreEntity)
            .ToListAsync(cancellationToken);

        if (tracks == null || tracks.Count == 0)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        var albumId3 = new AlbumWithSongsID3()
        {
            id = album.Id.ToAlbumId(),
            name = album.Title ?? ArtistServiceLocalizer["NoAlbumName"],
            artist = album.ArtistEntity?.Name ?? ArtistServiceLocalizer["NoArtistName"],
            artistId = album.ArtistId.ToArtistId(),
            coverArt = album.PictureId.ToString(),
            songCount = album.TrackEntities?.Count ?? 0,
            duration = album.TrackEntities?.Sum(y => (int)y.Duration) ?? 0,
            playCount = album.PlayCount,
            playCountSpecified = true,
            created = album.CreateTime,
            starred = album.AlbumStarEntities?.Count > 0 ? album.AlbumStarEntities.First().CreateTime : default,
            starredSpecified = album.AlbumStarEntities?.Count > 0,
            year = album.Year ?? 0,
            yearSpecified = album.Year.HasValue,
            genre = album.GenreEntity?.Name ?? "",
            song = tracks.Select(x => x.CreateTrackChild()).ToArray()
            
        };

        return albumId3;
    }

    public async Task<QueryData<AlbumEntity>> QueryAlbumRandomAsync(Guid userId, int? musicFolderId, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            
            .OrderByRandom()
            .Take(count)
            .ToListAsync(cancellationToken);
        
        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = albums.Count
        };
    }

    public async Task<QueryData<AlbumEntity>> QueryAlbumNewestAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderByDescending(x => x.CreateTime)
            .Count(out var totalCount)
            .OrderBy(x => x.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);
        
        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = (int)totalCount
        };
    }

    public async Task<QueryData<AlbumEntity>> QueryAlbumFrequentAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderByDescending(x => x.PlayCount)
            .OrderBy(x => x.Id)
            .Count(out var totalCount)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = (int)totalCount
        };
    }
    
    public async Task<QueryData<AlbumEntity>> QueryAlbumRecentAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderByDescending(x => x.LatestPlayTime)
            .OrderBy(x => x.Id)
            .Count(out var totalCount)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = (int)totalCount
        };
    }
    
    public async Task<QueryData<AlbumEntity>> QueryAlbumOrderedByAlbumTitleAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderBy(x => x.Title)
            .OrderBy(x => x.Id)
            .Count(out var totalCount)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = (int)totalCount
        };
    }
    
    public async Task<QueryData<AlbumEntity>> QueryAlbumOrderedByArtistNameAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .Where(x => x.ArtistEntity != null)
            .OrderBy(x => x.ArtistEntity!.Name)
            .OrderBy(x => x.Id)
            .Count(out var totalCount)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = (int)totalCount
        };
    }
    
    public async Task<QueryData<AlbumEntity>> QueryAlbumStarredAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderBy(x => x.Id)
            .Count(out var totalCount)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = (int)totalCount
        };
    }
    
    public async Task<QueryData<AlbumEntity>> QueryAlbumByYearAsync(Guid userId, int? musicFolderId, int offset, int count, int fromYear, int toYear, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .Where(x => x.Year >= fromYear && x.Year <= toYear)
            .OrderBy(x => x.Id)
            .Count(out var totalCount)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = (int)totalCount
        };
    }
    
    public async Task<QueryData<AlbumEntity>> QueryAlbumByGenreAsync(Guid userId, int? musicFolderId, int offset, int count, string genre, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .Where(x => x.GenreEntity!.Name == genre)
            .OrderBy(x => x.Id)
            .Count(out var totalCount)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new QueryData<AlbumEntity>()
        {
            Items = albums,
            TotalCount = (int)totalCount
        };
    }
    
    public async Task<AlbumID3[]> GetStar2AlbumsId3(Guid userId, int? musicFolderId, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .Where(x => x.AlbumStarEntities!.Any(y => y.UserId == userId))
            .ToListAsync(cancellationToken);
        
        return albums.Select(x => x.CreateAlbumId3()).ToArray();
    }

    public async Task<AlbumID3[]> GetSearch3AlbumId3(Guid userId, int? musicFolderId, string query, int albumCount, int albumOffset, CancellationToken cancellationToken)
    {
        if (albumCount == 0)
        {
            return [];
        }

        var albums = await GetAlbumBase(userId, musicFolderId)
            .Where(x => x.Title!.Contains(query))
            .Skip(albumOffset)
            .Take(albumCount)
            .ToListAsync(cancellationToken);

        return albums.Select(x => x.CreateAlbumId3()).ToArray();
    }
    
    public async Task StarAlbumAsync(Guid? userId, List<int> albumIds, CancellationToken cancellationToken)
    {
        var count = await AlbumEntity.Where(x => albumIds.Contains(x.Id)).CountAsync(cancellationToken);
        if (count != albumIds.Count)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        
        var albumStarEntities = albumIds.Select(x => new AlbumStarEntity()
        {
            UserId = userId!.Value,
            AlbumId = x,
            CreateTime = DateTime.Now
        });

        await BaseEntity.Orm.InsertOrUpdate<AlbumStarEntity>().SetSource(albumStarEntities)
            .ExecuteAffrowsAsync(cancellationToken);
    }
    
    public async Task UnStarAlbumAsync(Guid? userId, List<int> albumIds, CancellationToken cancellationToken)
    {
         await BaseEntity.Orm.Select<AlbumStarEntity>().Where(x => albumIds.Contains(x.AlbumId) && userId == x.UserId)
            .ToDelete()
            .ExecuteAffrowsAsync(cancellationToken);
    }

    public async Task SetAlbumRatingAsync(Guid userId, int albumId, float rating, CancellationToken cancellationToken)
    {
        var album = await AlbumEntity.FindAsync(albumId);

        if (album == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        if (rating == 0)
        {
            await BaseEntity.Orm.Delete<AlbumRatingEntity>().Where(x => x.AlbumId == albumId && x.UserId == userId)
                .ExecuteAffrowsAsync(cancellationToken);
        }
        else
        {
            var albumRatingEntity = new AlbumRatingEntity()
            {
                AlbumId = albumId,
                UserId = userId,
                Rating = rating,
                CreateTime = DateTime.Now
            };

            await BaseEntity.Orm.InsertOrUpdate<AlbumRatingEntity>().SetSource(albumRatingEntity)
                .ExecuteAffrowsAsync(cancellationToken);
        }
    }

    public async Task<QueryData<AlbumEntity>> QueryData(QueryPageOptions options, DynamicFilterInfo dynamicFilterInfo, Guid userId, bool starOnly)
    {
        var select = GetAlbumBase(userId, null)
            .WhereIf(starOnly, x => x.AlbumStarEntities.Any(y => y.UserId == userId))
            .WhereDynamicFilter(dynamicFilterInfo)
            .OrderByPropertyNameIf(options.SortOrder != SortOrder.Unset, options.SortName,
                options.SortOrder == SortOrder.Asc)
            .Count(out var count);
        if (options.IsPage)
        {
            select.Page(options.PageIndex, options.PageItems);
        }

        var data = await select.ToListAsync();
        
        return new QueryData<AlbumEntity>()
        {
            TotalCount = (int)count,
            Items = data,
            IsSorted = options.SortOrder != SortOrder.Unset,
            IsFiltered = options.Filters.Any(),
            IsAdvanceSearch = options.AdvanceSearches.Any(),
            IsSearch = options.Searches.Any() || options.CustomerSearches.Any()
        };
    }

    public async Task<AlbumEntity> GetAlbumById(int albumId, Guid userId, CancellationToken cancellationToken)
    {
        var album = await GetAlbumBase(userId, null)
            .Where(x => x.Id == albumId)
            .FirstAsync(cancellationToken);
        return album;
    }
    
    public async Task<List<TrackEntity>> GetTracksByAlbumIdAsync(int albumId, Guid userId, CancellationToken cancellationToken)
    {
        var tracks = await TrackEntity.Where(x => x.AlbumId == albumId && (x.DirectoryEntity!.IsAccessControlled == false ||
                                                                           x.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .IncludeMany(x => x.TrackStarEntities, then => then.Where(y => y.UserId == userId))
            .IncludeMany(x => x.ArtistEntities)
            .Include(x => x.LrcEntity)
            .Include(x => x.AlbumEntity)
            .Include(x => x.GenreEntity)
            .ToListAsync(cancellationToken);

        return tracks;
    }
}