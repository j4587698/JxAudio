using ATL;
using JxAudio.Core.Entity;
using JxAudio.Plugin;
using JxAudio.Web.Utils;
using Longbow.Tasks;

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
                    
                    var trackEntity = new TrackEntity()
                    {
                        Name = fsInfo.Name,
                        FullName = fsInfo.FullName,
                        Size = fsInfo.Size,
                        ProviderId = providerPlugin.Id,
                        TrackNumber = track.TrackNumber ?? 0,
                        Duration = track.Duration
                        
                    };
                    await trackEntity.SaveAsync();
                }
            }
        }
    }
}