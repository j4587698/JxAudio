﻿using System.Diagnostics.CodeAnalysis;
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

    public async Task<Child[]> GetSearch3Songs(Guid userId, int? musicFolderId, string query,int songCount, int songOffset, CancellationToken cancellationToken)
    {
        if (songCount == 0)
        {
            return Array.Empty<Child>();
        }

        var tracks = await GetTrackBase(userId, musicFolderId)
            .Where(x => x.Title!.Contains(query))
            .Skip(songOffset)
            .Take(songCount)
            .ToListAsync(cancellationToken);

        return tracks.Select(x => x.CreateTrackChild()).ToArray();
    }
}