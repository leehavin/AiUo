# AiUo.Extensions.SMS 模块

## 🎯 主要功能
### 📱 短信服务
- 支持多平台短信发送（阿里云、腾讯云等）
- 统一的短信发送接口
- 灵活的配置选项
- 完善的错误处理机制

## 使用方法

### 1. 配置

在 `appsettings.json` 中添加配置：

```json
{
  "SMS": {
    "Provider": "Aliyun",  // 可选值: Aliyun, Tencent
    "Aliyun": {
      "AccessKeyId": "your-access-key-id",
      "AccessKeySecret": "your-access-key-secret",
      "SignName": "your-sign-name",
      "RegionId": "cn-hangzhou"
    },
    "Tencent": {
      "SecretId": "your-secret-id",
      "SecretKey": "your-secret-key",
      "SmsSdkAppId": "your-sdk-app-id",
      "SignName": "your-sign-name"
    }
  }
}
```

### 2. 注册服务

```csharp
// 在 Program.cs 中
builder.Host.ConfigureAiUo(aiuo =>
{
    aiuo.UseSMS();
});
```

### 3. 发送短信

```csharp
// 注入短信服务和模板服务
private readonly ISmsService _smsService;
private readonly ISmsTemplateService _templateService;

public YourClass(ISmsService smsService, ISmsTemplateService templateService)
{
    _smsService = smsService;
    _templateService = templateService;
}

// 发送短信
public void SendSms()
{
    // 使用模板编码发送短信（服务会自动映射到对应供应商的模板ID）
    var message = new SmsMessage
    {
        PhoneNumber = "13800138000",
        TemplateCode = "verification_code", // 使用模板编码而非供应商特定的模板ID
        TemplateParams = new Dictionary<string, string>
        {
            { "code", "123456" },
            { "minutes", "5" }
        }
    };

    var result = _smsService.Send(message);
    if (result.Success)
    {
        Console.WriteLine($"短信发送成功，消息ID: {result.MessageId}");
    }
    else
    {
        Console.WriteLine($"短信发送失败，错误码: {result.ErrorCode}，错误信息: {result.ErrorMessage}");
    }
}
```

### 4. 自定义实现

如需添加其他短信服务商支持，只需实现 `ISmsService` 接口：

```csharp
public class CustomSmsService : BaseSmsService
{
    public CustomSmsService(IOptions<SmsOptions> options, ILogger<CustomSmsService> logger)
        : base(options, logger)
    {
    }

    public override Task<SmsResult> SendAsync(SmsMessage message)
    {
        // 实现自定义短信发送逻辑
    }
}
```