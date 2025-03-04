# AiUo 基础库

[![NuGet](https://img.shields.io/nuget/v/AiUo.svg)](https://www.nuget.org/packages/AiUo)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.svg)](https://www.nuget.org/packages/AiUo)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo 基础库是整个框架的核心组件，提供了丰富的工具类和基础功能，为其他模块提供基础支持。该库基于 .NET 8.0 构建，采用模块化设计，提供了一系列可重用的组件和工具类。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo 基础库：

#### .NET CLI

```bash
dotnet add package AiUo
```

#### Package Manager

```powershell
Install-Package AiUo
```

### ⚙️ 基本配置

```csharp
using AiUo;

// 在 Program.cs 中配置服务
var builder = WebApplication.CreateBuilder(args);

// 添加 AiUo 基础服务
builder.Services.AddAiUo(options =>
{
    // 配置选项
    options.UseDefaultConfiguration();
});
```

## 🎯 主要功能

### 💾 缓存管理 (Caching)
- 高性能内存缓存的封装和扩展
- 分布式缓存的抽象接口和实现
- 灵活的缓存策略和失效机制

### 📚 集合操作 (Collections)
- 丰富的泛型集合扩展方法
- 线程安全的高性能集合类
- 优化的数据结构实现

### 🛠️ 通用工具 (Common)
- 智能类型转换工具
- 全面的验证方法
- 高效的编码解码工具

### ⚙️ 配置管理 (Configuration)
- 灵活的配置文件读取和解析
- 多环境变量管理
- 实时配置更新机制

### 🏗️ 核心功能 (Core)
- 强大的依赖注入容器
- 完善的生命周期管理
- 统一的应用程序上下文

### 💽 数据访问 (Data)
- 智能数据库连接管理
- 可靠的事务处理机制
- 高效的查询构建器

### 🏢 托管服务 (Hosting)
- 稳定的应用程序托管
- 精确的服务生命周期管理
- 灵活的启动配置

### 📁 IO 操作
- 安全的文件操作封装
- 高效的流处理工具
- 可靠的压缩解压缩功能

### 📝 日志记录 (Logging)
- 统一的日志接口设计
- 多目标日志输出支持
- 精细的日志级别控制

### 🌐 网络通信 (Net)
- 现代化的 HTTP 客户端封装
- 高性能的 TCP/UDP 通信
- 实用的网络工具集

### 🎲 随机数生成 (Randoms)
- 密码学安全的随机数生成
- 多样化的随机字符串生成
- 高性能的唯一标识符生成

### 🔍 反射工具 (Reflection)
- 高效的类型信息获取
- 优化的动态方法调用
- 强大的特性处理功能

### 🔒 安全功能 (Security)
- 标准的加密解密实现
- 高效的哈希计算
- 安全的令牌处理机制

### 📄 文本处理 (Text)
- 全面的字符串处理工具
- 高效的正则表达式工具
- 智能的文本编码转换

### 📋 XML 处理
- 便捷的 XML 文档操作
- 高效的 XML 序列化反序列化
- 强大的 XPath 查询支持

## 🔧 高级配置

```csharp
builder.Services.AddAiUo(options =>
{
    // 配置缓存
    options.ConfigureCache(cache =>
    {
        cache.DefaultExpiration = TimeSpan.FromMinutes(30);
        cache.EnableCompression = true;
    });

    // 配置日志
    options.ConfigureLogging(logging =>
    {
        logging.MinimumLevel = LogLevel.Information;
        logging.AddConsole();
    });

    // 配置安全选项
    options.ConfigureSecurity(security =>
    {
        security.EnableEncryption = true;
        security.UseStrongCryptography = true;
    });
});
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。