using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;

namespace JxAudio.Core.Service;

[Transient]
public class DirectoryService
{
    public async Task<MusicFolders> GetMusicFoldersAsync(Guid userId, CancellationToken cancellationToken)
    {
        var folder = await DirectoryEntity.Where(x => !x.IsAccessControlled || x.UserEntities.Any(y => y.Id == userId))
            .OrderBy(x => x.Name)
            .ToListAsync<MusicFolder>(x => new MusicFolder()
            {
                id = (int)x.Id,
                name = x.Name
            }, cancellationToken);
        return new MusicFolders()
        {
            musicFolder = folder.ToArray()
        };
    }
}