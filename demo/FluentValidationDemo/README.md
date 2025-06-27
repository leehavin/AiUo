# FluentValidation Demo 演示项目

这是一个展示 AiUo 基于特性的 FluentValidation 框架使用方法的演示项目。

## 项目特点

- ✅ 展示基于特性的验证实现
- ✅ 包含多种验证场景示例
- ✅ 提供完整的 API 接口测试
- ✅ 集成 Swagger UI 便于测试
- ✅ 包含自动验证和手动验证示例

## 快速开始

### 1. 运行项目

```bash
cd demo/FluentValidationDemo
dotnet run
```

### 2. 访问 Swagger UI

项目启动后，访问：http://localhost:5000

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

### 7. 获取验证规则

**接口：** `GET /api/demo/validation-rules`

**功能：** 获取验证规则信息，用于前端动态验证

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