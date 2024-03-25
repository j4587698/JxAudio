using ATL;
using FreeSql;
using Jx.Toolbox.Extensions;
using JxAudio.Core;
using JxAudio.Core.Entity;
using JxAudio.Plugin;
using JxAudio.Web.Utils;
using Longbow.Tasks;
using Serilog;
using SixLabors.ImageSharp;

namespace JxAudio.Web.Jobs;

public class ScanJob : ITask
{

    private List<TrackEntity>? _trackEntities;
    
    public async Task Execute(IServiceProvider provider, CancellationToken cancellationToken)
    {
        _trackEntities = await TrackEntity.Select.ToListAsync(x => new TrackEntity()
            { Id = x.Id, ProviderId = x.ProviderId, FullName = x.FullName }, cancellationToken);
        var directoryEntities = await DirectoryEntity.Select.ToListAsync(cancellationToken);
        foreach (var directoryEntity in directoryEntities)
        {
            var providerPlugin = Constant.ProviderPlugins.FirstOrDefault(x => x.Id == directoryEntity.Provider);
            if (providerPlugin == null)
            {
                Log.Warning("提供器{pluginId}不存在，无法继续", directoryEntity.Provider);
                continue;
            }
            
            await ScanFiles(providerPlugin, directoryEntity.Path, directoryEntity);
        }
    }
    
    private async Task ScanFiles(IProviderPlugin providerPlugin, string path, DirectoryEntity directoryEntity)
    {
        var files = await providerPlugin.ListFilesAsync(path);
        foreach (var fsInfo in files)
        {
            if (fsInfo.IsDir)
            {
                await ScanFiles(providerPlugin, fsInfo.FullName, directoryEntity);
            }
            else
            {
                if (_trackEntities?.Any(x => x.ProviderId == providerPlugin.Id && x.FullName == fsInfo.FullName) == true)
                {
                    Log.Information("文件{filename}已存在，跳过执行", fsInfo.FullName);
                    continue;
                }
                if (Constants.AudioExtensions.Contains(Path.GetExtension(fsInfo.Name)))
                {
                    var stream = await providerPlugin.GetFileAsync(fsInfo.FullName);
                    if (stream == null)
                    {
                        continue;
                    }

                    var track = new Track(stream);
                    var artists = track.Artist.Split(new []{';', '&', '、', '|'}).Select(x => x.Trim()).ToArray();
                    var artistEntities = new List<ArtistEntity>();
                    if (artists.Length > 0)
                    {
                        foreach (var artist in artists)
                        {
                            var artistEntity = await ArtistEntity.Where(x => x.Name == artist).FirstAsync();
                            if (artistEntity == null)
                            {
                                artistEntity = new ArtistEntity
                                {
                                    Name = artist
                                };
                                await artistEntity.SaveAsync();
                                Log.Information("查找到新歌手{artist}", artistEntity.Name);
                            }
                            artistEntities.Add(artistEntity);
                        }
                    }

                    GenreEntity? genre = null;
                    if (!track.Genre.IsNullOrEmpty())
                    {
                        genre = await GenreEntity.Where(x => x.Name == track.Genre).FirstAsync();
                        if (genre == null)
                        {
                            genre = new GenreEntity()
                            {
                                Name = track.Genre
                            };
                            await genre.SaveAsync();
                        }
                    }
                    

                    AlbumEntity? albumEntity = null;
                    if (!track.Album.IsNullOrEmpty())
                    {
                        albumEntity = await AlbumEntity.Where(x => x.Title == track.Album && x.ArtistId == artistEntities[0].Id).FirstAsync();
                        if (albumEntity == null)
                        {
                            albumEntity = new AlbumEntity
                            {
                                Title = track.Album,
                                ArtistId = artistEntities is { Count: > 0 } ? artistEntities[0].Id : 0,
                                Year = track.Year,
                                GenreId = genre?.Id,
                                PictureId = (await GetPicture(providerPlugin, fsInfo, track))?.Id
                            };

                            await albumEntity.SaveAsync();
                            Log.Information("查找到新专辑{album}", albumEntity.Title);
                        }
                        else if (albumEntity.PictureId is null or 0)
                        {
                            var picture = await GetPicture(providerPlugin, fsInfo, track);
                            if (picture != null)
                            {
                                albumEntity.PictureId = picture.Id;
                                await albumEntity.SaveAsync();

                                var tracks = await TrackEntity
                                    .Where(x => x.AlbumId == albumEntity.Id)
                                    .ToListAsync();
                                foreach (var entity in tracks)
                                {
                                    entity.PictureId = picture.Id;
                                }

                                BaseEntity.Orm.Update<TrackEntity>(tracks);
                            }
                        }
                    }

                    LrcEntity? lrcEntity = null;

                    var lrc = await providerPlugin.GetLrcAsync(path);

                    if (!lrc.IsNullOrEmpty())
                    {
                        lrcEntity = new LrcEntity
                        {
                            Artist = artistEntities is { Count: > 0 } ? string.Join(",", artistEntities.Select(y => y.Name)) : "",
                            Title = track.Title,
                            Lrc = lrc
                        };
                        await lrcEntity.SaveAsync();
                    }
                    else if (track.Lyrics != null && track.Lyrics.SynchronizedLyrics.Count > 0)
                    {
                        lrcEntity = new LrcEntity
                        {
                            Artist = artistEntities is { Count: > 0 } ? string.Join(",", artistEntities.Select(y => y.Name)) : "",
                            Title = track.Title,
                            Lrc = track.Lyrics.FormatSynchToLRC()
                        };
                        await lrcEntity.SaveAsync();
                    }
                    
                    var trackEntity = new TrackEntity()
                    {
                        Name = fsInfo.Name,
                        FullName = fsInfo.FullName,
                        Size = fsInfo.Size,
                        ProviderId = providerPlugin.Id,
                        TrackNumber = track.TrackNumber,
                        DiscNumber = track.DiscNumber,
                        Duration = track.Duration,
                        BitRate = track.Bitrate,
                        Title = track.Title,
                        SortTitle = track.SortTitle,
                        AlbumId = albumEntity?.Id,
                        PictureId = albumEntity?.PictureId,
                        CodecName = track.AudioFormat.ShortName,
                        MimeType = track.AudioFormat.MimeList.FirstOrDefault(),
                        ArtistEntities = artistEntities,
                        DirectoryId = directoryEntity.Id,
                        GenreId = genre?.Id,
                        LrcId = lrcEntity?.Id ?? 0
                    };
                    await trackEntity.SaveAsync();
                    await trackEntity.SaveManyAsync(nameof(TrackEntity.ArtistEntities));
                    Log.Information("加入歌曲{track}", trackEntity.Title);
                }
            }
        }
    }

    private async Task<PictureEntity?> GetPicture(IProviderPlugin providerPlugin, FsInfo fsInfo, Track track)
    {
        var picStream = await providerPlugin.GetThumbAsync(fsInfo.FullName);
        if (picStream == null && track.EmbeddedPictures.Count > 0)
        {
            var picture =
                track.EmbeddedPictures.FirstOrDefault(x => x.PicType == PictureInfo.PIC_TYPE.Front) ??
                track.EmbeddedPictures[0];

            picStream = new MemoryStream(picture.PictureData);
        }

        if (picStream != null)
        {
            var image = await Image.LoadAsync(picStream);
            var extension = image.Metadata.DecodedImageFormat?.FileExtensions.FirstOrDefault() ?? "jpg";
            var mimeType = image.Metadata.DecodedImageFormat?.DefaultMimeType ?? "image/jpeg";
            var picName = Path.Combine($"{Guid.NewGuid()}.{extension}");
            var fullPath = Path.Combine(AppContext.BaseDirectory, Constants.CoverCachePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            await image.SaveAsync(Path.Combine(fullPath, picName));
            var pictureEntity = new PictureEntity
            {
                MimeType = mimeType,
                Path = picName,
                Height = image.Height,
                Width = image.Width
            };
            await pictureEntity.SaveAsync();
            return pictureEntity;
        }

        return null;
    }
}