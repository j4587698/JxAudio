using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;

namespace JxAudio.Core.Service;

[Transient]
public class StarService
{
    public void GetStarred2Async(Guid userId, int? musicFolderId)
    {
        AlbumStarEntity.Where(x => x.AlbumEntity!.TrackEntities!.Any(y =>
                y.DirectoryEntity!.IsAccessControlled == false ||
                y.DirectoryEntity.UserEntities!.Any(z => z.Id == userId)))
            .WhereIf(musicFolderId != null, x => x.AlbumEntity!.TrackEntities!.Any(y => y.DirectoryId == musicFolderId))
            .Include(x => x.AlbumEntity);

    }
}