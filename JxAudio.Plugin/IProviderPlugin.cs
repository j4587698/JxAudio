﻿using System.Net;

namespace JxAudio.Plugin;

/// <summary>
/// Provider插件接口
/// </summary>
public interface IProviderPlugin
{
    /// <summary>
    /// 插件ID，唯一值
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// 插件名称
    /// </summary>
    string? Name { get; }
    
    /// <summary>
    /// 是否已经准备就绪，可以使用
    /// </summary>
    bool Ready { get; }

    /// <summary>
    /// 获取所有文件夹信息
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<List<FsInfo>> ListFolderAsync(string path);

    /// <summary>
    /// 获取所有文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<List<FsInfo>> ListFilesAsync(string path);

    /// <summary>
    /// 获取缩略图(专辑封面)，如果不支持返回null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<Stream?> GetThumbAsync(string name);
    
    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<FsInfo?> GetFileInfoAsync(string name);

    /// <summary>
    /// 上传文件集
    /// </summary>
    /// <param name="uploadInfo"></param>
    /// <returns></returns>
    Task UploadFiles(params UploadInfo[] uploadInfos);

    /// <summary>
    /// 删除文件集
    /// </summary>
    /// <param name="fullpaths"></param>
    /// <returns></returns>
    Task DeleteFiles(params string[] fullpaths);

    /// <summary>
    /// 获取文件
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<Stream?> GetFileAsync(string name);

    /// <summary>
    /// 获得外置歌词，如果不支持或无外置歌词返回null
    /// </summary>
    public Task<string?> GetLrcAsync(string name);
}