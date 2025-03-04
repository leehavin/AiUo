# AiUo.AspNet.Hosting 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.AspNet.Hosting.svg)](https://www.nuget.org/packages/AiUo.AspNet.Hosting)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.AspNet.Hosting.svg)](https://www.nuget.org/packages/AiUo.AspNet.Hosting)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.AspNet.Hosting 是 AiUo 框架中的 ASP.NET 应用程序托管模块，为 ASP.NET Core 应用程序提供了一套完整的托管解决方案。该模块在 AiUo.Hosting 的基础上，增加了针对 Web 应用程序的特定功能，简化了 ASP.NET Core 应用的配置和部署流程。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.AspNet.Hosting 模块：

#### .NET CLI

```bash
dotnet add package AiUo.AspNet.Hosting
```

#### Package Manager

```powershell
Install-Package AiUo.AspNet.Hosting
```

### ⚙️ 基本配置

```csharp
using AiUo.AspNet.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseAiUoAspNetHosting()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

## 🎯 主要功能

### 🚀 Web 应用托管
- 简化的启动配置和应用程序模板
- 智能的环境适配（开发、测试、生产）
- 优化的 Kestrel 服务器配置

### 🔌 中间件集成
- 预配置的常用 ASP.NET Core 中间件
- 优化的中间件执行顺序
- 灵活的自定义中间件支持

### 🛡️ 安全增强
- 简化的 HTTPS 配置
- 内置的 HSTS 支持
- 自动添加安全相关的 HTTP 标头

### 📊 监控与诊断
- 完整的应用健康状态检查
- 详细的请求和响应诊断信息
- 实时的 Web 应用性能指标收集

## 💡 使用示例

### 📝 配置示例

```csharp
using AiUo.AspNet.Hosting;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAiUoAspNetHosting(options =>
        {
            // 配置健康检查
            options.EnableHealthChecks = true;
            
            // 配置HTTPS重定向
            options.UseHttpsRedirection = true;
            
            // 配置HSTS
            options.UseHsts = true;
            
            // 配置静态文件
            options.UseStaticFiles = true;
        });

        // 添加控制器
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 使用AiUo AspNet Hosting中间件
        app.UseAiUoAspNetHosting();
        
        // 配置路由
        app.UseRouting();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            
            // 配置健康检查端点
            endpoints.MapHealthChecks("/health");
        });
    }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/aspnet-hosting)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/AspNetHosting)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。