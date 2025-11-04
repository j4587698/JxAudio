[根目录](../../CLAUDE.md) > **LocalFileProviderPlugin**

# LocalFileProviderPlugin - 本地文件数据源插件

## 模块职责

LocalFileProviderPlugin 是 JxAudio 系统的官方插件，提供：
- 本地文件系统访问能力
- 音频文件扫描与索引
- 文件流读取支持
- 跨平台路径处理
- 目录遍历功能

## 入口与启动

### 主要入口文件
- `LocalFileProvider.cs` - 插件主类
- `SystemInstance.cs` - 系统实例提供器

### 启动流程
1. 插件被 Core 模块动态加载
2. 实现 `IProviderPlugin` 接口
3. 注册为数据源提供器
4. 等待系统调用进行文件扫描

## 对外接口

### IProviderPlugin 接口实现

#### 基本属性
- `Id: Guid` - C6E80410-1A50-4B9D-BF73-EFAAF3E5469C
- `Name: string` - "本地文件提供器"
- `Ready: bool` - 始终返回 true（本地文件系统始终可用）

#### 核心方法
- `ListFolderAsync(string path)` - 列出指定路径下的文件夹
- `ListFilesAsync(string path)` - 列出指定路径下的文件和文件夹
- `GetStreamAsync(string path)` - 获取文件流（继承自基类或接口）

### 功能特性

#### 跨平台支持
- Windows 环境下显示逻辑驱动器列表
- Linux/MacOS 环境下显示根目录结构
- 统一的路径处理机制

#### 文件系统操作
- 目录存在性检查
- 文件/文件夹枚举
- 文件信息获取（大小、修改时间等）
- 文件流访问

## 关键依赖与配置

### 项目依赖
- `JxAudio.Plugin` - 插件基础框架
- `Jx.Toolbox` - 工具库（用于操作系统检测）

### 系统要求
- 对目标目录的读取权限
- 足够的磁盘空间用于音频文件
- 支持的音频格式（通过 FFmpeg 识别）

### 配置参数
- 无需额外配置文件
- 通过系统配置指定扫描路径
- 支持多个扫描目录

## 数据模型

### FsInfo 模型使用
```csharp
public class FsInfo
{
    public string Name { get; set; }        // 文件/文件夹名称
    public string FullName { get; set; }    // 完整路径
    public bool IsDir { get; set; }         // 是否为目录
    public long Size { get; set; }          // 文件大小（字节）
    public DateTime ModifyTime { get; set; } // 修改时间
}
```

### 路径处理规则
- Windows：使用驱动器根目录（如 "C:\"）
- Unix：使用绝对路径（如 "/home/music"）
- 统一使用正斜杠作为路径分隔符

## 测试与质量

### 功能测试
- 目录列表准确性测试
- 文件信息获取测试
- 跨平台兼容性测试
- 权限处理测试

### 性能测试
- 大目录扫描性能
- 文件流读取速度
- 内存使用情况
- 并发访问处理

### 边界条件测试
- 不存在路径处理
- 权限不足处理
- 网络驱动器访问
- 特殊字符路径处理

## 常见问题 (FAQ)

### Q: 如何添加新的扫描目录？
A: 在系统配置的目录管理中添加新的本地路径。

### Q: 插件支持网络驱动器吗？
A: 理论支持，但性能取决于网络连接质量和权限配置。

### Q: 如何处理中文路径？
A: 插件完全支持 Unicode 路径，包括中文文件名。

### Q: 文件访问权限如何处理？
A: 插件会遵循系统权限设置，无权限时会跳过相应文件。

## 相关文件清单

### 核心文件
- `LocalFileProvider.cs` - 插件主实现类
- `SystemInstance.cs` - 系统实例提供器
- `LocalFileProviderPlugin.csproj` - 项目配置文件

### 编译输出
- `bin/` - 编译输出目录
- `obj/` - 编译临时文件
- 最终程序集：`LocalFileProviderPlugin.dll`

## 变更记录 (Changelog)

### 2025-11-04 06:57:53 - 初始化插件文档
- 创建 LocalFileProviderPlugin 插件文档
- 分析本地文件访问实现
- 识别跨平台兼容性特性
- 建立文件扫描工作流程