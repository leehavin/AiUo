# AiUo.Extensions.CommandLineParser 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Extensions.CommandLineParser.svg)](https://www.nuget.org/packages/AiUo.Extensions.CommandLineParser)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Extensions.CommandLineParser.svg)](https://www.nuget.org/packages/AiUo.Extensions.CommandLineParser)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Extensions.CommandLineParser 是一个专业级的命令行参数解析框架，专为 .NET 应用程序设计。它提供了强大而优雅的 API，支持复杂的命令行参数解析场景，帮助开发者构建出专业、可靠的命令行应用程序。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Extensions.CommandLineParser 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Extensions.CommandLineParser
```

#### Package Manager

```powershell
Install-Package AiUo.Extensions.CommandLineParser
```

## 🎯 主要功能

### 🔍 智能参数解析
- 支持多种标准参数格式（-p value, --param value, /param:value）
- 智能处理必选和可选参数
- 内置布尔开关参数支持
- 自动类型转换和验证

### ✅ 强大的验证机制
- 内置必选参数验证
- 支持参数值范围约束
- 参数间依赖关系校验
- 灵活的自定义验证规则

### 📚 专业文档支持
- 自动生成标准化帮助文档
- 多语言本地化支持
- 可自定义的文档模板

### 🛠️ 企业级功能
- 完整的子命令系统
- 便捷的环境变量集成
- 灵活的配置文件支持
- 健壮的错误处理机制

## 💡 使用示例

### 📝 基础示例

```csharp
using AiUo.Extensions.CommandLineParser;

// 定义命令行选项
public class Options
{
    [Option('f', "file", Required = true, HelpText = "输入文件路径")]
    public string InputFile { get; set; }

    [Option('o', "output", Required = false, HelpText = "输出文件路径")]
    public string OutputFile { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "启用详细日志输出")]
    public bool Verbose { get; set; }
}

// 实现解析逻辑
public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunOptions)
            .WithNotParsed(HandleParseError);
    }

    static void RunOptions(Options opts)
    {
        if (opts.Verbose)
        {
            Console.WriteLine($"正在处理文件: {opts.InputFile}");
        }
        // 业务逻辑实现
    }

    static void HandleParseError(IEnumerable<Error> errs)
    {
        // 错误处理逻辑
    }
}
```

### 🔥 子命令实现

```csharp
[Verb("add", HelpText = "添加新文件")]
public class AddOptions
{
    [Option('n', "name", Required = true, HelpText = "文件名称")]
    public string FileName { get; set; }
}

[Verb("remove", HelpText = "删除现有文件")]
public class RemoveOptions
{
    [Option('i', "id", Required = true, HelpText = "文件ID")]
    public int FileId { get; set; }
}

public class Program
{
    public static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<AddOptions, RemoveOptions>(args)
            .MapResult(
                (AddOptions opts) => RunAddCommand(opts),
                (RemoveOptions opts) => RunRemoveCommand(opts),
                errs => 1);
    }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/commandlineparser)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/CommandLineParser)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。