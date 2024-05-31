using ATL;
using Jx.Toolbox.Extensions;
using JxAudio.Core.Entity;
using JxAudio.TransVo;
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
            .Map(dest => dest.Count, src => src.TrackEntities != null ? src.TrackEntities.Count : 0)
            .Map(dest => dest.TotalTime, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Duration) : 0)
            .Map(dest => dest.TotalSize, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Size) : 0);

    }

    private static List<LrcVo>? LrcEntityToLrcVo(LrcEntity? lrcEntity)
    {
        if (lrcEntity == null)
        {
            return null;
        }
        var info = new LyricsInfo();
        info.ParseLRC(lrcEntity.Lrc);
        return info.SynchronizedLyrics.Adapt<List<LrcVo>>();
    }
    
}