# AiUo FluentValidation 基于特性的验证框架

## 概述

本框架提供了基于特性（Attribute）的 FluentValidation 深度封装实现，无需继承 `AbstractValidator` 类，通过在模型属性上添加验证特性即可实现强大的验证功能。

## 特性

- ✅ **基于特性的验证** - 无需创建单独的验证器类
- ✅ **深度封装** - 完全隐藏 FluentValidation 的复杂性
- ✅ **自动验证** - 通过 ActionFilter 自动验证请求模型
- ✅ **手动验证** - 支持手动调用验证方法
- ✅ **丰富的验证规则** - 内置常用验证特性
- ✅ **自定义验证** - 支持自定义验证逻辑
- ✅ **条件验证** - 支持基于条件的验证
- ✅ **错误码支持** - 每个验证规则可指定错误码
- ✅ **批量验证** - 支持批量验证多个对象

## 快速开始

### 1. 注册服务

在 `Program.cs` 或 `Startup.cs` 中注册服务：

```csharp
// 注册基于特性的 FluentValidation
builder.Services.AddAttributeBasedFluentValidation();

// 或者指定要扫描的程序集
builder.Services.AddAttributeBasedFluentValidation(
    Assembly.GetExecutingAssembly(),
    Assembly.GetAssembly(typeof(SomeOtherClass))
);
```

### 2. 创建验证模型

```csharp
public class UserModel
{
    [FluentRequired("USER_NAME_REQUIRED", "用户名不能为空")]
    [FluentLength(3, 20, "USER_NAME_LENGTH", "用户名长度必须在3-20个字符之间")]
    public string UserName { get; set; }

    [FluentRequired("EMAIL_REQUIRED", "邮箱不能为空")]
    [FluentEmail("EMAIL_FORMAT", "邮箱格式不正确")]
    public string Email { get; set; }

    [FluentRequired("PASSWORD_REQUIRED", "密码不能为空")]
    [FluentMinLength(6, "PASSWORD_MIN_LENGTH", "密码最少6个字符")]
    public string Password { get; set; }

    [FluentRange(18, 100, "AGE_RANGE", "年龄必须在18-100之间")]
    public int Age { get; set; }
}
```

### 3. 在控制器中使用

#### 自动验证（推荐）

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserModel model)
    {
        // 验证会自动进行，如果失败会自动返回 BadRequest
        // 这里直接处理业务逻辑即可
        
        return Ok(new { Message = "注册成功" });
    }
}
```

#### 手动验证

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginModel model)
{
    // 手动验证
    var validationResult = await model.ValidateModelAsync();
    if (!validationResult.IsValid)
    {
        return BadRequest(new
        {
            Message = "验证失败",
            Errors = validationResult.ToDictionary()
        });
    }

    // 处理业务逻辑
    return Ok();
}
```

## 内置验证特性

### 基础验证

| 特性 | 说明 | 示例 |
|------|------|------|
| `FluentRequired` | 必填验证 | `[FluentRequired("CODE", "不能为空")]` |
| `FluentMinLength` | 最小长度 | `[FluentMinLength(6, "CODE", "最少6个字符")]` |
| `FluentMaxLength` | 最大长度 | `[FluentMaxLength(20, "CODE", "最多20个字符")]` |
| `FluentLength` | 长度范围 | `[FluentLength(6, 20, "CODE", "长度6-20字符")]` |
| `FluentRange` | 数值范围 | `[FluentRange(18, 100, "CODE", "年龄18-100")]` |

### 格式验证

| 特性 | 说明 | 示例 |
|------|------|------|
| `FluentEmail` | 邮箱格式 | `[FluentEmail("CODE", "邮箱格式不正确")]` |
| `FluentRegularExpression` | 正则表达式 | `[FluentRegularExpression(@"^\d+$", "CODE", "只能是数字")]` |

### 比较验证

| 特性 | 说明 | 示例 |
|------|------|------|
| `FluentCompare` | 属性比较 | `[FluentCompare("Password", "CODE", "密码不一致")]` |

### 高级验证

| 特性 | 说明 | 示例 |
|------|------|------|
| `FluentCustom` | 自定义验证 | `[FluentCustom(obj => obj != null, "CODE", "自定义验证失败")]` |
| `FluentWhen` | 条件验证 | `[FluentWhen(obj => condition, innerAttribute)]` |

## 高级用法

### 1. 自定义验证

```csharp
public class OrderModel
{
    public DateTime StartDate { get; set; }
    
    [FluentCustom(model => 
    {
        var orderModel = model as OrderModel;
        return orderModel?.EndDate > orderModel?.StartDate;
    }, "END_DATE_INVALID", "结束日期必须大于开始日期")]
    public DateTime EndDate { get; set; }
}
```

### 2. 条件验证

```csharp
public class UpdateUserModel
{
    public string NewPassword { get; set; }
    
    // 只有当 NewPassword 不为空时才验证 OldPassword
    [FluentWhen(model => !string.IsNullOrEmpty(((UpdateUserModel)model).NewPassword),
        new FluentRequired("OLD_PASSWORD_REQUIRED", "修改密码时必须提供旧密码"))]
    public string OldPassword { get; set; }
}
```

### 3. 批量验证

```csharp
[HttpPost("batch-create")]
public async Task<IActionResult> BatchCreate([FromBody] List<UserModel> models)
{
    var errors = new List<object>();
    
    for (int i = 0; i < models.Count; i++)
    {
        var result = await models[i].ValidateModelAsync();
        if (!result.IsValid)
        {
            errors.Add(new { Index = i, Errors = result.ToDictionary() });
        }
    }
    
    if (errors.Any())
    {
        return BadRequest(new { ValidationErrors = errors });
    }
    
    // 处理业务逻辑
    return Ok();
}
```

### 4. 验证组

```csharp
[FluentValidationGroup(ValidationGroups.Create, ValidationGroups.Update)]
public class ProductModel
{
    // 验证规则...
}
```

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

### 手动验证结果

```csharp
var result = await model.ValidateModelAsync();

// 检查是否验证通过
if (result.IsValid) { /* 验证通过 */ }

// 获取第一个错误
var firstError = result.FirstErrorMessage;

// 获取指定属性的错误
var emailErrors = result.GetErrorsForProperty("Email");

// 转换为字典格式
var errorDict = result.ToDictionary();
```

## 扩展自定义验证特性

```csharp
public class FluentPhoneAttribute : FluentValidationAttribute
{
    public FluentPhoneAttribute(string code = null, string message = "手机号格式不正确")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Matches(@"^1[3-9]\d{9}$")
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}手机号格式不正确") 
                as IRuleBuilderOptions<T, TProperty>;
        }
        
        throw new InvalidOperationException("FluentPhoneAttribute只能应用于string类型的属性");
    }
}
```

## 性能优化

1. **验证器缓存** - 验证器会自动缓存，避免重复创建
2. **异步验证** - 支持异步验证，提高性能
3. **条件验证** - 只在满足条件时执行验证，减少不必要的计算

## 注意事项

1. 确保在 `Program.cs` 中正确注册服务
2. 验证特性的顺序会影响验证执行顺序，可通过 `Order` 属性控制
3. 自定义验证逻辑应该尽量简单，避免复杂的业务逻辑
4. 错误码建议使用常量定义，便于维护

## 示例项目

完整的示例代码请参考：
- `Examples/UserModel.cs` - 模型示例
- `Examples/UserController.cs` - 控制器示例

## 与传统方式对比

### 传统 FluentValidation 方式

```csharp
// 需要创建单独的验证器类
public class UserModelValidator : AbstractValidator<UserModel>
{
    public UserModelValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空")
            .Length(3, 20).WithMessage("用户名长度必须在3-20个字符之间");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确");
    }
}

// 需要注册验证器
services.AddScoped<IValidator<UserModel>, UserModelValidator>();
```

### 本框架方式

```csharp
// 直接在模型上添加特性即可
public class UserModel
{
    [FluentRequired("USER_NAME_REQUIRED", "用户名不能为空")]
    [FluentLength(3, 20, "USER_NAME_LENGTH", "用户名长度必须在3-20个字符之间")]
    public string UserName { get; set; }

    [FluentRequired("EMAIL_REQUIRED", "邮箱不能为空")]
    [FluentEmail("EMAIL_FORMAT", "邮箱格式不正确")]
    public string Email { get; set; }
}

// 只需要一次性注册服务
services.AddAttributeBasedFluentValidation();
```

**优势：**
- 🎯 **更简洁** - 无需创建额外的验证器类
- 🎯 **更直观** - 验证规则直接在属性上定义
- 🎯 **更易维护** - 模型和验证规则在同一个地方
- 🎯 **更少代码** - 减少样板代码
- 🎯 **自动发现** - 无需手动注册每个验证器