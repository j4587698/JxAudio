using JxAudio.Core.Entity;
using JxAudio.TransVo;
using LrcParser.Model;
using Mapster;

namespace JxAudio.Web.Utils;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<AlbumEntity, AlbumVo>.NewConfig()
            .Map(dest => dest.Artist, src => src.ArtistEntity)
            .Map(dest => dest.Count, src => src.TrackEntities != null ? src.TrackEntities.Count : 0)
            .Map(dest => dest.CoverId, src => src.PictureId)
            .Map(dest => dest.Star, src => src.AlbumStarEntities != null && src.AlbumStarEntities.Count > 0)
            .Map(dest => dest.TotalTime, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Duration) : 0)
            .Map(dest => dest.TotalSize, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Size) : 0);

        TypeAdapterConfig<TrackEntity, TrackVo>.NewConfig()
            .Map(dest => dest.LrcId, src => src.LrcId)
            .Map(dest => dest.CoverId, src => src.PictureId)
            .Map(dest => dest.Lrc, src => LrcEntityToLrcVo(src.LrcEntity))
            .Map(dest => dest.Album, src => src.AlbumEntity)
            .Map(dest => dest.Star, src => src.TrackStarEntities != null && src.TrackStarEntities.Count > 0)
            .Map(dest => dest.Artists, src =>src.ArtistEntities);
        
        TypeAdapterConfig<ArtistEntity, ArtistVo>.NewConfig()
            .Map(dest => dest.CoverId, src => src.PictureId)
            .Map(dest => dest.Star, src => src.ArtistStarEntities != null && src.ArtistStarEntities.Count > 0)
            .Map(dest => dest.Count, src => src.TrackEntities != null ? src.TrackEntities.Count : 0)
            .Map(dest => dest.TotalTime, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Duration) : 0)
            .Map(dest => dest.TotalSize, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Size) : 0);

        TypeAdapterConfig<PlaylistEntity, PlaylistVo>.NewConfig()
            .Map(dest => dest.Count, src => src.TrackEntities != null ? src.TrackEntities.Count : 0)
            .Map(dest => dest.TotalTime, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Duration) : 0)
            .Map(dest => dest.TotalSize, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Size) : 0)
            .Map(dest => dest.Songs, src => src.TrackEntities);

        TypeAdapterConfig<Lyric, LrcVo>.NewConfig()
            .Map(dest => dest.Text, src => src.Text)
            .Map(dest => dest.TimestampMs, src => src.StartTime);
    }

    public static List<LrcVo>? LrcEntityToLrcVo(LrcEntity? lrcEntity)
    {
        if (lrcEntity == null)
        {
            return null;
        }

        var song = new LrcParser.Parser.Lrc.LrcParser().Decode(lrcEntity.Lrc!);
        return song.Lyrics.Adapt<List<LrcVo>>();
    }
    
}