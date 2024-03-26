using Jx.Toolbox.Utils;
using JxAudio.Plugin;

namespace LocalFileProviderPlugin;

public class LocalFileProvider : IProviderPlugin
{
    public Guid Id { get; } = new Guid("C6E80410-1A50-4B9D-BF73-EFAAF3E5469C");
    
    public string? Name { get; } = "本地文件提供器";
    
    public Task<List<FsInfo>> ListFolderAsync(string path)
    {
        var fsInfos = new List<FsInfo>();
        if (path == "/" && Os.IsWindows())
        {
            fsInfos.AddRange(Environment.GetLogicalDrives().Select(x => new FsInfo(){IsDir = true, FullName = x, Name = x}));
            return Task.FromResult(fsInfos);
        }
        if (Directory.Exists(path))
        {
            fsInfos.AddRange(Directory.GetDirectories(path).Select(x => new FsInfo(){IsDir = true, FullName = x, Name = Path.GetFileName(x)}));
        }

        return Task.FromResult(fsInfos);
    }

    public Task<List<FsInfo>> ListFilesAsync(string path)
    {
        var fsInfos = new List<FsInfo>();
        if (Directory.Exists(path))
        {
            fsInfos.AddRange(Directory.GetDirectories(path).Select(x => new FsInfo(){IsDir = true, FullName = x, Name = Path.GetFileName(x)}));
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var fsInfo = new FsInfo
                {
                    Name = fileInfo.Name,
                    FullName = fileInfo.FullName,
                    IsDir = false,
                    Size = fileInfo.Length,
                    ModifyTime = fileInfo.LastWriteTime
                };
                fsInfos.Add(fsInfo);
            }
        }

        return Task.FromResult(fsInfos);
    }

    public async Task<Stream?> GetThumbAsync(string name)
    {
        var thumbPath = Path.Combine($"{Path.GetFileNameWithoutExtension(name)}.jpg" );
        if (File.Exists(thumbPath))
        {
            var memoryStream = new MemoryStream();
            await File.OpenRead(thumbPath).CopyToAsync(memoryStream);
            return memoryStream;
        }

        return null;
    }

    public Task<FsInfo?> GetFileInfoAsync(string name)
    {
        if (!File.Exists(name))
        {
            return Task.FromResult<FsInfo?>(null);
        }
        
        var fileInfo = new FileInfo(name);
        return Task.FromResult<FsInfo?>(new FsInfo
        {
            Name = fileInfo.Name,
            FullName = fileInfo.FullName,
            IsDir = false,
            Size = fileInfo.Length,
            ModifyTime = fileInfo.LastWriteTime
        });
    }

    public Task UploadFiles(params UploadInfo[] uploadInfos)
    {
        foreach (var uploadInfo in uploadInfos)
        {
            using FileStream fileStream = new FileStream(uploadInfo.FullPath, FileMode.Create, FileAccess.Write);
            // 将sourceStream的内容复制到fileStream
            uploadInfo.FileStream.CopyTo(fileStream);
            uploadInfo.FileStream.Dispose();
        }

        return Task.CompletedTask;
    }

    public Task DeleteFiles(params string[] fullpaths)
    {
        foreach (var fullpath in fullpaths)
        {
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
        }
        return Task.CompletedTask;
    }

    public Task<Stream?> GetFileAsync(string name)
    {
        if (!File.Exists(name))
        {
            return Task.FromResult<Stream?>(null);
        }
        return Task.FromResult<Stream?>(File.OpenRead(name));
    }

    public async Task<string?> GetLrcAsync(string name)
    {
        var lrcPath = Path.Combine($"{Path.GetFileNameWithoutExtension(name)}.lrc" );
        if (File.Exists(lrcPath))
        {
            return await File.ReadAllTextAsync(lrcPath);
        }

        return null;
    }
    
}