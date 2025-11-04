[根目录](../../CLAUDE.md) > **JxAudio.Web**

# JxAudio.Web - Web API 主机模块

## 模块职责

JxAudio.Web 是系统的 Web API 主机层，负责：
- ASP.NET Core 应用程序启动与配置
- HTTP 请求处理与路由
- 身份认证与授权
- 中间件管道配置
- Blazor Server 渲染支持
- OpenAPI/Swagger 文档生成

## 入口与启动

### 主要入口文件
- `Program.cs` - 应用程序启动入口
- `appsettings.json` - 应用配置文件
- `appsettings.Development.json` - 开发环境配置

### 启动流程
1. 配置 Serilog 日志系统
2. 读取数据库配置并初始化
3. 注册服务到 DI 容器
4. 配置认证与授权
5. 设置中间件管道
6. 启动 Web 服务器

## 对外接口

### HTTP API 端点
- `/api/*` - 动态 API 路由（基于 Service 层自动生成）
- `/Admin/*` - 管理后台页面
- `/User/*` - 用户相关页面
- `/` - 主页面（Blazor 应用）

### 认证与授权
- Cookie 认证机制
- 登录路径：`/User/Login`
- 登出路径：`/api/User/Logout`
- 访问拒绝路径：`/User/NotAuth`
- Cookie 名称：`JxAudio`
- 过期时间：7 天

### 中间件管道
1. 异常处理中间件
2. 本地化中间件
3. CORS 中间件
4. 静态文件中间件
5. 安装检查中间件
6. 认证中间件
7. 授权中间件
8. 路由中间件

## 关键依赖与配置

### 项目依赖
- `JxAudio.Core` - 核心业务逻辑
- `JxAudio.Plugin` - 插件系统
- `Front/JxAudio.Front` - Blazor 前端
- `Front/JxAudio.TransVo` - 数据传输对象

### NuGet 包
- `Microsoft.AspNetCore.Components.WebAssembly.Server` - Blazor 混合渲染
- `Newtonsoft.Json` - JSON 序列化
- `System.Threading.Tasks.Dataflow` - 数据流处理
- `JxAudio.Native` - 原生库（FFmpeg）

### 配置参数
- 监听端口：4587
- 支持文化：zh-CN, en-US
- 默认文化：zh
- 允许的主机：*（所有）

## 数据模型

### 配置模型
```csharp
// Kestrel 配置
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:4587"
      }
    }
  }
}

// BootstrapBlazor 配置
{
  "BootstrapBlazorOptions": {
    "FallbackCultureName": "zh",
    "SupportedCultures": ["zh-CN", "en-US"]
  }
}
```

### 环境变量
- `ASPNETCORE_ENVIRONMENT` - 运行环境（Development/Production）
- `JxAudio:AesKey` - 加密密钥（从配置文件读取）

## 测试与质量

### 端点测试
- API 接口响应测试
- 认证流程测试
- 静态资源访问测试

### 中间件测试
- 异常处理测试
- CORS 策略测试
- 本地化功能测试

### 配置验证
- 数据库连接测试
- 插件加载测试
- 服务注册验证

## 常见问题 (FAQ)

### Q: 如何修改监听端口？
A: 在 `appsettings.json` 中修改 `Kestrel.Endpoints.Http.Url` 配置。

### Q: 如何添加新的 API 控制器？
A: 系统使用动态 API 生成，在 Core 层的 Service 类中添加方法即可自动生成 API。

### Q: 如何配置 HTTPS？
A: 取消注释 `Program.cs` 中的 `app.UseHttpsRedirection()` 并配置相关证书。

### Q: 如何自定义认证方式？
A: 在 `Program.cs` 的 `AddAuthentication()` 配置中添加其他认证方案。

## 相关文件清单

### 核心文件
- `Program.cs` - 应用程序启动逻辑（200+ 行）
- `appsettings.json` - 基础配置
- `appsettings.Development.json` - 开发配置
- `JxAudio.Web.csproj` - 项目配置文件

### 目录结构
- `Components/` - Blazor 组件（如果存在）
- `Controllers/` - API 控制器（如果存在）
- `Middlewares/` - 自定义中间件
- `Services/` - Web 层服务
- `Utils/` - 工具类
- `wwwroot/` - 静态资源目录

### 静态资源
- `wwwroot/Images/` - 图片资源目录
- `index.html` - Blazor 应用入口页面

## 变更记录 (Changelog)

### 2025-11-04 06:57:53 - 初始化模块文档
- 创建 JxAudio.Web 模块文档
- 分析启动配置和中间件管道
- 识别认证授权机制
- 建立 API 路由规则