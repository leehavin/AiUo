# MQTT服务器使用说明

## 简介

AiUo.Extensions.MQTT 模块现在支持MQTT服务器功能，可以在同一个应用程序中同时作为MQTT客户端和服务器运行。服务器功能支持以下特性：

- 标准MQTT协议支持
- TLS/SSL加密连接
- 用户名/密码认证
- 主题订阅和发布授权
- 服务器端消息发布

## 配置说明

在`appsettings.json`中添加MQTT服务器配置：

```json
{
  "MQTT": {
    "Enabled": true,
    "MessageLogEnabled": true,
    "DebugLogEnabled": true,
    "ConsumerEnabled": true,
    "DefaultConnectionStringName": "default",
    "ConnectionStrings": {
      "default": {
        "Server": "localhost",
        "Port": 1883,
        "ClientId": "MQTTClient",
        "Username": "user",
        "Password": "password"
      }
    },
    "Server": {
      "Enabled": true,
      "Port": 1883,
      "UseTls": false,
      "TlsPort": 8883,
      "CertificatePath": "",
      "CertificatePassword": "",
      "EnableAuthentication": true,
      "EnableSubscriptionAuthorization": true,
      "EnablePublishAuthorization": true,
      "CloseConnectionOnUnauthorizedSubscription": false,
      "CloseConnectionOnUnauthorizedPublish": false,
      "MaximumConnections": 100,
      "Credentials": [
        "user1:password1",
        "user2:password2"
      ],
      "TopicAccessControl": [
        "client1:topic1,topic2,sensor/#",
        "client2:#"
      ]
    },
    "ConsumerAssemblies": [
      "YourAssemblyName"
    ]
  }
}
```

## 服务器配置选项说明

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| Enabled | 是否启用MQTT服务器 | false |
| Port | 服务器监听端口 | 1883 |
| UseTls | 是否使用TLS/SSL | false |
| TlsPort | TLS/SSL端口 | 8883 |
| CertificatePath | 证书文件路径 | - |
| CertificatePassword | 证书密码 | - |
| EnableAuthentication | 是否启用认证 | false |
| EnableSubscriptionAuthorization | 是否启用订阅授权 | false |
| EnablePublishAuthorization | 是否启用发布授权 | false |
| CloseConnectionOnUnauthorizedSubscription | 未授权订阅时是否关闭连接 | false |
| CloseConnectionOnUnauthorizedPublish | 未授权发布时是否关闭连接 | false |
| MaximumConnections | 最大并发客户端连接数 | 100 |
| Credentials | 用户凭据列表，格式为 "用户名:密码" | [] |
| TopicAccessControl | 主题访问控制列表，格式为 "客户端ID:主题1,主题2,..." | [] |

## 使用示例

### 初始化MQTT服务器

在应用程序启动时，只需调用`AddMQTTEx()`方法即可同时初始化MQTT客户端和服务器：

```csharp
AiUoHost.CreateBuilder().AddMQTTEx().StartAsync();
```

### 从服务器发布消息

```csharp
// 发布字节数组消息
await MQTTServerUtil.PublishFromServerAsync("test/topic", Encoding.UTF8.GetBytes("Hello from server"), 1, false);

// 发布对象消息
var message = new MyMessage { Content = "Hello from server", Value = 42 };
await MQTTServerUtil.PublishFromServerAsync(message, "test/topic", 1, false);

// 使用消息类型上的MQTTPublishMessageAttribute
await MQTTServerUtil.PublishFromServerAsync(message);
```

### 获取服务器实例

如果需要直接访问服务器实例，可以使用以下方法：

```csharp
var server = MQTTServerUtil.GetServer();
// 使用server进行更多操作
```

### 检查服务器状态

```csharp
if (MQTTServerUtil.IsServerStarted())
{
    Console.WriteLine("MQTT服务器已启动");
}
```

## 注意事项

1. 服务器和客户端可以在同一个应用程序中运行，但需要注意避免端口冲突。
2. 如果启用TLS/SSL，需要提供有效的证书文件。
3. 主题访问控制支持MQTT通配符（`#`和`+`）。
4. 服务器会自动随应用程序启动和停止，无需手动管理生命周期。