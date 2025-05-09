﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
public class ArtistService(IStringLocalizer<ArtistService> artistServiceLocalizer)
{
    private ISelect<ArtistEntity> GetArtistBase(Guid userId, int? musicFolderId)
    {
        return ArtistEntity
            .WhereIf(musicFolderId != null, x => x.TrackEntities!.Any(y => y.DirectoryId == musicFolderId))
            .IncludeMany(x => x.ArtistStarEntities, then => then.Where(y => y.UserId == userId))
            .IncludeMany(x => x.AlbumEntities!.Select(y => new AlbumEntity()
            {
                Id = x.Id
            }));
    }
    
    public async Task<ArtistsID3> GetArtistsAsync(Guid userId, int? musicFolderId, long? ifModifiedSince, CancellationToken cancellationToken)
    {
        var artist = await GetArtistBase(userId, musicFolderId)
            .WhereIf(ifModifiedSince != null,
                x => x.CreateTime > DateTimeOffset.FromUnixTimeMilliseconds(ifModifiedSince!.Value))
            .ToListAsync(cancellationToken);
        var id3List = artist.Select(x => x.CreateArtistId3())
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

    public async Task<ArtistWithAlbumsID3> GetArtistAsync(Guid userId, int artistId, CancellationToken cancellationToken)
    {
        var id3 = await ArtistEntity.Where(x => x.Id == artistId)
            .IncludeMany(x => x.ArtistStarEntities, then => then.Where(y => y.UserId == userId))
            .FirstAsync(cancellationToken);

        if (id3 == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        var albums = await AlbumEntity.Where(x => x.ArtistId == artistId && x.TrackEntities!.Any(y => 
                y.DirectoryEntity!.IsAccessControlled == false || y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .IncludeMany(x => x.TrackEntities, then => then.Where(y => y.DirectoryEntity!.IsAccessControlled == false || y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .IncludeMany(x => x.AlbumStarEntities, then => then.Where(y => y.UserId == userId))
            .Include(x => x.ArtistEntity)
            .OrderBy(x => x.CreateTime)
            .OrderBy(x => x.Title)
            .OrderBy(x => x.ArtistId)
            .ToListAsync(cancellationToken);

        if (albums == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        
        var albumId3 = albums
            .Select(x => x.CreateAlbumId3());

        return new ArtistWithAlbumsID3()
        {
            album = albumId3.ToArray(),
            id = id3.Id.ToArtistId(),
            name = id3.Name ?? artistServiceLocalizer["NoArtistName"],
            starred = id3.ArtistStarEntities?.Count > 0 ? id3.ArtistStarEntities.First().CreateTime : default,
            starredSpecified = id3.ArtistStarEntities?.Count > 0,
            coverArt = null,
            albumCount = albums.Count
        };
    }

    public async Task<ArtistID3[]> GetStar2ArtistsId3(Guid userId, int? musicFolderId, CancellationToken cancellationToken)
    {
        var artist = await GetArtistBase(userId, musicFolderId)
            .Where(x => x.ArtistStarEntities!.Any(y => y.UserId == userId))
            .ToListAsync(cancellationToken);
        
        return artist.Select(x => x.CreateArtistId3()).ToArray();
    }
    
    public async Task<List<ArtistEntity>> GetSearch3ArtistId3(Guid userId, int? musicFolderId, string query, int artistCount, int artistOffset, CancellationToken cancellationToken)
    {
        if (artistCount == 0)
        {
            return [];
        }

        var artists = await GetArtistBase(userId, musicFolderId)
            .Where(x => x.Name!.Contains(query))
            .Skip(artistOffset)
            .Take(artistCount)
            .ToListAsync(cancellationToken);

        return artists;
    }

    public async Task StarArtistAsync(Guid? userId, List<int> artistIds, CancellationToken cancellationToken)
    {
        var count = await ArtistEntity.Where(x => artistIds.Contains(x.Id)).CountAsync(cancellationToken);
        if (count != artistIds.Count)
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        
        var artistStarEntities = artistIds.Select(x => new ArtistStarEntity()
        {
            UserId = userId!.Value,
            ArtistId = x,
            CreateTime = DateTime.Now
        });

        await BaseEntity.Orm.InsertOrUpdate<ArtistStarEntity>().SetSource(artistStarEntities)
            .ExecuteAffrowsAsync(cancellationToken);
    }
    
    public async Task UnStarArtistAsync(Guid? userId, List<int> artistIds, CancellationToken cancellationToken)
    {
        await BaseEntity.Orm.Select<ArtistStarEntity>().Where(x => artistIds.Contains(x.ArtistId) && userId == x.UserId)
            .ToDelete()
            .ExecuteAffrowsAsync(cancellationToken);
    }

    public async Task SetArtistRatingAsync(Guid userId, int artistId, float rating, CancellationToken cancellationToken)
    {
        var artist = await ArtistEntity.FindAsync(artistId);
        if (artist == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        if (rating == 0)
        {
            await BaseEntity.Orm.Delete<ArtistRatingEntity>().Where(x => x.ArtistId == artistId && x.UserId == userId)
                .ExecuteAffrowsAsync(cancellationToken);
        }
        else
        {
            var artistStarEntity = new ArtistRatingEntity()
            {
                UserId = userId,
                ArtistId = artistId,
                Rating = rating,
                CreateTime = DateTime.Now
            };
            await BaseEntity.Orm.InsertOrUpdate<ArtistRatingEntity>().SetSource(artistStarEntity)
                .ExecuteAffrowsAsync(cancellationToken);
        }
    }
    
    public async Task<QueryData<ArtistEntity>> QueryData(QueryPageOptions options, DynamicFilterInfo dynamicFilterInfo,
        Guid userId, bool starOnly)
    {
        var select = GetArtistBase(userId, null)
            .WhereIf(starOnly, x => x.ArtistStarEntities!.Any(y => y.UserId == userId))
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
        
        return new QueryData<ArtistEntity>()
        {
            TotalCount = (int)count,
            Items = data,
            IsSorted = options.SortOrder != SortOrder.Unset,
            IsFiltered = true,
            IsAdvanceSearch = true,
            IsSearch = true
        };
    }

    public async Task<ArtistEntity> GetArtistById(int artistId, Guid userId, CancellationToken cancellationToken)
    {
        return await GetArtistBase(userId, null)
            .Where(x => x.Id == artistId)
            .FirstAsync(cancellationToken);
    }
    
    public async Task<List<TrackEntity>> GetTracksByArtistIdAsync(int artistId, Guid userId, CancellationToken cancellationToken)
    {
        var tracks = await TrackEntity.Where(x => x.ArtistEntities!.Any(y => y.Id == artistId) && (x.DirectoryEntity!.IsAccessControlled == false ||
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