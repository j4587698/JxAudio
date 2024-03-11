using JxAudio.Core.Subsonic;

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

        throw RestApiErrorException.GenericError($"Invalid id for '{id}'.");
    }
    
    public static int ParseAlbumId(this string id)
    {
        if (TryParseAlbumId(id, out var result))
        {
            return result;
        }

        throw RestApiErrorException.GenericError($"Invalid id for '{id}'.");
    }
    
    public static int ParseTrackId(this string id)
    {
        if (TryParseTrackId(id, out var result))
        {
            return result;
        }

        throw RestApiErrorException.GenericError($"Invalid id for '{id}'.");
    }
    
    public static int ParsePlaylistId(this string id)
    {
        if (TryParsePlaylistId(id, out var result))
        {
            return result;
        }

        throw RestApiErrorException.GenericError($"Invalid id for '{id}'.");
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

    public static Child CreateDirectoryChild(this Child song)
    {
        return new Child()
        {
            id = song.id,
            parent = "d" + song.artistId,
            isDir = song.isDir,
            title = song.title,
            album = song.album,
            artist = song.artist,
            track = song.track,
            trackSpecified = song.trackSpecified,
            year = song.year,
            yearSpecified = song.yearSpecified,
            genre = song.genre,
            coverArt = song.coverArt,
            size = song.size,
            sizeSpecified = song.sizeSpecified,
            contentType = song.contentType,
            suffix = song.suffix,
            transcodedContentType = song.transcodedContentType,
            transcodedSuffix = song.transcodedSuffix,
            duration = song.duration,
            durationSpecified = song.durationSpecified,
            bitRate = song.bitRate,
            bitRateSpecified = song.bitRateSpecified,
            path = song.path,
            isVideo = song.isVideo,
            isVideoSpecified = song.isVideoSpecified,
            userRating = song.userRating,
            userRatingSpecified = song.userRatingSpecified,
            averageRating = song.averageRating,
            averageRatingSpecified = song.averageRatingSpecified,
            playCount = song.playCount,
            playCountSpecified = song.playCountSpecified,
            discNumber = song.discNumber,
            discNumberSpecified = song.discNumberSpecified,
            created = song.created,
            createdSpecified = song.createdSpecified,
            starred = song.starred,
            starredSpecified = song.starredSpecified,
            albumId = song.albumId,
            artistId = song.artistId,
            type = song.type,
            typeSpecified = song.typeSpecified,
            bookmarkPosition = song.bookmarkPosition,
            bookmarkPositionSpecified = song.bookmarkPositionSpecified,
            originalWidth = song.originalWidth,
            originalWidthSpecified = song.originalWidthSpecified,
            originalHeight = song.originalHeight,
            originalHeightSpecified = song.originalHeightSpecified,
        };
    }
}