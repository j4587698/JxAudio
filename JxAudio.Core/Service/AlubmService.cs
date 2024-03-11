using System.Diagnostics.CodeAnalysis;
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
    
    public async Task<AlbumWithSongsID3> GetAlbumAsync(Guid userId, int albumId, CancellationToken cancellationToken)
    {
        var album = await AlbumEntity.Where(x => x.Id == albumId && x.TrackEntities!.Any(y => 
                y.DirectoryEntity!.IsAccessControlled == false || y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .IncludeMany(x => x.TrackEntities, then => then.Where(y => y.DirectoryEntity!.IsAccessControlled == false || y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .IncludeMany(x => x.AlbumStarEntities, then => then.Where(y => y.UserId == userId))
            .Include(x => x.ArtistEntity)
            .FirstAsync(cancellationToken);

        if (album == null)
        {
            throw RestApiErrorException.DataNotFoundError();
        }

        var tracks = await TrackEntity.Where(x => x.AlbumId == albumId && (x.DirectoryEntity!.IsAccessControlled == false ||
                                                        x.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .IncludeMany(x => x.TrackStarEntities, then => then.Where(y => y.UserId == userId))
            .IncludeMany(x => x.ArtistEntities)
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
            song = tracks.Select(x => new Child()
            {
                id = x.Id.ToTrackId(),
                isDir = false,
                parent = default,
                title = x.Title ?? ArtistServiceLocalizer["NoTrackName"],
                album = album.Title ?? ArtistServiceLocalizer["NoAlbumName"],
                artist = x.ArtistEntities == null ? ArtistServiceLocalizer["NoArtistName"] : string.Join(",", x.ArtistEntities.Select(y => y.Name)),
                track = x.TrackNumber ?? int.MaxValue,
                trackSpecified = x.TrackNumber != 0,
                year = album.Year ?? 0,
                yearSpecified = album.Year.HasValue,
                genre = album.GenreEntity?.Name ?? "",
                coverArt = album.PictureId.ToString(),
                size = x.Size,
                sizeSpecified = true,
                contentType = x.MimeType ?? "",
                suffix = Path.GetExtension(x.FullName)?.TrimStart('.'),
                duration = (int)Math.Round(x.Duration),
                durationSpecified = true,
                bitRate = x.BitRate ?? 0,
                bitRateSpecified = x.BitRate.HasValue,
                path = default,
                isVideo = false,
                isVideoSpecified = false,
                userRating = default,
                userRatingSpecified = false,
                averageRating = default,
                averageRatingSpecified = false,
                playCount = x.PlayCount,
                playCountSpecified = true,
                discNumber = x.DiscNumber ?? 0,
                discNumberSpecified = x.DiscNumber.HasValue,
                created = x.CreateTime,
                createdSpecified = true,
                starred = x.TrackStarEntities?.Count > 0 ? x.TrackStarEntities.First().CreateTime : default,
                starredSpecified = x.TrackStarEntities?.Count > 0,
                albumId = x.AlbumId?.ToAlbumId() ?? default,
                artistId = album.ArtistEntity?.Id.ToArtistId() ?? default,
                type = MediaType.music,
                typeSpecified = true,
                bookmarkPosition = default,
                bookmarkPositionSpecified = false,
                originalWidth = default,
                originalHeightSpecified = false,
                originalHeight = default,
                originalWidthSpecified = false
            }).ToArray()
            
        };

        return albumId3;
    }
}