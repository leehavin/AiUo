# AiUo.Hosting 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Hosting.svg)](https://www.nuget.org/packages/AiUo.Hosting)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Hosting.svg)](https://www.nuget.org/packages/AiUo.Hosting)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Hosting 模块是 AiUo 框架的核心托管服务组件，提供了应用程序生命周期管理、依赖注入配置和服务托管的功能。该模块基于 .NET 8.0 的泛型主机（Generic Host）构建，简化了应用程序的启动、运行和关闭过程。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Hosting 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Hosting
```

#### Package Manager

```powershell
Install-Package AiUo.Hosting
```

### ⚙️ 基本配置

```csharp
using AiUo.Hosting;

// 创建并配置 AiUo 主机
var host = AiUoHost.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // 注册服务
        services.AddSingleton<IMyService, MyService>();
    })
    .ConfigureLogging((context, logging) =>
    {
        // 配置日志
        logging.AddConsole();
    })
    .Build();

// 运行主机
host.Run();
```

## 🎯 主要功能

### 🏢 应用程序托管
- 统一的应用程序启动入口
- 优雅的应用程序关闭机制
- 环境感知的配置加载

### ⏱️ 服务生命周期管理
- 托管服务的注册和管理
- 后台服务的生命周期控制
- 服务依赖关系的自动解析

### ⚙️ 配置管理
- 多源配置支持（文件、环境变量、命令行等）
- 配置变更通知机制
- 分层配置结构

### 💉 依赖注入
- 服务注册的扩展方法
- 服务生命周期管理（Singleton、Scoped、Transient）
- 服务解析和工厂模式支持

## 🔧 高级用法

### 🔄 托管服务示例

```csharp
using AiUo.Hosting;
using Microsoft.Extensions.Hosting;

public class MyHostedService : IHostedService
{
    private readonly ILogger<MyHostedService> _logger;

    public MyHostedService(ILogger<MyHostedService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("服务启动中...");
        // 初始化逻辑
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("服务停止中...");
        // 清理逻辑
        return Task.CompletedTask;
    }
}

// 注册托管服务
services.AddHostedService<MyHostedService>();
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/hosting)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/Hosting)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。