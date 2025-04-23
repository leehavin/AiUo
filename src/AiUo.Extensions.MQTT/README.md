# AiUo.Extensions.MQTT 优化说明

## 优化内容

### 1. 解决 "Unknown element with name 'doc' found in keyring" 警告

- 添加了 `DisableKeyring` 选项，默认设置为 `true`，通过调用 `WithDisableKeyring()` 方法禁用密钥环功能，从而解决警告问题

### 2. 增强连接稳定性

- 实现了更强大的自动重连机制，支持指数退避算法，避免频繁重连导致的资源消耗
- 添加了 `MaxReconnectAttempts` 参数，限制最大重连尝试次数，防止无限重连
- 增加了重连日志，便于问题诊断和监控
- 优化了异常处理，提高了连接失败时的容错能力

### 3. 避免客户端ID冲突

- 添加了 `UseRandomClientIdSuffix` 选项，为客户端ID添加随机后缀，避免多个客户端使用相同ID导致的连接冲突
- 优化了客户端ID生成逻辑，确保唯一性

### 4. 增强消息处理能力

- 添加了 `MaxPendingMessages` 属性，控制最大挂起消息数量，默认值为100
- 添加了消息队列相关配置，包括 `EnableMessageQueueing` 和 `MaxQueueSize`

### 5. 支持遗嘱消息

- 添加了完整的遗嘱消息（Last Will）支持，包括主题、内容、QoS和保留标志
- 通过配置文件可灵活设置遗嘱消息参数

## 配置示例

```json
"MQTT": {
  "ClientOptions": {
    "AutoReconnectDelayMs": 5000,
    "MaxReconnectAttempts": 10,
    "EnableMessageQueueing": true,
    "MaxQueueSize": 1000,
    "DisableKeyring": true,
    "UseRandomClientIdSuffix": true,
    "EnableLastWill": false,
    "LastWillTopic": "clients/status",
    "LastWillMessage": "Offline",
    "LastWillQoS": 1,
    "LastWillRetain": true
  },
  "ConnectionStrings": {
    "default": {
      "Server": "localhost",
      "Port": 1883,
      "ClientId": "MQTTClient",
      "Username": "user",
      "Password": "password",
      "AutoReconnect": true,
      "ReconnectDelay": 5,
      "MaxPendingMessages": 100
    }
  }
}
```

## 使用说明

无需修改现有代码，只需在配置文件中添加相应的配置项即可启用新功能。所有新增选项都有合理的默认值，确保向后兼容性。