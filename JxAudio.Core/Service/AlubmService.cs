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
                    Duration = y.Duration
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

    public async Task<AlbumList2> GetAlbumList2RandomAsync(Guid userId, int? musicFolderId, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderByRandom()
            .Take(count)
            .ToListAsync(cancellationToken);

        return new AlbumList2()
        {
            album = albums.Select(x => x.CreateAlbumId3()).ToArray()
        };
    }

    public async Task<AlbumList2> GetAlbumList2NewestAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderByDescending(x => x.CreateTime)
            .OrderBy(x => x.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new AlbumList2()
        {
            album = albums.Select(x => x.CreateAlbumId3()).ToArray()
        };
    }

    public async Task<AlbumList2> GetAlbumList2FrequentAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderByDescending(x => x.PlayCount)
            .OrderBy(x => x.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new AlbumList2()
        {
            album = albums.Select(x => x.CreateAlbumId3()).ToArray()
        };
    }
    
    public async Task<AlbumList2> GetAlbumList2RecentAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderByDescending(x => x.LatestPlayTime)
            .OrderBy(x => x.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new AlbumList2()
        {
            album = albums.Select(x => x.CreateAlbumId3()).ToArray()
        };
    }
    
    public async Task<AlbumList2> GetAlbumList2OrderedByAlbumTitleAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderBy(x => x.Title)
            .OrderBy(x => x.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new AlbumList2()
        {
            album = albums.Select(x => x.CreateAlbumId3()).ToArray()
        };
    }
    
    public async Task<AlbumList2> GetAlbumList2OrderedByArtistNameAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .Where(x => x.ArtistEntity != null)
            .OrderBy(x => x.ArtistEntity!.Name)
            .OrderBy(x => x.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new AlbumList2()
        {
            album = albums.Select(x => x.CreateAlbumId3()).ToArray()
        };
    }
    
    public async Task<AlbumList2> GetAlbumList2StarredAsync(Guid userId, int? musicFolderId, int offset, int count, CancellationToken cancellationToken)
    {
        var albums = await GetAlbumBase(userId, musicFolderId)
            .OrderBy(x => x.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new AlbumList2()
        {
            album = albums.Where(x => x.AlbumStarEntities is { Count: > 0 })
                .Select(x => x.CreateAlbumId3()).ToArray()
        };
    }
}