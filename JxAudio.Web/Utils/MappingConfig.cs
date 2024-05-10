using JxAudio.Core.Entity;
using JxAudio.TransVo;
using Mapster;

namespace JxAudio.Web.Utils;

public class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<AlbumEntity, AlbumVo>.NewConfig()
            .Map(dest => dest.ArtistName, src => src.ArtistEntity.Name ?? "[未知歌手]")
            .Map(dest => dest.Count, src => src.TrackEntities.Count)
            .Map(dest => dest.TotalSize, src => src.TrackEntities.Sum(x => x.Size));
    }
    
}