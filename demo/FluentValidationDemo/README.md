# FluentValidation Demo 演示项目

这是一个展示 AiUo 基于特性的 FluentValidation 框架使用方法的演示项目。

## 项目特点

- **基于特性的验证**: 使用 FluentValidation 特性进行模型验证
- **统一依赖注入**: 采用新的 `AddFluentValidationWithAttributes()` 注册方式
- **性能监控**: 内置验证性能监控，支持性能阈值配置
- **智能缓存**: 验证器缓存管理，提升验证性能
- **完善错误处理**: 详细的错误信息和异常处理机制
- **配置验证**: 启动时自动验证 FluentValidation 配置
- **自动验证**: 支持 ASP.NET Core 模型绑定时的自动验证
- **手动验证**: 提供手动验证 API 用于复杂场景
- **批量验证**: 支持批量数据验证和性能统计
- **多种验证规则**: 包含常用的验证规则示例
- **统一响应格式**: 标准化的错误响应格式
- **实际应用场景**: 涵盖用户注册、产品管理、订单处理等常见业务场景

## 快速开始

### 1. 运行项目

```bash
cd demo/FluentValidationDemo
dotnet run
```

### 2. 访问 Swagger UI

项目启动后，访问：http://localhost:5000

## 新功能配置

### FluentValidation 配置

项目已升级使用新的配置方式，在 `Program.cs` 中：

```csharp
builder.Services.AddFluentValidationWithAttributes(options =>
{
    options.DefaultErrorCode = "G_VALIDATION_ERROR";
    options.StopOnFirstFailure = false;
    options.MaxErrors = 10;
    options.EnablePerformanceMonitoring = true;
    options.PerformanceThresholdMs = 500;
});
```

### 配置选项说明

- **DefaultErrorCode**: 默认错误码，用于统一错误响应
- **StopOnFirstFailure**: 是否在第一个验证失败时停止
- **MaxErrors**: 最大错误数量限制
- **EnablePerformanceMonitoring**: 启用性能监控
- **PerformanceThresholdMs**: 性能阈值（毫秒），超过此值会记录警告日志

### 性能监控

新版本内置性能监控功能：
- 自动记录验证耗时
- 超过阈值时输出警告日志
- 支持性能统计和分析
- 在响应中包含验证时间信息

### 缓存管理

验证器缓存自动管理：
- 智能缓存验证器实例
- 定期清理缓存（每30分钟）
- 缓存大小限制（1000个实例）
- 自动资源释放

## API 接口说明

### 1. 用户注册 - 自动验证演示

**接口：** `POST /api/demo/register`

**功能：** 展示自动验证功能，验证失败时自动返回错误信息

**测试数据：**
```json
{
  "userName": "testuser",
  "email": "test@example.com",
  "password": "Test123",
  "confirmPassword": "Test123",
  "age": 25,
  "phone": "13800138000"
}
```

**验证规则：**
- 用户名：必填，3-20字符，只能包含字母数字下划线
- 邮箱：必填，邮箱格式
- 密码：必填，最少6字符，必须包含大小写字母和数字
- 确认密码：必填，必须与密码一致
- 年龄：18-100之间
- 手机号：可选，但如果填写必须符合格式

### 2. 用户登录 - 手动验证演示

**接口：** `POST /api/demo/login`

**功能：** 展示手动验证功能

**测试数据：**
```json
{
  "loginName": "admin",
  "password": "Admin123",
  "rememberMe": true
}
```

### 3. 创建产品 - 数值验证演示

**接口：** `POST /api/demo/products`

**功能：** 展示数值范围验证

**测试数据：**
```json
{
  "name": "测试产品",
  "description": "这是一个测试产品",
  "price": 99.99,
  "stock": 100,
  "categoryId": 1
}
```

### 4. 创建订单 - 复杂验证演示

**接口：** `POST /api/demo/orders`

**功能：** 展示自定义验证逻辑

**测试数据：**
```json
{
  "orderNo": "ORD1234567890",
  "customerEmail": "customer@example.com",
  "amount": 299.99,
  "orderDate": "2024-01-15T10:30:00",
  "expectedShipDate": "2024-01-16T10:30:00",
  "remark": "测试订单"
}
```

### 5. 文件上传 - 文件验证演示

**接口：** `POST /api/demo/upload`

**功能：** 展示文件相关验证

**测试数据：**
```json
{
  "fileName": "test.jpg",
  "fileSize": 1048576,
  "fileType": "image",
  "fileExtension": ".jpg"
}
```

### 6. 批量注册 - 批量验证演示

**接口：** `POST /api/demo/batch-register`

**功能：** 展示批量验证功能

**测试数据：**
```json
[
  {
    "userName": "user1",
    "email": "user1@example.com",
    "password": "User123",
    "confirmPassword": "User123",
    "age": 25
  },
  {
    "userName": "user2",
    "email": "user2@example.com",
    "password": "User456",
    "confirmPassword": "User456",
    "age": 30
  }
]
```

### 7. 验证服务性能测试 🆕

**接口：** `POST /api/demo/performance-test`

**功能：** 演示新验证服务的性能监控功能

**特性：** 包含验证时间统计、详细错误信息、异常处理

**测试数据：**
```json
{
  "userName": "advanceduser",
  "email": "user@example.com",
  "age": 25,
  "userType": "Premium",
  "tags": ["developer", "tester"]
}
```

**响应示例：**
```json
{
  "code": "G_SUCCESS",
  "message": "验证成功",
  "validationTime": "15ms",
  "data": {
    "userName": "advanceduser",
    "email": "user@example.com",
    "userType": "Premium",
    "tagCount": 2,
    "processedAt": "2024-01-15T10:30:00"
  }
}
```

### 8. 批量验证演示 🆕

**接口：** `POST /api/demo/batch-validation`

**功能：** 演示验证服务的批量处理能力

**特性：** 批量验证、性能统计、详细结果报告

**测试数据：**
```json
[
  {
    "userName": "user1",
    "email": "user1@example.com",
    "password": "password123",
    "confirmPassword": "password123",
    "age": 25
  },
  {
    "userName": "user2",
    "email": "invalid-email",
    "password": "123",
    "confirmPassword": "456",
    "age": 15
  }
]
```

**响应示例：**
```json
{
  "code": "G_SUCCESS",
  "message": "批量验证完成",
  "summary": {
    "totalCount": 2,
    "validCount": 1,
    "invalidCount": 1,
    "totalTime": "25ms",
    "averageTime": "12ms"
  },
  "results": [
    {
      "index": 0,
      "isValid": true,
      "validationTime": "10ms",
      "userName": "user1",
      "email": "user1@example.com",
      "errors": null
    },
    {
      "index": 1,
      "isValid": false,
      "validationTime": "15ms",
      "userName": "user2",
      "email": "invalid-email",
      "errors": [
        {
          "property": "Email",
          "message": "请输入有效的邮箱地址",
          "code": "G_INVALID_EMAIL"
        }
      ]
    }
  ]
}
```

### 9. 获取验证规则

**接口：** `GET /api/demo/validation-rules`

**功能：** 获取验证规则信息，用于前端动态验证

## GET 请求验证演示 🆕

### 1. 直接参数验证

**接口：** `GET /api/demo/user?id={id}&name={name}`

**功能：** 在方法参数上直接使用 FluentValidation 特性

**测试示例：**
```
# 有效请求
GET /api/demo/user?id=1&name=validuser
GET /api/demo/user?id=999
GET /api/demo/user?id=123&name=test123

# 无效请求
GET /api/demo/user?id=0&name=validuser     # id必须大于0
GET /api/demo/user?id=1&name=ab           # name长度必须在3-20个字符之间
```

### 2. 复杂查询参数验证

**接口：** `GET /api/demo/users/search`

**功能：** 多个参数的复杂验证，包括必填、长度、范围、正则表达式等

**参数说明：**
- `query` (必填): 搜索关键词，至少2个字符
- `page` (可选): 页码，必须大于0，默认为1
- `pageSize` (可选): 每页大小，1-100之间，默认为10
- `sortBy` (可选): 排序字段，只能是id、name、email或createTime

**测试示例：**
```
# 有效请求
GET /api/demo/users/search?query=test&page=1&pageSize=10&sortBy=id
GET /api/demo/users/search?query=admin&page=2&pageSize=20&sortBy=name

# 无效请求
GET /api/demo/users/search                              # 缺少必填的query参数
GET /api/demo/users/search?query=a                      # query长度至少2个字符
GET /api/demo/users/search?query=test&page=0            # page必须大于0
GET /api/demo/users/search?query=test&pageSize=101      # pageSize必须在1-100之间
GET /api/demo/users/search?query=test&sortBy=invalid    # sortBy只能是指定值
```

### 3. 路径参数验证

**接口：** `GET /api/demo/users/{userId}`

**功能：** 路径参数的验证

**参数说明：**
- `userId` (路径参数): 用户ID，必须大于0
- `includeProfile` (查询参数): 是否包含详细资料，布尔值

**测试示例：**
```
# 有效请求
GET /api/demo/users/1
GET /api/demo/users/999?includeProfile=true

# 无效请求
GET /api/demo/users/0     # userId必须大于0
GET /api/demo/users/-1    # userId必须大于0
```

### 4. 模型绑定验证

**接口：** `GET /api/demo/users`

**功能：** 使用 `[FromQuery]` 绑定到模型，自动进行验证

**查询参数：**
- `page`: 页码，必须大于0
- `pageSize`: 每页大小，1-100之间
- `keyword`: 搜索关键词，最多50个字符
- `minAge`: 最小年龄，0-150之间
- `maxAge`: 最大年龄，0-150之间
- `status`: 用户状态，只能是active、inactive或pending
- `sortBy`: 排序字段，只能是id、name、email、createTime或age
- `sortOrder`: 排序方向，只能是asc或desc
- `registerStartDate`: 注册开始日期，不能是未来时间
- `registerEndDate`: 注册结束日期，不能是未来时间

**测试示例：**
```
# 有效请求
GET /api/demo/users?page=1&pageSize=10
GET /api/demo/users?page=2&pageSize=20&keyword=test&minAge=18&maxAge=65
GET /api/demo/users?status=active&sortBy=name&sortOrder=desc
GET /api/demo/users?registerStartDate=2023-01-01&registerEndDate=2023-12-31

# 无效请求
GET /api/demo/users?page=0                                    # page必须大于0
GET /api/demo/users?pageSize=101                              # pageSize必须在1-100之间
GET /api/demo/users?keyword=verylongkeywordthatexceedslimit   # keyword最多50个字符
GET /api/demo/users?minAge=-1                                # minAge必须在0-150之间
GET /api/demo/users?status=unknown                           # status只能是指定值
GET /api/demo/users?registerStartDate=2030-01-01             # 不能是未来时间
```

### 5. 手动验证GET参数

**接口：** `GET /api/demo/validate-contact`

**功能：** 在方法内部手动创建模型并验证

**参数说明：**
- `email`: 邮箱地址，可选，但如果提供必须符合邮箱格式
- `phone`: 手机号，可选，但如果提供必须符合中国手机号格式

**测试示例：**
```
# 有效请求
GET /api/demo/validate-contact?email=test@example.com&phone=13812345678
GET /api/demo/validate-contact?email=user@domain.com
GET /api/demo/validate-contact?phone=13987654321

# 无效请求
GET /api/demo/validate-contact?email=invalid-email    # 邮箱格式不正确
GET /api/demo/validate-contact?phone=123              # 手机号格式不正确
```

## GET 请求验证方式总结

### 1. 直接参数验证
```csharp
[HttpGet("user")]
public async Task<IActionResult> GetUser(
    [FluentRange(1, int.MaxValue, "USER_ID_INVALID", "用户ID必须大于0")] int id,
    [FluentLength(3, 20, "USER_NAME_LENGTH", "用户名长度必须在3-20个字符之间")] string name = null)
{
    // 验证会自动进行
}
```

### 2. 模型绑定验证
```csharp
[HttpGet("users")]
public async Task<IActionResult> GetUsers([FromQuery] UserFilterModel filter)
{
    // 模型验证会自动进行
}
```

### 3. 手动验证
```csharp
[HttpGet("validate-contact")]
public async Task<IActionResult> ValidateContact(string email, string phone)
{
    var contactModel = new ContactValidationModel { Email = email, Phone = phone };
    var validationResult = await _validationService.ValidateAsync(contactModel);
    
    if (!validationResult.IsValid)
    {
        return BadRequest(validationResult.Errors);
    }
    
    return Ok();
}
```

### 验证方式选择建议

1. **直接参数验证**: 适用于简单的参数验证，参数较少的情况
2. **模型绑定验证**: 推荐方式，适用于复杂的查询条件，参数较多的情况
3. **手动验证**: 适用于需要动态创建验证模型或复杂验证逻辑的场景
4. **路径参数验证**: 对URL路径中的参数进行验证

### 注意事项

- GET请求的参数验证依赖于 `FluentValidationActionFilter`
- 确保在 `Program.cs` 中正确配置了 FluentValidation 服务
- 可选参数需要设置默认值或使用可空类型
- 复杂的验证逻辑建议使用模型绑定方式
- 手动验证适用于需要动态创建验证模型的场景

## 验证失败示例

### 1. 测试必填验证

```json
{
  "userName": "",
  "email": "",
  "password": "",
  "confirmPassword": "",
  "age": 0
}
```

**预期结果：** 返回多个验证错误

### 2. 测试格式验证

```json
{
  "userName": "test user!",
  "email": "invalid-email",
  "password": "123",
  "confirmPassword": "456",
  "age": 150,
  "phone": "123"
}
```

**预期结果：** 返回格式验证错误

### 3. 测试长度验证

```json
{
  "userName": "ab",
  "email": "test@example.com",
  "password": "Test123456789012345678901234567890",
  "confirmPassword": "Test123456789012345678901234567890",
  "age": 25
}
```

**预期结果：** 返回长度验证错误

## 错误响应格式

### 自动验证错误响应

```json
{
  "code": "G_BAD_REQUEST",
  "message": "验证失败",
  "errors": [
    {
      "field": "UserName",
      "message": "用户名不能为空",
      "code": "USER_NAME_REQUIRED"
    },
    {
      "field": "Email",
      "message": "邮箱格式不正确",
      "code": "EMAIL_FORMAT"
    }
  ]
}
```

### 手动验证错误响应

```json
{
  "code": "G_BAD_REQUEST",
  "message": "验证失败",
  "errors": {
    "UserName": ["用户名不能为空"],
    "Email": ["邮箱格式不正确"]
  },
  "firstError": "用户名不能为空"
}
```

## 测试建议

1. **使用 Swagger UI 测试**
   - 访问 http://localhost:5000
   - 逐个测试各个接口
   - 尝试不同的输入数据

2. **测试验证失败场景**
   - 提交空数据
   - 提交格式错误的数据
   - 提交超出范围的数据

3. **测试批量验证**
   - 提交部分正确、部分错误的数据
   - 观察错误信息的返回格式

4. **对比自动验证和手动验证**
   - 注册接口使用自动验证
   - 登录接口使用手动验证
   - 观察两种方式的差异

## 扩展示例

如果你想添加新的验证场景，可以：

1. 在 `Models/DemoModels.cs` 中添加新的模型
2. 在 `Controllers/DemoController.cs` 中添加新的接口
3. 使用各种验证特性组合

## 注意事项

1. 确保项目依赖正确安装
2. 确保 AiUo.AspNet 项目已正确引用
3. 如果遇到编译错误，请检查 FluentValidation 包版本
4. 建议在开发环境中运行，便于调试

## 相关文档

- [AiUo FluentValidation 框架文档](../src/AiUo.AspNet/Validations/FluentValidation/README.md)
- [FluentValidation 官方文档](https://docs.fluentvalidation.net/)
- [ASP.NET Core 验证文档](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation)