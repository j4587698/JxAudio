﻿using System.Diagnostics.CodeAnalysis;
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
public class TrackService
{

    private ISelect<TrackEntity> GetTrackBase(Guid userId, int? musicFolderId)
    {
        return TrackEntity.Where(x => x.DirectoryEntity!.IsAccessControlled == false || 
                                      x.DirectoryEntity!.UserEntities!.Any(z => z.Id == userId))
            .WhereIf(musicFolderId != null, x => x.DirectoryId == musicFolderId)
            .Include(x => x.AlbumEntity)
            .Include(x => x.GenreEntity)
            .IncludeMany(x => x.ArtistEntities)
            .IncludeMany(x => x.TrackStarEntities, 
                then => then.Where(y => y.UserId == userId));
    }

    public async Task<TrackEntity> GetSongEntityAsync(Guid userId, int trackId, CancellationToken cancellationToken)
    {
        var track = await GetTrackBase(userId, null)
            .Where(x => x.Id == trackId)
            .FirstAsync(cancellationToken);
        if (track == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        return track;
    }
    
    public async Task<Child> GetSongAsync(Guid userId, int trackId, CancellationToken cancellationToken)
    {
        var track = await GetTrackBase(userId, null)
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
        var songs = await GetTrackBase(userId, musicFolderId)
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

    public async Task<Songs> GetSongsByGenreAsync(Guid userId, int? musicFolderId, string genre, int offset, int count, CancellationToken cancellationToken)
    {
        var songs = await GetTrackBase(userId, musicFolderId)
            .Where(x => x.GenreEntity!.Name == genre)
            .Skip(offset)
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

    public async Task<Child[]> GeStar2Songs(Guid userId, int? musicFolderId, CancellationToken cancellationToken)
    {
        var tracks = await GetTrackBase(userId, musicFolderId)
            .Where(x => x.TrackStarEntities!.Any(y => y.UserId == userId))
            .ToListAsync(cancellationToken);

        return tracks.Select(x => x.CreateTrackChild()).ToArray();
    }

    public async Task<List<TrackEntity>> GetSearch3Songs(Guid userId, int? musicFolderId, string query,int songCount, int songOffset, CancellationToken cancellationToken)
    {
        if (songCount == 0)
        {
            return [];
        }

        var tracks = await GetTrackBase(userId, musicFolderId)
            .Where(x => x.Title!.Contains(query))
            .Skip(songOffset)
            .Take(songCount)
            .ToListAsync(cancellationToken);

        return tracks;
    }
    
    public async Task StarTrackAsync(Guid? userId, List<int> trackIds, CancellationToken cancellationToken)
    {
        var count = await TrackEntity.Where(x => trackIds.Contains(x.Id)).CountAsync(cancellationToken);
        if (count != trackIds.Count)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        
        var trackStarEntities = trackIds.Select(x => new TrackStarEntity()
        {
            UserId = userId!.Value,
            TrackId = x,
            CreateTime = DateTime.Now
        });

        await BaseEntity.Orm.InsertOrUpdate<TrackStarEntity>().SetSource(trackStarEntities)
            .ExecuteAffrowsAsync(cancellationToken);
    }
    
    public async Task UnStarTrackAsync(Guid? userId, List<int> trackIds, CancellationToken cancellationToken)
    {
        await BaseEntity.Orm.Select<TrackStarEntity>().Where(x => trackIds.Contains(x.TrackId) && userId == x.UserId)
            .ToDelete()
            .ExecuteAffrowsAsync(cancellationToken);
    }

    public async Task<Child[]> GetTopSongsAsync(Guid userId, string artistName, int count, CancellationToken cancellationToken)
    {
        var tracks = await GetTrackBase(userId, null).Where(x => x.ArtistEntities!.Any(y => y.Name == artistName))
            .OrderByDescending(x => x.PlayCount)
            .Take(count)
            .ToListAsync(cancellationToken);
        
        return tracks.Select(x => x.CreateTrackChild()).ToArray();
    }
    
    public async Task SetTrackRatingAsync(Guid userId, int trackId, float rating, CancellationToken cancellationToken)
    {
        var track = await TrackEntity.FindAsync(trackId);
        if (track == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        if (rating == 0)
        {
            await BaseEntity.Orm.Delete<TrackRatingEntity>().Where(x => x.TrackId == trackId && x.UserId == userId)
                .ExecuteAffrowsAsync(cancellationToken);
        }
        else
        {
            var trackRatingEntity = new TrackRatingEntity()
            {
                UserId = userId,
                TrackId = trackId,
                Rating = rating
            };

            await BaseEntity.Orm.InsertOrUpdate<TrackRatingEntity>().SetSource(trackRatingEntity)
                .ExecuteAffrowsAsync(cancellationToken);
        }

        track.Save();
    }

    public async Task UpdatePlayCountAsync(int trackId, CancellationToken cancellationToken)
    {
        var track = await TrackEntity.Where(x => x.Id == trackId)
            .Include(x => x.AlbumEntity)
            .FirstAsync(cancellationToken);
        if (track == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        
        track.PlayCount += 1;
        if (track.AlbumEntity != null)
        {
            track.AlbumEntity.PlayCount += 1;
            track.AlbumEntity.LatestPlayTime = DateTime.Now;
            await track.AlbumEntity.SaveAsync();
        }

        await track.SaveAsync();
    }
    
    public async Task<QueryData<TrackEntity>> QueryData(QueryPageOptions options, DynamicFilterInfo dynamicFilterInfo, 
        Guid userId, bool starOnly)
    {
        var select = GetTrackBase(userId, null)
            .WhereIf(starOnly, x => x.TrackStarEntities!.Any(y => y.UserId == userId))
            .WhereDynamicFilter(dynamicFilterInfo)
            .OrderByPropertyNameIf(options.SortOrder != SortOrder.Unset, options.SortName,
                options.SortOrder == SortOrder.Asc)
            .Count(out var count);
        if (options.IsPage)
        {
            select.Page(options.PageIndex, options.PageItems);
        }

        var data = await select.ToListAsync();
        
        return new QueryData<TrackEntity>()
        {
            TotalCount = (int)count,
            Items = data,
            IsSorted = options.SortOrder != SortOrder.Unset,
            IsFiltered = options.Filters.Any(),
            IsAdvanceSearch = options.AdvanceSearches.Any(),
            IsSearch = options.Searches.Any() || options.CustomerSearches.Any()
        };
    }

    public async Task<List<TrackEntity>> Search(string searchText, Guid userId, CancellationToken cancellationToken)
    {
        return await GetTrackBase(userId, null)
            .Where(x => x.Name!.Contains(searchText) || x.AlbumEntity!.Title!.Contains(searchText) ||
                        x.ArtistEntities!.Any(y => y.Name!.Contains(searchText)))
            .ToListAsync(cancellationToken);
    }
    
    public async Task<QueryData<TrackEntity>> QueryNewestTrackAsync(Guid userId, int? musicFolderId, int offset, 
        int count, CancellationToken cancellationToken)
    {
        var select = await GetTrackBase(userId, musicFolderId)
            .OrderByDescending(x => x.CreateTime)
            .Skip(offset)
            .Take(count)
            .Count(out var totalCount)
            .ToListAsync(cancellationToken);

        return new QueryData<TrackEntity>()
        {
            TotalCount = (int)totalCount,
            Items = select,
            IsSorted = true,
            IsFiltered = false,
            IsAdvanceSearch = false,
            IsSearch = false
        };
    }
    
    public async Task<QueryData<TrackEntity>> QueryFrequentTrackAsync(Guid userId, int? musicFolderId, int offset, 
        int count, CancellationToken cancellationToken)
    {
        var select = await GetTrackBase(userId, musicFolderId)
            .OrderByDescending(x => x.PlayCount)
            .Skip(offset)
            .Take(count)
            .Count(out var totalCount)
            .ToListAsync(cancellationToken);

        return new QueryData<TrackEntity>()
        {
            TotalCount = (int)totalCount,
            Items = select,
            IsSorted = true,
            IsFiltered = false,
            IsAdvanceSearch = false,
            IsSearch = false
        };
    }
    
    public async Task<QueryData<TrackEntity>> QueryRecentTrackAsync(Guid userId, int? musicFolderId, int offset,
        int count, CancellationToken cancellationToken)
    {
        var select = await GetTrackBase(userId, musicFolderId)
            .OrderByDescending(x => x.UpdateTime)
            .Skip(offset)
            .Take(count)
            .Count(out var totalCount)
            .ToListAsync(cancellationToken);

        return new QueryData<TrackEntity>()
        {
            TotalCount = (int)totalCount,
            Items = select,
            IsSorted = true,
            IsFiltered = false,
            IsAdvanceSearch = false,
            IsSearch = false
        };
    }
    
    public async Task<QueryData<TrackEntity>> QueryRandomTrackAsync(Guid userId, int? musicFolderId, int count, 
        CancellationToken cancellationToken)
    {
        var select = await GetTrackBase(userId, musicFolderId)
            .OrderByRandom()
            .Take(count)
            .Count(out var totalCount)
            .ToListAsync(cancellationToken);

        return new QueryData<TrackEntity>()
        {
            TotalCount = (int)totalCount,
            Items = select,
            IsSorted = true,
            IsFiltered = false,
            IsAdvanceSearch = false,
            IsSearch = false
        };
    }
}