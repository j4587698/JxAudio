using Jx.Toolbox.Extensions;
using JxAudio.Core.Entity;
using JxAudio.TransVo;
using Mapster;

namespace JxAudio.Web.Utils;

public class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<AlbumEntity, AlbumVo>.NewConfig()
            .Map(dest => dest.Artist, src => src.ArtistEntity)
            .Map(dest => dest.Count, src => src.TrackEntities != null ? src.TrackEntities.Count : 0)
            .Map(dest => dest.CoverId, src => src.PictureId)
            .Map(dest => dest.TotalSize, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Size) : 0);

        TypeAdapterConfig<TrackEntity, TrackVo>.NewConfig()
            .Map(dest => dest.Artists, src =>src.ArtistEntities);
        
        TypeAdapterConfig<ArtistEntity, ArtistVo>.NewConfig()
            .Map(dest => dest.Count, src => src.TrackEntities != null ? src.TrackEntities.Count : 0)
            .Map(dest => dest.TotalSize, src => src.TrackEntities != null ? src.TrackEntities.Sum(x => x.Size) : 0);
    }
    
}