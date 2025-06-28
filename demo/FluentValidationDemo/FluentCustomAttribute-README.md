# FluentCustomAttribute 使用指南

`FluentCustomAttribute` 是 AiUo FluentValidation 框架中的一个强大特性，允许开发者定义完全自定义的验证逻辑。

## 基本语法

```csharp
[FluentCustom(validationMethodName, code, message)]
public string PropertyName { get; set; }
```

### 参数说明

- **validationMethodName**: `string` - 验证方法名称，方法必须是静态方法，签名为 `static bool MethodName(object value)`
- **code**: `string` - 错误代码（可选），用于国际化或错误分类
- **message**: `string` - 错误消息（可选），验证失败时显示的消息

### 重要说明

由于 C# 特性参数只能是编译时常量，`FluentCustomAttribute` 使用方法名字符串来指定验证逻辑，而不是直接传递委托。验证方法必须满足以下要求：

1. **静态方法**: 必须是 `static` 方法
2. **方法签名**: `static bool MethodName(object value)`
3. **返回类型**: 必须返回 `bool` 类型
4. **访问修饰符**: 可以是 `public` 或 `private`

## 使用示例

### 1. 基础自定义验证

```csharp
public class UserModel
{
    /// <summary>
    /// 用户名 - 不能包含敏感词
    /// </summary>
    [FluentRequired("USER_NAME_REQUIRED", "用户名不能为空")]
    [FluentCustom(nameof(ValidateUserName), "USER_NAME_SENSITIVE", "用户名不能包含敏感词汇")]
    public string UserName { get; set; }

    /// <summary>
    /// 验证用户名是否包含敏感词
    /// </summary>
    private static bool ValidateUserName(object value)
    {
        if (value is not string userName || string.IsNullOrEmpty(userName))
            return true; // 空值由其他验证器处理

        var sensitiveWords = new[] { "admin", "root", "test", "guest" };
        return !sensitiveWords.Any(word => userName.ToLower().Contains(word));
    }
}
```

### 2. 使用方法名引用

```csharp
public class ProductModel
{
    /// <summary>
    /// 产品代码 - 使用方法名引用验证
    /// </summary>
    [FluentCustom(nameof(ValidateProductCodeWrapper), 
                  "PRODUCT_CODE_FORMAT", 
                  "产品代码格式不正确：必须是PRD-开头，后跟8位数字")]
    public string ProductCode { get; set; }

    /// <summary>
    /// 验证方法包装器 - 必须符合 static bool MethodName(object value) 签名
    /// </summary>
    private static bool ValidateProductCodeWrapper(object value)
    {
        return ValidateProductCode(value as string);
    }

    private static bool ValidateProductCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return true;
        return code.StartsWith("PRD-") && 
               code.Length == 12 && 
               code.Substring(4).All(char.IsDigit);
    }
}
```

### 3. 复杂业务逻辑验证

```csharp
public class OrderModel
{
    /// <summary>
    /// 订单金额 - 复杂业务验证
    /// </summary>
    [FluentCustom(nameof(ValidateOrderAmount), 
                  "ORDER_AMOUNT_BUSINESS", 
                  "订单金额不符合业务规则")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 年龄 - 必须是偶数
    /// </summary>
    [FluentCustom(nameof(ValidateEvenAge), "AGE_MUST_BE_EVEN", "年龄必须是偶数")]
    public int Age { get; set; }

    /// <summary>
    /// 邮箱 - 不能是临时邮箱
    /// </summary>
    [FluentEmail("EMAIL_FORMAT", "邮箱格式不正确")]
    [FluentCustom(nameof(ValidateEmailDomain), "EMAIL_TEMP_DOMAIN", "不允许使用临时邮箱域名")]
    public string Email { get; set; }

    public string CustomerType { get; set; }
    public bool IsVip { get; set; }

    private static bool ValidateOrderAmount(object value)
    {
        if (value is not decimal amount) return false;
        
        // 复杂的业务逻辑验证
        if (amount <= 0) return false;
        if (amount > 100000) return false; // 单笔订单不能超过10万
        
        return true;
    }

    private static bool ValidateEvenAge(object value)
    {
        return value is int age && age % 2 == 0;
    }

    private static bool ValidateEmailDomain(object value)
    {
        if (value is not string email || string.IsNullOrEmpty(email))
            return true;

        var tempDomains = new[] { "10minutemail.com", "tempmail.org", "guerrillamail.com" };
        var domain = email.Split('@').LastOrDefault()?.ToLower();
        return domain != null && !tempDomains.Contains(domain);
    }
}
```

## 最佳实践

### 1. 验证方法设计原则

- **方法签名**: 必须是 `static bool MethodName(object value)` 格式
- **空值处理**: 通常应该返回 `true`，让其他验证器（如 `FluentRequired`）处理空值
- **类型安全**: 使用 `is` 操作符进行类型检查和转换
- **性能考虑**: 避免在验证方法中进行耗时操作
- **方法命名**: 使用 `nameof()` 操作符引用方法名，避免硬编码字符串

```csharp
private static bool ValidateExample(object value)
{
    // 1. 处理空值
    if (value is not string stringValue || string.IsNullOrEmpty(stringValue))
        return true;

    // 2. 执行验证逻辑
    return stringValue.Length <= 100;
}
```

### 2. 错误代码和消息

- **错误代码**: 使用有意义的常量，便于国际化和错误处理
- **错误消息**: 提供清晰、用户友好的错误描述

```csharp
// 推荐
[FluentCustom(nameof(ValidateUserName), "USER_NAME_SENSITIVE", "用户名不能包含敏感词汇")]

// 不推荐
[FluentCustom(nameof(ValidateUserName), "ERR001", "Invalid")]
```

### 3. 组合使用

`FluentCustom` 可以与其他验证特性组合使用：

```csharp
[FluentRequired("FIELD_REQUIRED", "字段不能为空")]
[FluentLength(3, 50, "FIELD_LENGTH", "字段长度必须在3-50个字符之间")]
[FluentCustom(nameof(ValidateCustomLogic), "FIELD_CUSTOM", "字段不符合业务规则")]
public string CustomField { get; set; }

private static bool ValidateCustomLogic(object value)
{
    // 自定义业务逻辑
    return value is string str && !str.Contains("forbidden");
}
```

## API 测试示例

### 测试端点

- `POST /api/CustomValidation/basic-custom-validation` - 基础自定义验证
- `POST /api/CustomValidation/advanced-custom-validation` - 高级自定义验证
- `POST /api/CustomValidation/manual-validation` - 手动验证示例
- `GET /api/CustomValidation/examples` - 获取示例数据

### 测试数据

#### 有效数据
```json
{
  "userName": "john_doe",
  "password": "MyPassword123!",
  "age": 24,
  "email": "john@example.com"
}
```

#### 无效数据
```json
{
  "userName": "admin_user",     // 包含敏感词
  "password": "MyPassword123",  // 缺少特殊字符
  "age": 25,                    // 不是偶数
  "email": "test@10minutemail.com" // 临时邮箱
}
```

## 常见使用场景

1. **业务规则验证**: 复杂的业务逻辑验证
2. **格式验证**: 自定义格式要求（如特殊编码规则）
3. **黑名单检查**: 敏感词、禁用域名等
4. **数据一致性**: 跨字段的复杂验证逻辑
5. **外部依赖验证**: 需要调用外部服务的验证（注意性能）

## 注意事项

1. **性能**: 避免在验证函数中执行耗时操作
2. **异常处理**: 验证函数应该捕获异常并返回适当的布尔值
3. **线程安全**: 确保验证函数是线程安全的
4. **测试**: 为自定义验证逻辑编写单元测试

## 总结

`FluentCustomAttribute` 提供了极大的灵活性，允许开发者实现任何复杂的验证逻辑。通过合理使用这个特性，可以确保应用程序的数据完整性和业务规则的正确执行。