# AiUo.AspNet 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.AspNet.svg)](https://www.nuget.org/packages/AiUo.AspNet)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.AspNet.svg)](https://www.nuget.org/packages/AiUo.AspNet)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.AspNet 是 AiUo 框架中的 ASP.NET 扩展模块，为 ASP.NET Core 应用程序提供了一系列实用的扩展功能和中间件。该模块旨在简化 Web 应用程序的开发，提供统一的异常处理、身份认证、跨域配置等常用功能。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.AspNet 模块：

#### .NET CLI

```bash
dotnet add package AiUo.AspNet
```

#### Package Manager

```powershell
Install-Package AiUo.AspNet
```

### ⚙️ 基本配置

```csharp
using AiUo.AspNet;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAiUoAspNet(options =>
        {
            // 配置全局异常处理
            options.UseGlobalExceptionHandler = true;
            
            // 配置请求日志
            options.EnableRequestLogging = true;
            
            // 配置跨域策略
            options.Cors.Enable = true;
            options.Cors.AllowedOrigins = new[] { "https://example.com" };
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 使用 AiUo AspNet 中间件
        app.UseAiUoAspNet();
    }
}
```

## 🎯 主要功能

### 🛡️ 中间件
- 统一的异常捕获和处理机制
- 自动记录 HTTP 请求和响应信息
- 实时的请求执行时间和资源使用统计

### 🔒 安全功能
- 完整的身份认证集成（JWT、Cookie等）
- 灵活的基于角色和策略的授权机制
- 简化的跨域资源共享（CORS）配置

### 🔌 MVC 扩展
- 增强的模型验证功能
- 标准化的 API 响应格式
- 完善的 API 版本管理支持

### 📝 Swagger 集成
- 智能的 API 文档自动生成
- 可定制的 Swagger UI 界面
- 安全的文档访问控制

## 💡 使用示例

### 📋 统一响应格式

```csharp
using AiUo.AspNet.Models;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ApiResponse<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return ApiResponse.Success(user);
    }

    [HttpPost]
    public async Task<ApiResponse> CreateUser(CreateUserDto dto)
    {
        await _userService.CreateAsync(dto);
        return ApiResponse.Success();
    }
}
```

### 🔐 身份认证配置

```csharp
using AiUo.AspNet.Authentication;

public void ConfigureServices(IServiceCollection services)
{
    services.AddAiUoAuthentication(options =>
    {
        options.JwtBearer.Enable = true;
        options.JwtBearer.SecurityKey = "your-secret-key";
        options.JwtBearer.Issuer = "your-issuer";
        options.JwtBearer.Audience = "your-audience";
        options.JwtBearer.ExpiresInMinutes = 60;
    });
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/aspnet)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/AspNet)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。