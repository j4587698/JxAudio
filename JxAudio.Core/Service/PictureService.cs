using System.Reflection;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace JxAudio.Core.Service;

[Transient]
public class PictureService
{
    public async Task<Stream?> GetPictureAsync(string id, int? size, CancellationToken cancellationToken)
    {
        PictureEntity? pictureEntity = default;
        if (id.TryParseTrackId(out var trackId))
        {
            var track = await TrackEntity
                .Where(x => x.Id == trackId)
                .Include(x => x.PictureEntity)
                .FirstAsync(cancellationToken);

            if (track?.PictureEntity == null)
            {
                return Assembly.GetExecutingAssembly().GetManifestResourceStream("JxAudio.Core.Assets.NoCover.png");
            }

            pictureEntity = track.PictureEntity;
        }
        else if (id.TryParseAlbumId(out var albumId))
        {
            var album = await AlbumEntity
                .Where(x => x.Id == albumId)
                .Include(x => x.PictureEntity)
                .FirstAsync(cancellationToken);

            if (album?.PictureEntity == null)
            {
                return Assembly.GetExecutingAssembly().GetManifestResourceStream("JxAudio.Core.Assets.NoCover.png");
            }

            pictureEntity = album.PictureEntity;
        }
        else if (id.TryParseArtistId(out var artistId))
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("JxAudio.Core.Assets.avatar.png");
        }
        else
        {
            throw RestApiErrorException.DataNotFoundError();
        }
        
        if (size == null || size > pictureEntity.Width)
        {
            return new MemoryStream(await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, Constants.CoverCachePath, pictureEntity!.Path!), cancellationToken));
        }
        
        using var image = Image.Load(await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, Constants.CoverCachePath, pictureEntity!.Path!), cancellationToken));
        image.Mutate(x => x.Resize(size.Value, size.Value));

        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream, cancellationToken: cancellationToken);
        return memoryStream;
    }
}