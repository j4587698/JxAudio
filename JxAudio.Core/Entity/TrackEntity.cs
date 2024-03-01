using System.ComponentModel;
using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

[Description("歌曲表")]
public class TrackEntity : BaseEntity<TrackEntity, int>
{
    [Description("提供器Id")]
    public Guid ProviderId { get; set; }

    [Description("音轨路径")]
    public string? FullName { get; set; }

    [Description("音轨大小")]
    public long Size { get; set; }

    [Description("音轨名")]
    public string? Name { get; set; }
    
    [Description("音轨序号")]
    public int TrackNumber { get; set; }

    [Description("音频格式")]
    public string? CodecName { get; set; }
    
    [Description("比特率")]
    public int? BitRate { get; set; }
    
    [Description("音频时长")]
    public float Duration { get; set; }
    
    [Description("音轨标题")]
    public string? Title { get; set; }

    [Description("排序名称")]
    public string? SortTitle { get; set; }

    [Description("专辑Id")]
    public int? AlbumId { get; set; }

    [Navigate(nameof(AlbumId))]
    public AlbumEntity? AlbumEntity { get; set; }

    [Description("封面Id")]
    public int? PictureId { get; set; }

    [Navigate(nameof(PictureId))]
    public PictureEntity? PictureEntity { get; set; }

    [Navigate(ManyToMany = typeof(TrackArtistEntity))]
    public ICollection<ArtistEntity>? ArtistEntities { get; set; }
    
    [Description("目录Id")]
    public long DirectoryId { get; set; }

    [Navigate(nameof(DirectoryId))]
    public DirectoryEntity? DirectoryEntity { get; set; }
}