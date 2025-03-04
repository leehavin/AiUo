# AiUo.Extensions.AWS 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Extensions.AWS.svg)](https://www.nuget.org/packages/AiUo.Extensions.AWS)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Extensions.AWS.svg)](https://www.nuget.org/packages/AiUo.Extensions.AWS)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Extensions.AWS 是对 AWS SDK 的扩展封装，提供了更简便的 AWS 服务集成方式。该模块支持多种 AWS 服务的操作，包括 S3 存储、SQS 消息队列、SNS 通知服务等，并提供了统一的接口和便捷的配置方式。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Extensions.AWS 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Extensions.AWS
```

#### Package Manager

```powershell
Install-Package AiUo.Extensions.AWS
```

### ⚙️ 基本配置

```csharp
using AiUo.Extensions.AWS;

// 在 Program.cs 中配置服务
var builder = WebApplication.CreateBuilder(args);

// 添加 AWS 服务
builder.Services.AddAWSServices(options =>
{
    options.AccessKeyId = "your-access-key";
    options.SecretAccessKey = "your-secret-key";
    options.Region = "ap-northeast-1";
});
```

## 🎯 主要功能

### 📦 S3 存储服务
- 高效的文件上传下载操作
- 完整的存储桶管理功能
- 精细的文件权限控制机制
- 便捷的预签名URL生成工具

### 📨 SQS 消息队列
- 可靠的消息发送接收机制
- 全面的队列管理功能
- 完善的死信队列支持
- 高性能的消息批量处理

### 📢 SNS 通知服务
- 灵活的主题管理功能
- 高效的消息发布机制
- 多样化的订阅管理选项
- 精准的消息过滤能力

### 🔧 通用功能
- 统一的认证配置接口
- 健壮的异常处理机制
- 智能的重试策略实现
- 完整的日志集成支持

## 💡 使用示例

### 📦 S3 存储操作

```csharp
public class FileService
{
    private readonly IS3Service _s3Service;

    public FileService(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task UploadFileAsync(string bucketName, string key, Stream fileStream)
    {
        try
        {
            await _s3Service.UploadFileAsync(bucketName, key, fileStream);
        }
        catch (AWSException ex)
        {
            // 处理异常
        }
    }

    public async Task<string> GetPreSignedUrlAsync(string bucketName, string key)
    {
        return await _s3Service.GetPreSignedUrlAsync(bucketName, key, TimeSpan.FromHours(1));
    }
}
```

### 📨 SQS 消息队列操作

```csharp
public class MessageService
{
    private readonly ISQSService _sqsService;

    public MessageService(ISQSService sqsService)
    {
        _sqsService = sqsService;
    }

    public async Task SendMessageAsync(string queueUrl, object message)
    {
        await _sqsService.SendMessageAsync(queueUrl, message);
    }

    public async Task<IEnumerable<Message>> ReceiveMessagesAsync(string queueUrl, int maxMessages = 10)
    {
        return await _sqsService.ReceiveMessagesAsync(queueUrl, maxMessages);
    }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/aws)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/AWS)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。