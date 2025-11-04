[根目录](../../CLAUDE.md) > [Front](../) > **JxAudio.TransVo**

# JxAudio.TransVo - 数据传输对象模块

## 模块职责

JxAudio.TransVo 是前端数据传输层，专门负责：
- 前后端数据传输对象定义
- API 响应数据模型
- 前端数据绑定模型
- 查询参数与选项模型
- 数据验证与转换规则

## 入口与启动

### 模块特点
- 纯数据模型定义，无业务逻辑
- 被 JxAudio.Front 引用
- 与 Core 层实体对应但独立
- 支持前端特定需求优化

## 对外接口

### 核心数据传输对象 (VO)

#### 音频相关 VO
- `TrackVo` - 音轨信息传输对象
- `AlbumVo` - 专辑信息传输对象
- `ArtistVo` - 艺术家信息传输对象
- `LrcVo` - 歌词信息传输对象

#### 用户相关 VO
- `User.cs` - 用户信息传输对象
- `RePassVo.cs` - 密码重置传输对象

#### 播放相关 VO
- `PlaylistVo` - 播放列表传输对象
- `SearchResultVo.cs` - 搜索结果传输对象

#### 查询与过滤 VO
- `QueryOptionsVo.cs` - 查询选项传输对象
- `DynamicFilterInfo.cs` - 动态过滤信息
- `DynamicFilterLogic.cs` - 动态过滤逻辑
- `DynamicFilterOperator.cs` - 动态过滤操作符

#### 通用 VO
- `ResultVo.cs` - 通用结果传输对象
- `Search.cs` - 搜索模型

## 关键依赖与配置

### 项目依赖
- 无外部 NuGet 包依赖
- 纯 .NET 9.0 基础类库
- 仅包含数据模型定义

### 设计原则
- 保持轻量级，仅包含数据定义
- 支持序列化/反序列化
- 前端友好的属性命名
- 数据验证特性支持

## 数据模型映射

### 与 Core 实体对应关系
| TransVo 类 | Core 实体 | 主要差异 |
|------------|-----------|----------|
| `TrackVo` | `TrackEntity` | 简化导航属性，优化前端绑定 |
| `AlbumVo` | `AlbumEntity` | 包含前端展示需要的额外属性 |
| `ArtistVo` | `ArtistEntity` | 简化关联关系 |
| `User.cs` | `UserEntity` | 去除敏感信息，仅保留前端需要的字段 |
| `PlaylistVo` | `PlaylistEntity` | 包含音轨统计信息 |

### 数据转换规则
- 使用 Mapster 进行对象映射
- 自动处理空值和默认值
- 支持自定义转换逻辑
- 保持数据一致性

## 测试与质量

### 数据验证测试
- VO 属性验证规则测试
- 序列化/反序列化测试
- 数据类型转换测试

### 映射测试
- VO 与 Entity 之间的映射测试
- 复杂对象嵌套映射测试
- 空值处理测试

### 性能测试
- 大量数据传输性能测试
- 内存占用优化验证
- 序列化效率测试

## 常见问题 (FAQ)

### Q: 为什么需要独立的 VO 层？
A: 为了前后端数据解耦，提供前端友好的数据结构，避免直接暴露实体模型。

### Q: VO 与 Entity 如何同步？
A: 通过 Mapster 配置映射规则，在 Service 层进行数据转换。

### Q: 如何添加新的 VO？
A: 创建对应的类文件，定义属性，然后在 Service 层添加映射逻辑。

### Q: VO 支持数据验证吗？
A: 支持，可以使用 .NET 的数据验证特性（如 [Required], [Range] 等）。

## 相关文件清单

### 音频相关 VO
- `TrackVo.cs` - 音轨传输对象
- `AlbumVo.cs` - 专辑传输对象
- `ArtistVo.cs` - 艺术家传输对象
- `LrcVo.cs` - 歌词传输对象

### 用户相关 VO
- `User.cs` - 用户传输对象
- `RePassVo.cs` - 密码重置传输对象

### 播放相关 VO
- `PlaylistVo.cs` - 播放列表传输对象

### 搜索相关 VO
- `SearchResultVo.cs` - 搜索结果传输对象
- `Search.cs` - 搜索模型

### 查询相关 VO
- `QueryOptionsVo.cs` - 查询选项
- `DynamicFilterInfo.cs` - 动态过滤信息
- `DynamicFilterLogic.cs` - 动态过滤逻辑
- `DynamicFilterOperator.cs` - 动态过滤操作符

### 通用 VO
- `ResultVo.cs` - 通用结果对象

### 项目文件
- `JxAudio.TransVo.csproj` - 项目配置文件

## 变更记录 (Changelog)

### 2025-11-04 06:57:53 - 初始化模块文档
- 创建 JxAudio.TransVo 模块文档
- 分析 14 个数据传输对象
- 建立与 Core 实体的映射关系
- 定义数据转换和验证规则