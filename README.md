# JxAudio

JxAudio是一个基于.net core的音频管理系统，支持音乐的播放、上传、下载、删除等功能。
兼容Subsonic协议，可以使用Subsonic客户端进行访问。
支持Windows、Linux、MacOS等操作系统。目前只提供Docker部署方式，其他方式须自行编译安装包，Windows和Linux提供ffmpeg二进制文件，MacOS需要自行安装ffmpeg。

## 使用的库和包

- asp.net core 9.0
- [BootstrapBlazor](https://github.com/dotnetcore/BootstrapBlazor)
- [FreeSql](https://github.com/dotnetcore/FreeSql) 支持Sqlite、MySql、SqlServer、PostgreSql
- [Serilog](https://github.com/serilog/serilog)
- [Mapster](https://github.com/MapsterMapper/Mapster)
- [AutoFac](https://autofac.org/)
- [FFmpegCore](https://github.com/rosenbjerg/FFMpegCore)
- FFmpeg 6.1
- [atldotnet](https://github.com/Zeugma440/atldotnet)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)
- [Longbow.Tasks](https://gitee.com/Longbow/Longbow.Tasks)

## 特点

- 支持插件功能，可以自定义插件进行扩展
- 支持直连网盘，可以直接播放网盘音乐
- 目前官方支持Alist网盘直接播放
- 可自行扩展后台页面，对音乐进行自定义管理

## 功能

- [x] 音乐播放
- [ ] 音乐上传
- [x] 音乐下载
- [x] 音乐删除
- [x] 歌单
- [x] 音乐搜索
- [x] 插件
- [x] Subsonic协议

## 插件

- AListProviderPlugin 支持AList网盘作为数据源
- LocalFileProviderPlugin 支持本地文件作为数据源

更多插件开发中...

## 使用

Docker部署

```shell
docker run -d -p 4587:4587 -v /path/to/config:/app/config -v /path/to/log:/app/log --name jxaudio j4587698/jxaudio
```
