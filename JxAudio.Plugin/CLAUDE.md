[根目录](../../CLAUDE.md) > **JxAudio.Plugin**

# JxAudio.Plugin - 插件系统模块

## 模块职责

JxAudio.Plugin 是系统的插件基础设施层，提供：
- 插件接口定义与规范
- 插件加载与生命周期管理
- 插件配置与元数据管理
- 插件间通信机制
- 动态 API 注册支持

## 入口与启动

### 核心接口
- `IProviderPlugin` - 数据源提供器插件接口
- `ISystemPlugin` - 系统扩展插件接口

### 启动流程
1. Core 模块启动时扫描插件目录
2. 通过 `McMaster.NETCore.Plugins` 加载插件程序集
3. 验证插件接口实现
4. 注册插件服务到 DI 容器
5. 激活插件功能

## 对外接口

### 插件接口定义

#### IProviderPlugin - 数据源提供器
```csharp
public interface IProviderPlugin
{
    Guid Id { get; }          // 插件唯一标识
    string? Name { get; }     // 插件显示名称
    bool Ready { get; }       // 插件就绪状态

    Task<List<FsInfo>> ListFolderAsync(string path);    // 列出文件夹
    Task<List<FsInfo>> ListFilesAsync(string path);     // 列出文件
    Task<Stream?> GetStreamAsync(string path);          // 获取文件流
}
```

#### ISystemPlugin - 系统扩展
```csharp
public interface ISystemPlugin
{
    Guid Id { get; }          // 插件唯一标识
    string? Name { get; }     // 插件显示名称
    bool Ready { get; }       // 插件就绪状态

    Task OnLoadAsync();       // 插件加载时调用
    Task OnUnloadAsync();     // 插件卸载时调用
}
```

### 插件元数据
- `PluginInfoAttribute` - 插件信息标注特性
- `PluginConfig` - 插件配置模型
- `PluginLoaderInfo` - 插件加载信息
- `PluginMenuModel` - 插件菜单模型

### 数据模型
- `FsInfo` - 文件系统信息模型
- `UploadInfo` - 上传信息模型
- `PluginUtil` - 插件工具类

## 关键依赖与配置

### 项目依赖
- 依赖 `JxAudio.Core` 获取基础服务
- 使用 `McMaster.NETCore.Plugins` 进行动态加载

### 配置要求
- 插件必须实现相应接口
- 插件程序集需要标注 `PluginInfoAttribute`
- 插件配置文件遵循指定格式

## 插件生态

### 官方插件
1. **LocalFileProviderPlugin** - 本地文件数据源
   - 支持本地文件系统访问
   - 跨平台驱动器列表
   - 文件/文件夹枚举

2. **AListProviderPlugin** - AList 网盘数据源
   - 支持 AList 协议网盘
   - 在线音频流播放
   - 云存储集成

### 插件分类
- **数据源插件** - 提供音频文件访问能力
- **功能插件** - 扩展系统功能特性
- **UI 插件** - 增强用户界面体验

## 测试与质量

### 插件验证
- 接口实现验证
- 依赖关系检查
- 配置文件格式验证
- 运行时安全检查

### 兼容性测试
- 不同插件版本兼容性
- 插件间冲突检测
- 性能影响评估

### 安全机制
- 插件沙箱隔离
- 权限控制机制
- 恶意插件检测

## 常见问题 (FAQ)

### Q: 如何开发新的数据源插件？
A: 实现 `IProviderPlugin` 接口，标注 `PluginInfoAttribute`，编译为独立程序集放入 Plugins 目录。

### Q: 插件如何访问系统服务？
A: 通过依赖注入获取服务，或在插件加载时传入 `IServiceProvider`。

### Q: 插件配置如何管理？
A: 使用 `PluginConfig` 类定义配置结构，系统会自动加载和保存配置。

### Q: 如何处理插件版本升级？
A: 通过 `PluginInfoAttribute` 指定版本信息，系统支持插件热更新。

## 相关文件清单

### 核心接口文件
- `IProviderPlugin.cs` - 数据源插件接口
- `ISystemPlugin.cs` - 系统插件接口

### 插件管理文件
- `PluginInfoAttribute.cs` - 插件信息特性
- `PluginConfig.cs` - 插件配置模型
- `PluginLoaderInfo.cs` - 加载信息模型
- `PluginUtil.cs` - 插件工具类

### 运行时支持
- `MyActionDescriptorChangeProvider.cs` - 动态 API 支持
- `FsInfo.cs` - 文件系统信息模型
- `UploadInfo.cs` - 上传信息模型

### UI 集成
- `PluginMenuModel.cs` - 插件菜单模型

## 变更记录 (Changelog)

### 2025-11-04 06:57:53 - 初始化模块文档
- 创建 JxAudio.Plugin 模块文档
- 定义插件接口规范
- 分析官方插件实现
- 建立插件生态系统架构