using JxAudio.Core.Entity;
using JxAudio.Core.Service;
using JxAudio.Core.Subsonic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JxAudio.Core.Extensions;

public static class SubsonicExtension
{
    public static string ToArtistId(this int id)
    {
        return $"r{id}";
    }
    
    public static string ToAlbumId(this int id)
    {
        return $"a{id}";
    }
    
    public static string ToTrackId(this int id)
    {
        return $"t{id}";
    }
    
    public static string ToPlaylistId(this int id)
    {
        return $"p{id}";
    }
    
    public static bool TryParseArtistId(this string id, out int result)
    {
        if (id.StartsWith('r') && int.TryParse(id[1..], out result))
        {
            return true;
        }

        result = default;
        return false;
    }
    
    public static bool TryParseAlbumId(this string id, out int result)
    {
        if (id.StartsWith('a') && int.TryParse(id[1..], out result))
        {
            return true;
        }

        result = default;
        return false;
    }
    
    public static bool TryParseTrackId(this string id, out int result)
    {
        if (id.StartsWith('t') && int.TryParse(id[1..], out result))
        {
            return true;
        }

        result = default;
        return false;
    }
    
    public static bool TryParsePlaylistId(this string id, out int result)
    {
        if (id.StartsWith('p') && int.TryParse(id[1..], out result))
        {
            return true;
        }

        result = default;
        return false;
    }
    
    public static int ParseArtistId(this string id)
    {
        if (TryParseArtistId(id, out var result))
        {
            return result;
        }

        throw RestApiErrorException.InvalidParameterError(nameof(id));
    }
    
    public static int ParseAlbumId(this string id)
    {
        if (TryParseAlbumId(id, out var result))
        {
            return result;
        }

        throw RestApiErrorException.InvalidParameterError(nameof(id));
    }
    
    public static int ParseTrackId(this string id)
    {
        if (TryParseTrackId(id, out var result))
        {
            return result;
        }

        throw RestApiErrorException.InvalidParameterError(nameof(id));
    }
    
    public static int ParsePlaylistId(this string id)
    {
        if (TryParsePlaylistId(id, out var result))
        {
            return result;
        }

        throw RestApiErrorException.InvalidParameterError(nameof(id));
    }

    public static Child CreateDirectoryChild(this AlbumID3 album)
    {
        return new Child()
        {
            id = album.id,
            parent = album.artistId,
            isDir = true,
            title = album.name,
            album = null,
            artist = album.artist,
            track = default,
            trackSpecified = false,
            year = album.year,
            yearSpecified = album.yearSpecified,
            genre = album.genre,
            coverArt = album.coverArt,
            size = default,
            sizeSpecified = false,
            contentType = null,
            suffix = null,
            transcodedContentType = null,
            transcodedSuffix = null,
            duration = album.duration,
            durationSpecified = true,
            bitRate = default,
            bitRateSpecified = false,
            path = null,
            isVideo = default,
            isVideoSpecified = false,
            userRating = default,
            userRatingSpecified = false,
            averageRating = default,
            averageRatingSpecified = false,
            playCount = default,
            playCountSpecified = false,
            discNumber = default,
            discNumberSpecified = false,
            created = default,
            createdSpecified = false,
            starred = album.starred,
            starredSpecified = album.starredSpecified,
            albumId = album.id,
            artistId = album.artistId,
            type = default,
            typeSpecified = false,
            bookmarkPosition = default,
            bookmarkPositionSpecified = false,
            originalWidth = default,
            originalWidthSpecified = false,
            originalHeight = default,
            originalHeightSpecified = false,
        };
    }

    public static Child CreateTrackChild(this TrackEntity trackEntity)
    {
        var artistServiceLocalizer = Application.GetRequiredService<IStringLocalizer<ArtistService>>();
        return new Child()
        {
            id = trackEntity.Id.ToTrackId(),
            isDir = false,
            parent = default,
            title = trackEntity.Title ?? artistServiceLocalizer!["NoTrackName"],
            album = trackEntity.AlbumEntity?.Title ?? artistServiceLocalizer!["NoAlbumName"],
            artist = trackEntity.ArtistEntities == null
                ? artistServiceLocalizer!["NoArtistName"]
                : string.Join(",", trackEntity.ArtistEntities.Select(y => y.Name)),
            track = trackEntity.TrackNumber ?? int.MaxValue,
            trackSpecified = trackEntity.TrackNumber != 0,
            year = trackEntity.AlbumEntity?.Year ?? 0,
            yearSpecified = trackEntity.AlbumEntity?.Year.HasValue ?? false,
            genre = trackEntity.GenreEntity?.Name ?? "",
            coverArt = trackEntity.AlbumEntity?.PictureId.ToString(),
            size = trackEntity.Size,
            sizeSpecified = true,
            contentType = trackEntity.MimeType ?? "",
            suffix = Path.GetExtension(trackEntity.FullName)?.TrimStart('.'),
            duration = (int)Math.Round(trackEntity.Duration),
            durationSpecified = true,
            bitRate = trackEntity.BitRate ?? 0,
            bitRateSpecified = trackEntity.BitRate.HasValue,
            path = default,
            isVideo = false,
            isVideoSpecified = false,
            userRating = default,
            userRatingSpecified = false,
            averageRating = default,
            averageRatingSpecified = false,
            playCount = trackEntity.PlayCount,
            playCountSpecified = true,
            discNumber = trackEntity.DiscNumber ?? 0,
            discNumberSpecified = trackEntity.DiscNumber.HasValue,
            created = trackEntity.CreateTime,
            createdSpecified = true,
            starred = trackEntity.TrackStarEntities?.Count > 0
                ? trackEntity.TrackStarEntities.First().CreateTime
                : default,
            starredSpecified = trackEntity.TrackStarEntities?.Count > 0,
            albumId = trackEntity.AlbumId?.ToAlbumId() ?? default,
            artistId = trackEntity.AlbumEntity?.ArtistId.ToArtistId() ?? default,
            type = MediaType.music,
            typeSpecified = true,
            bookmarkPosition = default,
            bookmarkPositionSpecified = false,
            originalWidth = default,
            originalHeightSpecified = false,
            originalHeight = default,
            originalWidthSpecified = false
        };
    }

    public static AlbumID3 CreateAlbumId3(this AlbumEntity albumEntity)
    {
        var artistServiceLocalizer = Application.GetRequiredService<IStringLocalizer<ArtistService>>();
        return new AlbumID3()
        {
            id = albumEntity.Id.ToAlbumId(),
            name = albumEntity.Title ?? artistServiceLocalizer!["NoAlbumName"],
            artist = albumEntity.ArtistEntity?.Name ?? artistServiceLocalizer!["NoArtistName"],
            artistId = albumEntity.ArtistId.ToArtistId(),
            coverArt = albumEntity.PictureId.ToString(),
            songCount = albumEntity.TrackEntities?.Count ?? 0,
            duration = albumEntity.TrackEntities?.Sum(y => (int)y.Duration) ?? 0,
            playCount = default,
            playCountSpecified = false,
            created = albumEntity.CreateTime,
            starred = albumEntity.AlbumStarEntities?.Count > 0
                ? albumEntity.AlbumStarEntities.First().CreateTime
                : default,
            starredSpecified = albumEntity.AlbumStarEntities?.Count > 0,
            year = albumEntity.Year ?? 0,
            yearSpecified = albumEntity.Year.HasValue,
            genre = albumEntity.GenreEntity?.Name ?? ""
        };
    }
    
}