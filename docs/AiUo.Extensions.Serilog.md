# AiUo.Extensions.Serilog 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Extensions.Serilog.svg)](https://www.nuget.org/packages/AiUo.Extensions.Serilog)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Extensions.Serilog.svg)](https://www.nuget.org/packages/AiUo.Extensions.Serilog)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Extensions.Serilog 是对 Serilog 日志框架的扩展封装，提供了更便捷的日志配置和输出方式。该模块支持多种日志输出目标，提供了结构化日志记录功能，并集成了常用的日志增强特性。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Extensions.Serilog 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Extensions.Serilog
```

#### Package Manager

```powershell
Install-Package AiUo.Extensions.Serilog
```

### ⚙️ 基本配置

```csharp
using AiUo.Extensions.Serilog;

// 在 Program.cs 中配置服务
var builder = WebApplication.CreateBuilder(args);

// 添加 Serilog 服务
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day);
});
```

## 🎯 主要功能

### ⚙️ 日志配置
- 多样化的配置方式支持（代码、配置文件）
- 智能的动态日志级别调整
- 灵活的日志模板自定义
- 完整的环境变量集成

### 📤 输出目标
- 增强的控制台输出功能
- 高效的文件日志管理
- 可靠的数据库日志存储
- 强大的 Elasticsearch 集成
- 便捷的 Seq 日志服务器支持

### 🔌 日志增强
- 精确的结构化日志记录
- 丰富的上下文信息注入
- 详细的异常信息格式化
- 可扩展的日志事件扩展

### ⚡ 性能优化
- 高效的异步日志写入
- 智能的缓冲区管理
- 批量的日志处理机制
- 优化的资源使用策略

## 💡 使用示例

### 📝 代码配置方式

```csharp
using AiUo.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);

// 添加 Serilog 服务
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.WithProperty("Application", "MyApp")
        .Enrich.WithMachineName()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
});
```

### 📋 配置文件方式

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/serilog)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/Serilog)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。