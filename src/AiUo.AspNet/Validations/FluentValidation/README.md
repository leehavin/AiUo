# FluentValidation 组件重构说明

## 目录结构

本次重构按照微软命名规范重新组织了 FluentValidation 相关代码，将原本单一文件中的多个类分离到不同的目录中：

```
FluentValidation/
├── Attributes/           # 验证特性
│   ├── FluentValidationAttribute.cs      # 基础验证特性
│   └── ValidationAttributes.cs           # 具体验证特性实现
├── Extensions/           # 扩展方法
│   └── FluentValidationExtensions.cs     # 服务注册扩展
├── Models/              # 数据模型
│   ├── ComparisonType.cs                 # 比较类型枚举
│   ├── ValidationError.cs                # 验证错误模型
│   ├── ValidationGroups.cs               # 验证分组常量
│   └── ValidationResult.cs               # 验证结果模型
├── Rules/               # 规则扩展
│   └── FluentValidationRuleExtensions.cs # 验证规则扩展方法
├── Services/            # 服务接口和实现
│   ├── AttributeBasedValidatorFactory.cs # 验证器工厂实现
│   ├── FluentValidationOptions.cs        # 配置选项
│   ├── IAttributeBasedValidatorFactory.cs # 验证器工厂接口
│   ├── IValidationService.cs              # 验证服务接口
│   └── ValidationService.cs               # 验证服务实现
├── Validators/          # 验证器
│   └── AttributeBasedValidator.cs        # 基于特性的验证器
├── GlobalUsings.cs      # 全局命名空间导入
└── README.md           # 说明文档
```

## 主要改进

### 1. 符合微软命名规范
- **Attributes**: 存放所有验证特性类
- **Extensions**: 存放扩展方法
- **Models**: 存放数据模型和枚举
- **Rules**: 存放验证规则扩展
- **Services**: 存放服务接口和实现
- **Validators**: 存放验证器类

### 2. 清晰的职责分离
- 每个目录都有明确的职责范围
- 类之间的依赖关系更加清晰
- 便于单元测试和维护

### 3. 更好的可扩展性
- 新增验证特性只需在 Attributes 目录添加
- 新增验证规则只需在 Rules 目录添加
- 新增服务实现只需在 Services 目录添加

### 4. 统一的依赖注入模式
- 所有服务使用一致的构造函数注入
- 支持服务生命周期管理
- 便于单元测试和模拟

### 5. 完善的错误处理
- 详细的异常处理和日志记录
- 友好的错误信息提示
- 支持多语言错误消息

### 6. 性能监控支持
- 可配置的验证性能监控
- 验证耗时统计和分析
- 支持性能阈值告警

### 7. 智能缓存管理
- 自动清理验证器缓存
- 内存使用优化
- 支持缓存策略配置

### 8. 配置验证
- 启动时自动验证配置参数
- 配置项完整性检查
- 运行时配置热更新支持

## 使用方法

### 1. 服务注册

```csharp
using AiUo.AspNet.Validations.FluentValidation.Extensions;

// 在 Program.cs 或 Startup.cs 中
services.AddFluentValidationWithAttributes();

// 或者自定义配置
services.AddFluentValidationWithAttributes(options =>
{
    options.DefaultErrorCode = "CUSTOM_ERROR";
    options.StopOnFirstFailure = true;
    options.MaxErrors = 50;
    
    // 启用性能监控
    options.EnablePerformanceMonitoring = true;
    options.PerformanceThresholdMs = 500; // 500ms阈值
});
```

### 2. 使用验证特性

```csharp
using AiUo.AspNet.Validations.FluentValidation.Attributes;

public class UserModel
{
    [FluentRequired(message: "用户名不能为空")]
    [FluentStringLength(2, 50, message: "用户名长度必须在2-50个字符之间")]
    public string Username { get; set; }

    [FluentRequired(message: "邮箱不能为空")]
    [FluentEmail(message: "邮箱格式不正确")]
    public string Email { get; set; }

    [FluentChineseIdCard(message: "身份证号格式不正确")]
    public string IdCard { get; set; }

    [FluentChineseMobile(message: "手机号格式不正确")]
    public string Mobile { get; set; }
}
```

### 3. 使用验证服务

```csharp
using AiUo.AspNet.Validations.FluentValidation.Services;

public class UserController : ControllerBase
{
    private readonly IValidationService _validationService;

    public UserController(IValidationService validationService)
    {
        _validationService = validationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(UserModel model)
    {
        var result = await _validationService.ValidateAsync(model);
        if (!result.IsValid)
        {
            return BadRequest(result.ToDictionary());
        }

        // 处理业务逻辑
        return Ok();
    }
}
```

### 4. 使用规则扩展

```csharp
using AiUo.AspNet.Validations.FluentValidation.Rules;

public class UserValidator : AbstractValidator<UserModel>
{
    public UserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(2, 50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, cancellation) => 
            {
                // 自定义异步验证逻辑
                return await IsEmailUniqueAsync(email);
            })
            .WithMessage("邮箱地址已存在");

        RuleFor(x => x.IdCard)
            .MustBeValidChineseIdCard()
            .When(x => !string.IsNullOrEmpty(x.IdCard));

        RuleFor(x => x.Mobile)
            .MustBeValidChineseMobile()
            .When(x => !string.IsNullOrEmpty(x.Mobile));
    }
}
```

### 5. 性能监控

```csharp
// 启用性能监控后，验证耗时会自动记录到日志
// 超过阈值的验证会产生警告日志
var result = await validationService.ValidateAsync(model);

// 日志示例：
// [Debug] Validation for UserModel completed in 45ms
// [Warning] Validation for ComplexModel took 1200ms, which exceeds threshold of 500ms
```

### 6. 错误处理

```csharp
try
{
    var result = await validationService.ValidateAsync(model);
    if (!result.IsValid)
    {
        // 处理验证错误
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
        }
    }
}
catch (ValidationException ex)
{
    // 处理验证异常
    logger.LogError(ex, "Validation failed");
}
```

## 迁移指南

如果你之前使用的是旧版本的 FluentValidation 组件，需要更新命名空间引用：

### 旧的引用方式
```csharp
using AiUo.AspNet.Validations.FluentValidation;
```

### 新的引用方式
```csharp
// 可以使用具体的命名空间
using AiUo.AspNet.Validations.FluentValidation.Attributes;
using AiUo.AspNet.Validations.FluentValidation.Services;
using AiUo.AspNet.Validations.FluentValidation.Extensions;

// 或者使用 GlobalUsings.cs 中定义的全局引用
```

## 注意事项

1. 确保项目中引用了 FluentValidation NuGet 包
2. 如果使用自定义验证器，需要继承 `AttributeBasedValidator<T>` 类
3. 验证特性的 Order 属性可以控制验证顺序
4. 可以通过 `FluentValidationOptions` 配置全局验证行为

## 扩展开发

### 添加新的验证特性

1. 在 `Attributes` 目录创建新的特性类
2. 继承 `FluentValidationAttribute` 基类
3. 实现 `ApplyRule` 方法

### 添加新的验证规则

1. 在 `Rules` 目录的 `FluentValidationRuleExtensions` 类中添加扩展方法
2. 使用 FluentValidation 的标准 API 实现验证逻辑

### 添加新的服务

1. 在 `Services` 目录定义接口和实现
2. 在 `Extensions` 目录的扩展方法中注册服务