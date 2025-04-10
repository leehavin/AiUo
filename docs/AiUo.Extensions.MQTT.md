# AiUo.Extensions.MQTT 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Extensions.MQTT.svg)](https://www.nuget.org/packages/AiUo.Extensions.MQTT)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Extensions.MQTT.svg)](https://www.nuget.org/packages/AiUo.Extensions.MQTT)

## 简介

AiUo.Extensions.MQTT 是对 MQTT 协议的扩展封装，提供了更简便的消息发布和订阅功能。该模块简化了 MQTT 的配置和使用流程，支持多种服务质量等级(QoS)，并提供了可靠的消息处理机制。

## 安装

选择以下方式之一安装 AiUo.Extensions.MQTT 模块：

### .NET CLI

```bash
dotnet add package AiUo.Extensions.MQTT
```

### Package Manager

```powershell
Install-Package AiUo.Extensions.MQTT
```

## 使用方法

### 1. 引入命名空间

```csharp
using AiUo.Extensions.MQTT;
```

### 2. 配置

在 `appsettings.json` 中添加 MQTT 配置：

```json
{
  "MQTT": {
    "Enabled": true,
    "MessageLogEnabled": true,
    "DebugLogEnabled": false,
    "ConsumerEnabled": true,
    "DefaultConnectionStringName": "default",
    "ConnectionStrings": {
      "default": {
        "Server": "localhost",
        "Port": 1883,
        "ClientId": "MyClientId",
        "Username": "user",
        "Password": "password",
        "UseTls": false,
        "CleanSession": true,
        "KeepAlivePeriod": 60,
        "CommunicationTimeout": 10,
        "ReconnectDelay": 5
      }
    },
    "ConsumerAssemblies": [
      "MyApp.MQTT"
    ]
  }
}
```

### 3. 注册服务

在 `Program.cs` 或 `Startup.cs` 中注册 MQTT 服务：

```csharp
var builder = Host.CreateDefaultBuilder(args)
    .AddMQTTEx();
```

### 4. 发布消息

#### 4.1 定义消息类型

```csharp
[MQTTPublishMessage("my/topic", 1, false)]
public class MyMessage : MQTTMessage
{
    public string Content { get; set; }
    public int Value { get; set; }
}
```

#### 4.2 发布消息

```csharp
// 使用消息类型上的主题和QoS
var message = new MyMessage { Content = "Hello MQTT", Value = 42 };
MQTTUtil.Publish(message);

// 或者指定主题和QoS
MQTTUtil.Publish(message, "custom/topic", 2, true);

// 异步发布
await MQTTUtil.PublishAsync(message);
```

### 5. 订阅消息

#### 5.1 使用消费者类

```csharp
public class MyMessageConsumer : MQTTSubscribeConsumer<MyMessage>
{
    public MyMessageConsumer() : base("my/topic", 1)
    {
    }

    protected override async Task ProcessMessageAsync(MyMessage message)
    {
        Console.WriteLine($"收到消息: {message.Content}, 值: {message.Value}");
        await Task.CompletedTask;
    }
}
```

#### 5.2 使用订阅方法

```csharp
// 直接订阅主题
MQTTUtil.Subscribe("my/topic", e => {
    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.Array);
    Console.WriteLine($"收到消息: {payload}");
});

// 异步订阅
await MQTTUtil.SubscribeAsync("my/topic", HandleMessage, 1);

// 取消订阅
MQTTUtil.Unsubscribe("my/topic");
```

## 高级功能

### 1. 使用不同的连接

```csharp
// 发布消息到指定连接
MQTTUtil.Publish(message, connectionStringName: "connection2");

// 从指定连接订阅
MQTTUtil.Subscribe("my/topic", HandleMessage, connectionStringName: "connection2");
```

### 2. 消息保留

```csharp
// 发布保留消息
MQTTUtil.Publish(message, retain: true);
```

### 3. 不同的服务质量等级(QoS)

```csharp
// QoS 0 - 最多一次传递（不保证）
MQTTUtil.Publish(message, qosLevel: 0);

// QoS 1 - 至少一次传递（可能重复）
MQTTUtil.Publish(message, qosLevel: 1);

// QoS 2 - 恰好一次传递（保证且不重复）
MQTTUtil.Publish(message, qosLevel: 2);
```

## 注意事项

1. 确保MQTT服务器已正确配置并可访问
2. 对于生产环境，建议使用TLS/SSL加密连接
3. 选择合适的QoS级别以平衡性能和可靠性
4. 使用有意义的主题结构以便于管理和过滤