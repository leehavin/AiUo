# AiUo.Extensions.RabbitMQ 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Extensions.RabbitMQ.svg)](https://www.nuget.org/packages/AiUo.Extensions.RabbitMQ)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Extensions.RabbitMQ.svg)](https://www.nuget.org/packages/AiUo.Extensions.RabbitMQ)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Extensions.RabbitMQ 是对 RabbitMQ 消息队列的扩展封装，提供了更简便的消息发布和订阅功能。该模块简化了 RabbitMQ 的配置和使用流程，支持多种消息交换模式，并提供了可靠的消息处理机制。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Extensions.RabbitMQ 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Extensions.RabbitMQ
```

#### Package Manager

```powershell
Install-Package AiUo.Extensions.RabbitMQ
```

### ⚙️ 基本配置

```csharp
using AiUo.Extensions.RabbitMQ;

// 在 Program.cs 中配置服务
var builder = WebApplication.CreateBuilder(args);

// 添加 RabbitMQ 服务
builder.Services.AddRabbitMQ(options =>
{
    options.HostName = "localhost";
    options.UserName = "guest";
    options.Password = "guest";
    options.VirtualHost = "/";
    options.Port = 5672;
});
```

## 🎯 主要功能

### 🔌 连接管理
- 高效的连接池管理
- 智能的自动重连机制
- 强大的集群支持
- 实时的连接健康检查

### 📤 消息发布
- 完整的交换机类型支持（Direct, Topic, Fanout, Headers）
- 可靠的消息持久化
- 实时的发布确认机制
- 高性能的批量发布优化

### 📥 消息订阅
- 精确的消费者并发控制
- 可靠的消息确认机制
- 灵活的消息重试策略
- 完善的死信队列处理

### 🛠️ 高级特性
- 高效的消息序列化/反序列化
- 智能的消息路由策略
- 完整的消息优先级支持
- 灵活的延迟消息处理

## 💡 使用示例

### 📤 发布消息

```csharp
public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public OrderService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task CreateOrderAsync(OrderDto orderDto)
    {
        // 业务逻辑处理
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = orderDto.CustomerId,
            Amount = orderDto.Amount,
            CreatedAt = DateTime.UtcNow
        };
        
        // 发布订单创建事件
        await _publisher.PublishAsync("order.created", order, new PublishOptions
        {
            Exchange = "orders",
            RoutingKey = "order.created",
            Persistent = true
        });
    }
}
```

### 📥 订阅消息

```csharp
public class OrderCreatedHandler : IMessageHandler<Order>
{
    private readonly ILogger<OrderCreatedHandler> _logger;

    public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(Order order, MessageContext context)
    {
        _logger.LogInformation("收到新订单: {OrderId}", order.Id);
        // 处理订单逻辑
        await ProcessOrderAsync(order);
    }
}

// 注册消息处理器
services.AddMessageHandler<OrderCreatedHandler>("orders", "order.created");
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/rabbitmq)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/RabbitMQ)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。