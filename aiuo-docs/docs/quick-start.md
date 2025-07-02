---
sidebar_position: 2
---

# 快速开始

本指南将帮助您在几分钟内开始使用 AiUo 框架。

## 📋 系统要求

- **.NET 8.0** 或更高版本
- **Visual Studio 2022** 或 **VS Code**
- **MySQL 8.0+** 或其他支持的数据库
- **Redis 6.0+**（可选，用于缓存）

## 🚀 创建新项目

### 1. 创建 ASP.NET Core 项目

```bash
# 创建新的 Web API 项目
dotnet new webapi -n MyAiUoApp
cd MyAiUoApp
```

### 2. 安装 AiUo 包

```bash
# 安装核心包
dotnet add package AiUo
dotnet add package AiUo.AspNet.Hosting

# 安装数据访问包
dotnet add package AiUo.Data.SqlSugar

# 安装扩展包（可选）
dotnet add package AiUo.Extensions.StackExchangeRedis
dotnet add package AiUo.Extensions.Serilog
```

## ⚙️ 配置应用程序

### 1. 更新 `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AiUo": {
    "Database": {
      "ConnectionString": "Server=localhost;Database=myapp;Uid=root;Pwd=password;",
      "DbType": "MySql"
    },
    "Redis": {
      "ConnectionString": "localhost:6379"
    },
    "Jwt": {
      "SecretKey": "your-secret-key-here-must-be-at-least-32-characters",
      "Issuer": "MyAiUoApp",
      "Audience": "MyAiUoApp",
      "ExpireMinutes": 60
    }
  }
}
```

### 2. 配置 `Program.cs`

```csharp
using AiUo.AspNet.Hosting;
using AiUo.Data.SqlSugar;
using AiUo.Extensions.StackExchangeRedis;
using AiUo.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);

// 添加 AiUo 服务
builder.Services.AddAiUo(builder.Configuration, options =>
{
    // 配置数据库
    options.UseSqlSugar();
    
    // 配置 Redis 缓存
    options.UseRedis();
    
    // 配置日志
    options.UseSerilog();
    
    // 配置 JWT 认证
    options.UseJwtAuthentication();
});

// 添加控制器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 配置请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用 AiUo 中间件
app.UseAiUo();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 📝 创建第一个 API

### 1. 创建数据模型

创建 `Models/User.cs`：

```csharp
using SqlSugar;

namespace MyAiUoApp.Models
{
    [SugarTable("users")]
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        
        [SugarColumn(Length = 50, IsNullable = false)]
        public string Name { get; set; } = string.Empty;
        
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Email { get; set; } = string.Empty;
        
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
```

### 2. 创建服务层

创建 `Services/IUserService.cs`：

```csharp
using MyAiUoApp.Models;

namespace MyAiUoApp.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }
}
```

创建 `Services/UserService.cs`：

```csharp
using AiUo.Data.SqlSugar;
using MyAiUoApp.Models;
using SqlSugar;

namespace MyAiUoApp.Services
{
    public class UserService : IUserService
    {
        private readonly ISqlSugarClient _db;
        
        public UserService(ISqlSugarClient db)
        {
            _db = db;
        }
        
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _db.Queryable<User>().ToListAsync();
        }
        
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _db.Queryable<User>().FirstAsync(x => x.Id == id);
        }
        
        public async Task<User> CreateUserAsync(User user)
        {
            var result = await _db.Insertable(user).ExecuteReturnEntityAsync();
            return result;
        }
        
        public async Task<bool> UpdateUserAsync(User user)
        {
            return await _db.Updateable(user).ExecuteCommandHasChangeAsync();
        }
        
        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _db.Deleteable<User>().Where(x => x.Id == id).ExecuteCommandHasChangeAsync();
        }
    }
}
```

### 3. 创建控制器

创建 `Controllers/UsersController.cs`：

```csharp
using Microsoft.AspNetCore.Mvc;
using MyAiUoApp.Models;
using MyAiUoApp.Services;

namespace MyAiUoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            
            var updated = await _userService.UpdateUserAsync(user);
            if (!updated)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            
            return NoContent();
        }
    }
}
```

### 4. 注册服务

在 `Program.cs` 中添加服务注册：

```csharp
// 在 builder.Services.AddControllers(); 之前添加
builder.Services.AddScoped<IUserService, UserService>();
```

## 🗄️ 数据库初始化

### 1. 创建数据库迁移

创建 `DbInitializer.cs`：

```csharp
using AiUo.Data.SqlSugar;
using MyAiUoApp.Models;
using SqlSugar;

namespace MyAiUoApp
{
    public static class DbInitializer
    {
        public static void Initialize(ISqlSugarClient db)
        {
            // 创建表
            db.CodeFirst.InitTables<User>();
            
            // 插入测试数据
            if (!db.Queryable<User>().Any())
            {
                var users = new List<User>
                {
                    new User { Name = "张三", Email = "zhangsan@example.com" },
                    new User { Name = "李四", Email = "lisi@example.com" },
                    new User { Name = "王五", Email = "wangwu@example.com" }
                };
                
                db.Insertable(users).ExecuteCommand();
            }
        }
    }
}
```

### 2. 在应用启动时初始化数据库

在 `Program.cs` 中添加：

```csharp
// 在 app.Run(); 之前添加
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
    DbInitializer.Initialize(db);
}
```

## 🏃‍♂️ 运行应用程序

```bash
# 运行应用程序
dotnet run
```

应用程序将在 `https://localhost:5001` 启动。访问 `https://localhost:5001/swagger` 查看 API 文档。

## 🧪 测试 API

使用 curl 或 Postman 测试您的 API：

```bash
# 获取所有用户
curl -X GET https://localhost:5001/api/users

# 创建新用户
curl -X POST https://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"新用户","email":"newuser@example.com"}'
```

## 🎉 恭喜！

您已经成功创建了第一个 AiUo 应用程序！接下来您可以：

- 查看 [核心组件文档](./core/overview) 了解更多功能
- 学习 [最佳实践](./guides/best-practices)
- 探索 [示例项目](./examples/basic)
- 了解 [部署指南](./deployment/docker)

## 🔗 相关链接

- [配置指南](./configuration/overview)
- [数据访问](./data-access/sqlsugar)
- [缓存策略](./caching/redis)
- [认证授权](./security/jwt)