# AiUo.Data.MySql 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Data.MySql.svg)](https://www.nuget.org/packages/AiUo.Data.MySql)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Data.MySql.svg)](https://www.nuget.org/packages/AiUo.Data.MySql)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Data.MySql 是 AiUo 框架的 MySQL 数据访问模块，提供了对 MySQL 数据库的高效访问能力。该模块封装了底层数据库操作，提供了简洁易用的 API，支持事务管理、连接池优化、查询构建等功能，大幅简化了数据库访问代码的编写。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Data.MySql 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Data.MySql
```

#### Package Manager

```powershell
Install-Package AiUo.Data.MySql
```

### ⚙️ 基本配置

```csharp
using AiUo.Data.MySql;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 注册 MySQL 服务
        services.AddAiUoMySql(options =>
        {
            options.ConnectionString = "Server=localhost;Database=mydb;User=root;Password=password;";
            options.MinPoolSize = 5;
            options.MaxPoolSize = 100;
        });
    }
}
```

## 🎯 主要功能

### 🔌 数据库连接管理
- 智能的连接字符串配置和管理
- 高效的连接池优化
- 完善的多数据库支持

### 💾 数据访问操作
- 全面的 CRUD 操作封装
- 高性能的批量操作支持
- 安全的参数化查询

### 🔄 事务管理
- 灵活的事务控制接口
- 可靠的分布式事务支持
- 智能的嵌套事务处理

### 🔍 查询构建
- 直观的流式 API 查询构建
- 强大的条件查询支持
- 便捷的排序和分页功能

### 🗺️ ORM 功能
- 高效的对象关系映射
- 自动的实体类映射
- 灵活的自定义映射配置

### 📊 数据库架构
- 完整的表结构管理
- 高效的索引操作
- 可靠的数据库迁移支持

## 💡 使用示例

### 📋 基本查询示例

```csharp
using AiUo.Data.MySql;

public class UserRepository
{
    private readonly MySqlDatabase _database;

    public UserRepository(MySqlDatabase database)
    {
        _database = database;
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        var sql = "SELECT * FROM Users WHERE Id = @UserId";
        return await _database.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _database.QueryAsync<User>("SELECT * FROM Users");
    }

    public async Task<int> CreateUserAsync(User user)
    {
        var sql = @"INSERT INTO Users (Name, Email, CreatedAt) 
                  VALUES (@Name, @Email, @CreatedAt);
                  SELECT LAST_INSERT_ID()";
        
        return await _database.ExecuteScalarAsync<int>(sql, user);
    }
}
```

## 📚 最佳实践

1. **连接管理**
   - 合理配置连接池大小
   - 及时释放数据库连接
   - 使用异步方法提高性能

2. **事务处理**
   - 合理划分事务边界
   - 正确处理事务嵌套
   - 注意事务隔离级别

3. **性能优化**
   - 使用参数化查询
   - 避免大事务
   - 合理使用批量操作

4. **安全性**
   - 防止 SQL 注入
   - 最小权限原则
   - 敏感数据加密

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/mysql)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/MySql)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。