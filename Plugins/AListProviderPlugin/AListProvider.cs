using AListSdkSharp.Api;
using AListSdkSharp.Vo;
using JxAudio.Plugin;
using Serilog;

namespace AListProviderPlugin;

public class AListProvider: IProviderPlugin
{
    public Guid Id { get; } = Guid.Parse("11C1B9B4-B826-4B9B-8418-31BF82E21F07");
    public string? Name => "AList提供器";

    public bool Ready
    {
        get
        {
            if (Constants.Token != null)
            {
                var isJwtExpired = Constants.IsJwtExpired(Constants.Token);
                if (!isJwtExpired)
                {
                    return true;
                }
            }

            try
            {
                Constants.Account ??= Constants.GetAccount().Result;

                if (Constants.Account.ServerUrl != null && Constants.Account.UserName != null && Constants.Account.Password != null)
                {
                    var auth = new Auth(Constants.Account.ServerUrl);
                    var login = auth.Login(Constants.Account.UserName, Constants.Account.Password).Result;
                    if (login is { Code: 200 })
                    {
                        Constants.Token = login.Data.Token;
                        return true;
                    }
                }
                
            }
            catch (Exception e)
            {
                Log.Error(e, "AList provider error");
            }

            Constants.Account = null;
            return false;
        }
    }

    public async Task<List<FsInfo>> ListFolderAsync(string path)
    {
        var fsInfoList = new List<FsInfo>();
        try
        {
            Fs fs = new Fs(Constants.Account!.ServerUrl);
            var list = await fs.List(Constants.Token, new ListIn()
            {
                Path = path,
                PrePage = int.MaxValue
            });
            if (list.Code != 200)
            {
                Log.Error(list.Message);
                return fsInfoList;
            }
        
            foreach (var item in list.Data.Content.Where(x => x.IsDir))
            {
                fsInfoList.Add(new FsInfo()
                {
                    Name = item.Name,
                    IsDir = item.IsDir,
                    Size = item.Size,
                    ModifyTime = item.Modified,
                    FullName = Path.Combine(path, item.Name).Replace("\\", "/")
                });
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "AList provider error");
        }
        
        return fsInfoList;
    }

    public async Task<List<FsInfo>> ListFilesAsync(string path)
    {
        var fsInfoList = new List<FsInfo>();
        try
        {
            Fs fs = new Fs(Constants.Account!.ServerUrl);
            var list = await fs.List(Constants.Token, new ListIn()
            {
                Path = path,
                PrePage = int.MaxValue
            });
            if (list.Code != 200)
            {
                Log.Error(list.Message);
                return fsInfoList;
            }
        
            foreach (var item in list.Data.Content)
            {
                fsInfoList.Add(new FsInfo()
                {
                    Name = item.Name,
                    IsDir = item.IsDir,
                    Size = item.Size,
                    ModifyTime = item.Modified,
                    FullName = Path.Combine(path, item.Name).Replace("\\", "/")
                });
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "AList provider error");
        }
        

        return fsInfoList;
    }

    public async Task<Stream?> GetThumbAsync(string name)
    {
        try
        {
            Fs fs = new Fs(Constants.Account!.ServerUrl);
            var info = await fs.Info(Constants.Token, new ListIn()
            {
                Path = Path.Combine($"{Path.GetFileNameWithoutExtension(name)}.jpg" )
            });
            if (info.Code != 200)
            {
                return null;
            }

            return await fs.Download(info);
        }
        catch (Exception e)
        {
            Log.Error(e, "AList provider error");
        }

        return null;
    }

    public async Task<FsInfo?> GetFileInfoAsync(string name)
    {
        try
        {
            Fs fs = new Fs(Constants.Account!.ServerUrl);
            var info = await fs.Info(Constants.Token, new ListIn()
            {
                Path = name
            });
            if (info.Code != 200)
            {
                return null;
            }

            return new FsInfo()
            {
                FullName = name,
                Name = info.Data.Name,
                IsDir = info.Data.IsDir,
                Size = info.Data.Size,
                ModifyTime = info.Data.Modified
            };
        }
        catch (Exception e)
        {
            Log.Error(e, "AList provider error");
        }

        return null;
    }

    public async Task UploadFiles(params UploadInfo[] uploadInfos)
    {
        Fs fs = new Fs(Constants.Account!.ServerUrl);
        foreach (var uploadInfo in uploadInfos)
        {
            var upload = await fs.Upload(Constants.Token, uploadInfo.FullPath, uploadInfo.FileStream);
            if (upload.Code != 200)
            {
                Log.Error(upload.Message);
            }
        }
    }

    public async Task DeleteFiles(params string[] fullpaths)
    {
        Fs fs = new Fs(Constants.Account!.ServerUrl);
        foreach (var fullpath in fullpaths)
        {
            var delete = await fs.Remove(Constants.Token, Path.GetDirectoryName(fullpath), [Path.GetFileName(fullpath)]);
            if (delete.Code != 200)
            {
                Log.Error(delete.Message);
            }
        }
    }

    public async Task<Stream?> GetFileAsync(string name)
    {
        try
        {
            Fs fs = new Fs(Constants.Account!.ServerUrl);
            var info = await fs.Info(Constants.Token, new ListIn()
            {
                Path = name
            });
            if (info.Code != 200)
            {
                return null;
            }
            return new PartialHttpStream(fs, info);
        }
        catch (Exception e)
        {
            Log.Error(e, "AList provider error");
        }

        return null;
    }

    public async Task<string?> GetLrcAsync(string name)
    {
        try
        {
            Fs fs = new Fs(Constants.Account!.ServerUrl);
            var info = await fs.Info(Constants.Token, new ListIn()
            {
                Path = Path.Combine($"{Path.GetFileNameWithoutExtension(name)}.lrc" )
            });
            if (info.Code != 200)
            {
                return null;
            }

            var stream = await fs.Download(info);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}