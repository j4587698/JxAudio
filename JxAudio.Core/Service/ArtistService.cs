using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using JxAudio.Core.Subsonic;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace JxAudio.Core.Service;

[Transient]
public class ArtistService
{
    [Inject]
    [NotNull]
    private IStringLocalizer<ArtistService>? ArtistServiceLocalizer { get; set; }
    
    public async Task<ArtistsID3> GetArtistsAsync(Guid userId, int? musicFolderId, long? ifModifiedSince, CancellationToken cancellationToken)
    {
        var artist = await ArtistEntity.Select
            .WhereIf(musicFolderId != null, x => x.TrackEntities!.Any(y => y.DirectoryId == musicFolderId))
            .WhereIf(ifModifiedSince != null, x => x.CreateTime > DateTimeOffset.FromUnixTimeMilliseconds(ifModifiedSince!.Value))
            .IncludeMany(x => x.ArtistStarEntities, then => then.Where(y => y.UserId == userId))
            .ToListAsync(cancellationToken);
        var id3List = artist.Select(x => new ArtistID3()
        {
            albumCount = (int)AlbumEntity.Select.Where(y => y.ArtistId == x.Id).Count(),
            coverArt = null,
            id = x.Id.ToString(),
            name = x.Name,
            starred = x.ArtistStarEntities?.Count > 0 ? x.ArtistStarEntities.First().CreateTime : default,
            starredSpecified = x.ArtistStarEntities?.Count > 0
        })
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
            .Select(x => new AlbumID3()
        {
            id = x.Id.ToAlbumId(),
            name = x.Title ?? ArtistServiceLocalizer["NoAlbumName"],
            artist = x.ArtistEntity?.Name ?? ArtistServiceLocalizer["NoArtistName"],
            artistId = x.ArtistId.ToArtistId(),
            coverArt = x.PictureId.ToString(),
            songCount = x.TrackEntities?.Count ?? 0,
            duration = x.TrackEntities?.Sum(y => (int)y.Duration) ?? 0,
            playCount = default,
            playCountSpecified = false,
            created = x.CreateTime,
            starred = x.AlbumStarEntities?.Count > 0 ? x.AlbumStarEntities.First().CreateTime : default,
            starredSpecified = x.AlbumStarEntities?.Count > 0,
            year = x.Year ?? 0,
            yearSpecified = x.Year.HasValue,
            genre = x.GenreEntity?.Name ?? ""
        });

        return new ArtistWithAlbumsID3()
        {
            album = albumId3.ToArray(),
            id = id3.Id.ToArtistId(),
            name = id3.Name ?? ArtistServiceLocalizer["NoArtistName"],
            starred = id3.ArtistStarEntities?.Count > 0 ? id3.ArtistStarEntities.First().CreateTime : default,
            starredSpecified = id3.ArtistStarEntities?.Count > 0,
            coverArt = null,
            albumCount = albums.Count
        };
    }
}