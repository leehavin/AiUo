# AiUo.Extensions.Nacos 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Extensions.Nacos.svg)](https://www.nuget.org/packages/AiUo.Extensions.Nacos)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Extensions.Nacos.svg)](https://www.nuget.org/packages/AiUo.Extensions.Nacos)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Extensions.Nacos 是对阿里巴巴 Nacos 服务的扩展封装，提供了更简便的服务注册、发现和配置管理功能。该模块简化了 Nacos 的接入流程，支持动态配置更新、服务健康检查和负载均衡等特性，适用于微服务架构中的服务治理场景。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Extensions.Nacos 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Extensions.Nacos
```

#### Package Manager

```powershell
Install-Package AiUo.Extensions.Nacos
```

### ⚙️ 基本配置

```csharp
using AiUo.Extensions.Nacos;

// 在 Program.cs 中配置服务
var builder = WebApplication.CreateBuilder(args);

// 添加 Nacos 服务
builder.Services.AddNacos(options =>
{
    options.ServerAddresses = new[] { "http://localhost:8848" };
    options.Namespace = "public";
    options.ServiceName = "my-service";
    options.GroupName = "DEFAULT_GROUP";
    options.ClusterName = "DEFAULT";
    options.Weight = 100;
    options.Metadata.Add("version", "1.0.0");
});
```

## 🎯 主要功能

### 🔍 服务注册与发现
- 自动的服务注册机制
- 可靠的服务健康检查
- 精确的服务实例管理
- 实时的服务订阅与监听

### ⚙️ 配置管理
- 高效的动态配置获取
- 实时的配置变更监听
- 完整的配置版本管理
- 安全的命名空间隔离

### ⚖️ 负载均衡
- 多样化的负载均衡策略
- 灵活的权重配置支持
- 智能的故障转移机制
- 精准的服务路由控制

### 🔌 集成功能
- 无缝的 ASP.NET Core 集成
- 便捷的依赖注入支持
- 完善的日志集成机制
- 全面的指标监控能力

## 💡 使用示例

### 🔄 服务注册与发现

```csharp
public class OrderController : ControllerBase
{
    private readonly INacosServiceDiscovery _serviceDiscovery;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderController(INacosServiceDiscovery serviceDiscovery, IHttpClientFactory httpClientFactory)
    {
        _serviceDiscovery = serviceDiscovery;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        // 获取支付服务实例
        var instance = await _serviceDiscovery.SelectOneHealthyInstance("payment-service");
        if (instance == null)
        {
            return StatusCode(503, "支付服务不可用");
        }

        // 构建请求URL
        var requestUrl = $"{instance.GetUrl()}/api/payments/{id}";
        
        // 发送请求
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(requestUrl);
        
        // 处理响应
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Ok(JsonSerializer.Deserialize<PaymentInfo>(content));
        }
        
        return StatusCode((int)response.StatusCode, "支付服务请求失败");
    }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/nacos)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/Nacos)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。