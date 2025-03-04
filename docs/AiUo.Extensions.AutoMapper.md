# AiUo.Extensions.AutoMapper 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Extensions.AutoMapper.svg)](https://www.nuget.org/packages/AiUo.Extensions.AutoMapper)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Extensions.AutoMapper.svg)](https://www.nuget.org/packages/AiUo.Extensions.AutoMapper)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Extensions.AutoMapper 是对 AutoMapper 对象映射库的扩展封装，提供了更简便的配置方式和更多实用功能。该模块简化了对象之间的映射操作，提高了开发效率。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Extensions.AutoMapper 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Extensions.AutoMapper
```

#### Package Manager

```powershell
Install-Package AiUo.Extensions.AutoMapper
```

### ⚙️ 基本配置

```csharp
using AiUo.Extensions.AutoMapper;

// 在 Program.cs 中配置服务
var builder = WebApplication.CreateBuilder(args);

// 添加 AutoMapper 服务
builder.Services.AddAutoMapper();

// 或者指定程序集扫描
builder.Services.AddAutoMapper(typeof(Program).Assembly);
```

## 🎯 主要功能

### 🔄 自动映射配置
- 智能的自动注册映射配置
- 灵活的配置文件方式定义映射规则
- 强大的默认映射约定支持

### 🔌 映射扩展
- 完整的深层对象映射支持
- 高效的集合映射优化
- 可定制的自定义值转换器

### 🧩 依赖注入集成
- 便捷的 IMapper 服务自动注册
- 精确的作用域管理
- 可靠的映射配置生命周期控制

### ⚡ 性能优化
- 安全的编译时类型检查
- 高效的映射缓存机制
- 强大的批量映射优化

## 💡 使用示例

### 📝 定义映射配置

```csharp
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.FullName, opt => 
                opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    }
}
```

### 🔍 使用映射

```csharp
public class UserService
{
    private readonly IMapper _mapper;

    public UserService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public User ConvertToUser(UserDto userDto)
    {
        return _mapper.Map<User>(userDto);
    }

    public List<User> ConvertToUsers(List<UserDto> userDtos)
    {
        return _mapper.Map<List<User>>(userDtos);
    }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/automapper)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/AutoMapper)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。