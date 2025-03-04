# AiUo.DbCaching 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.DbCaching.svg)](https://www.nuget.org/packages/AiUo.DbCaching)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.DbCaching.svg)](https://www.nuget.org/packages/AiUo.DbCaching)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.DbCaching 是 AiUo 框架中的数据库缓存模块，提供了一套高效的数据库查询结果缓存解决方案。该模块通过缓存频繁访问的数据库查询结果，显著减少数据库访问次数，提高应用程序性能，同时降低数据库负载。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.DbCaching 模块：

#### .NET CLI

```bash
dotnet add package AiUo.DbCaching
```

#### Package Manager

```powershell
Install-Package AiUo.DbCaching
```

### ⚙️ 基本配置

```csharp
using AiUo.DbCaching;

public void ConfigureServices(IServiceCollection services)
{
    // 添加数据库缓存服务
    services.AddAiUoDbCaching(options =>
    {
        // 配置缓存提供程序（默认使用内存缓存）
        options.CacheProvider = CacheProviderType.Memory;
        
        // 设置默认缓存过期时间
        options.DefaultExpiration = TimeSpan.FromMinutes(30);
        
        // 启用查询缓存
        options.EnableQueryCache = true;
        
        // 启用二级缓存
        options.EnableSecondLevelCache = true;
    });
}
```

## 🎯 主要功能

### 🚀 高性能缓存
- 多级缓存策略支持（内存缓存、分布式缓存）
- 智能的自动缓存失效机制
- 精确的基于表依赖的缓存自动更新

### 🔌 无缝集成
- 与 AiUo.Data 系列模块的无缝集成
- 对开发者透明的缓存实现
- 完善的依赖注入支持

### ⚙️ 灵活配置
- 细粒度的缓存策略控制
- 基于查询条件的智能缓存决策
- 强大的缓存标签管理系统

### 📊 缓存监控
- 实时的缓存命中率统计
- 全面的缓存使用情况监控
- 可视化的性能指标展示

## 💡 使用示例

### 📝 使用查询缓存

```csharp
using AiUo.DbCaching;

public class UserService
{
    private readonly IRepository<User> _userRepository;
    
    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<List<User>> GetActiveUsersAsync()
    {
        // 使用缓存查询，结果将被缓存
        return await _userRepository.AsQueryable()
            .Where(u => u.IsActive)
            .WithCache(TimeSpan.FromMinutes(10)) // 指定缓存时间
            .ToListAsync();
    }
    
    public async Task<User> GetUserByIdAsync(int id)
    {
        // 使用缓存查询单个实体
        return await _userRepository.AsQueryable()
            .Where(u => u.Id == id)
            .WithCache() // 使用默认缓存时间
            .FirstOrDefaultAsync();
    }
}
```

### 📋 缓存标签和依赖

```csharp
public class ProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly ICacheManager _cacheManager;
    
    public ProductService(IRepository<Product> productRepository, ICacheManager cacheManager)
    {
        _productRepository = productRepository;
        _cacheManager = cacheManager;
    }
    
    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        // 使用带标签的缓存
        return await _productRepository.AsQueryable()
            .Where(p => p.CategoryId == categoryId)
            .WithCache()
            .WithTags($"category:{categoryId}") // 添加缓存标签
            .ToListAsync();
    }
    
    public async Task UpdateProductAsync(Product product)
    {
        await _productRepository.UpdateAsync(product);
        
        // 清除相关缓存
        await _cacheManager.InvalidateTagAsync($"category:{product.CategoryId}");
    }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/dbcaching)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/DbCaching)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。