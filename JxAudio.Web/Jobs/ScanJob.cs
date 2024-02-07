using ATL;
using Jx.Toolbox.Extensions;
using JxAudio.Core.Entity;
using JxAudio.Plugin;
using JxAudio.Web.Utils;
using Longbow.Tasks;
using SixLabors.ImageSharp;

namespace JxAudio.Web.Jobs;

public class ScanJob : ITask
{
    public async Task Execute(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var directoryEntities = await DirectoryEntity.Select.ToListAsync(cancellationToken);
        foreach (var directoryEntity in directoryEntities)
        {
            var providerPlugin = Constant.ProviderPlugins.FirstOrDefault(x => x.Id == directoryEntity.Provider);
            if (providerPlugin == null)
            {
                continue;
            }

            await ScanFiles(providerPlugin, directoryEntity.Path);
        }
    }
    
    private async Task ScanFiles(IProviderPlugin providerPlugin, string path)
    {
        var files = await providerPlugin.ListFilesAsync(path);
        foreach (var fsInfo in files)
        {
            if (fsInfo.IsDir)
            {
                await ScanFiles(providerPlugin, fsInfo.FullName);
            }
            else
            {
                if (Constant.AudioExtensions.Contains(Path.GetExtension(fsInfo.Name)))
                {
                    var stream = await providerPlugin.GetFileAsync(fsInfo.FullName);
                    if (stream == null)
                    {
                        continue;
                    }

                    var track = new Track(stream);
                    var artists = track.Artist.Split(';');
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
                            }
                            artistEntities.Add(artistEntity);
                        }
                    }

                    AlbumEntity? albumEntity = null;
                    if (!track.Album.IsNullOrEmpty() && artistEntities.Count > 0)
                    {
                        albumEntity = await AlbumEntity.Where(x => x.Title == track.Album && x.ArtistId == artistEntities[0].Id).FirstAsync();
                        if (albumEntity == null)
                        {
                            albumEntity = new AlbumEntity
                            {
                                Title = track.Album,
                                ArtistId = artistEntities[0].Id,
                                Year = track.Year,
                                PictureId = (await GetPicture(providerPlugin, fsInfo, track))?.Id
                            };

                            await albumEntity.SaveAsync();
                        }
                        else if(albumEntity.PictureId is null or 0)
                        {
                            var picture = await GetPicture(providerPlugin, fsInfo, track);
                            if (picture != null)
                            {
                                albumEntity.PictureId = picture.Id;
                                await albumEntity.SaveAsync();
                            }
                        }
                    }
                    
                    var trackEntity = new TrackEntity()
                    {
                        Name = fsInfo.Name,
                        FullName = fsInfo.FullName,
                        Size = fsInfo.Size,
                        ProviderId = providerPlugin.Id,
                        TrackNumber = track.TrackNumber ?? 999,
                        Duration = track.Duration,
                        BitRate = track.Bitrate,
                        Title = track.Title,
                        SortTitle = track.SortTitle,
                        AlbumId = albumEntity?.Id,
                        PictureId = albumEntity?.PictureId
                    };
                    await trackEntity.SaveAsync();
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
            var extension = image.Metadata.DecodedImageFormat?.FileExtensions.FirstOrDefault() ?? ".jpg";
            var mimeType = image.Metadata.DecodedImageFormat?.DefaultMimeType ?? "image/jpeg";
            var picName = Path.Combine("config", "cache", $"{Guid.NewGuid()}{extension}");
            var fullPath = Path.Combine(AppContext.BaseDirectory, picName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            await image.SaveAsync(fullPath);
            var pictureEntity = new PictureEntity
            {
                MimeType = mimeType,
                Path = picName
            };
            await pictureEntity.SaveAsync();
            return pictureEntity;
        }

        return null;
    }
}