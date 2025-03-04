# AiUo.Data.SqlSugar 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Data.SqlSugar.svg)](https://www.nuget.org/packages/AiUo.Data.SqlSugar)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Data.SqlSugar.svg)](https://www.nuget.org/packages/AiUo.Data.SqlSugar)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Data.SqlSugar 是 AiUo 框架中的 SqlSugar ORM 集成模块，为开发者提供了一套简单、高效的数据访问解决方案。SqlSugar 是一个轻量级的 .NET ORM 框架，支持多种数据库，包括 SQL Server、MySQL、PostgreSQL、Oracle 等。本模块对 SqlSugar 进行了封装和扩展，使其更好地融入 AiUo 框架生态。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Data.SqlSugar 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Data.SqlSugar
```

#### Package Manager

```powershell
Install-Package AiUo.Data.SqlSugar
```

### ⚙️ 基本配置

```csharp
using AiUo.Data.SqlSugar;

public void ConfigureServices(IServiceCollection services)
{
    services.AddAiUoSqlSugar(options =>
    {
        // 添加默认数据库连接
        options.ConnectionString = "Server=localhost;Database=testdb;Uid=root;Pwd=password;";
        options.DbType = DbType.MySql;
        
        // 可选：添加其他数据库连接
        options.SlaveConnectionConfigs = new List<SlaveConnectionConfig>
        {
            new SlaveConnectionConfig { ConnectionString = "Server=slave1;Database=testdb;Uid=root;Pwd=password;", HitRate = 10 }
        };
        
        // 可选：配置其他选项
        options.IsAutoCloseConnection = true;
        options.InitKeyType = InitKeyType.Attribute;
    });
}
```

## 🎯 主要功能

### 🔌 无缝集成
- 智能的依赖注入支持
- 简化的配置方式
- 完整的多数据库支持

### 🛠️ 仓储模式
- 强大的通用仓储实现
- 丰富的仓储扩展方法
- 可靠的工作单元支持

### 📊 查询增强
- 灵活的 Lambda 表达式支持
- 高效的分页查询功能
- 强大的动态查询能力

### 🔄 代码生成
- 自动的实体类生成
- 便捷的仓储代码生成
- 可定制的代码模板

## 💡 使用示例

### 📝 实体定义

```csharp
using AiUo.Data.SqlSugar.Attributes;
using SqlSugar;

[SugarTable("users")]
public class User
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    
    [SugarColumn(Length = 50, IsNullable = false)]
    public string Username { get; set; }
    
    [SugarColumn(Length = 100)]
    public string Email { get; set; }
    
    [SugarColumn(IsNullable = true)]
    public DateTime? CreatedAt { get; set; }
}
```

### 🔍 仓储使用

```csharp
using AiUo.Data.SqlSugar.Repositories;

public class UserService
{
    private readonly IRepository<User> _userRepository;
    
    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }
    
    public async Task<List<User>> GetUsersByConditionAsync(string email)
    {
        return await _userRepository.GetListAsync(u => u.Email == email);
    }
    
    public async Task<bool> CreateUserAsync(User user)
    {
        return await _userRepository.InsertAsync(user);
    }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/sqlsugar)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/SqlSugar)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。