using Microsoft.AspNetCore.Mvc;
using AiUo.AspNet.Validations;
using FluentValidationDemo.Models;
using AiUo.Net;

namespace FluentValidationDemo.Controllers;

/// <summary>
/// FluentValidation 演示控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    /// <summary>
    /// 用户注册 - 自动验证演示
    /// </summary>
    /// <param name="model">注册模型</param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // 由于使用了FluentValidationActionFilter，验证会自动进行
        // 如果验证失败，会自动返回BadRequest响应
        
        // 模拟用户注册逻辑
        await Task.Delay(100);
        
        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "注册成功",
            Data = new 
            { 
                UserId = new Random().Next(1000, 9999),
                UserName = model.UserName,
                Email = model.Email,
                RegisterTime = DateTime.Now
            }
        });
    }

    /// <summary>
    /// 用户登录 - 手动验证演示
    /// </summary>
    /// <param name="model">登录模型</param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // 手动验证演示
        var validationResult = await model.ValidateModelAsync();
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Code = GResponseCodes.G_BAD_REQUEST,
                Message = "验证失败",
                Errors = validationResult.ToDictionary(),
                FirstError = validationResult.FirstErrorMessage
            });
        }

        // 模拟登录验证
        if (model.LoginName == "admin" && model.Password == "Admin123")
        {
            return Ok(new
            {
                Code = GResponseCodes.G_SUCCESS,
                Message = "登录成功",
                Data = new
                {
                    Token = Guid.NewGuid().ToString("N"),
                    ExpiresIn = 3600,
                    UserInfo = new
                    {
                        UserName = "admin",
                        Email = "admin@example.com",
                        LoginTime = DateTime.Now
                    }
                }
            });
        }

        return Unauthorized(new
        {
            Code = GResponseCodes.G_UNAUTHORIZED,
            Message = "用户名或密码错误"
        });
    }

    /// <summary>
    /// 创建产品 - 数值验证演示
    /// </summary>
    /// <param name="model">产品模型</param>
    /// <returns></returns>
    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductModel model)
    {
        // 自动验证会处理所有验证规则
        
        // 模拟创建产品
        await Task.Delay(50);
        
        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "产品创建成功",
            Data = new
            {
                ProductId = new Random().Next(1000, 9999),
                Name = model.Name,
                Price = model.Price,
                Stock = model.Stock,
                CreateTime = DateTime.Now
            }
        });
    }

    /// <summary>
    /// 创建订单 - 复杂验证演示
    /// </summary>
    /// <param name="model">订单模型</param>
    /// <returns></returns>
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel model)
    {
        // 自动验证包括自定义验证逻辑
        
        // 额外的业务验证
        if (model.OrderDate > DateTime.Now)
        {
            return BadRequest(new
            {
                Code = "ORDER_DATE_FUTURE",
                Message = "下单时间不能是未来时间"
            });
        }
        
        // 模拟创建订单
        await Task.Delay(100);
        
        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "订单创建成功",
            Data = new
            {
                OrderId = new Random().Next(10000, 99999),
                OrderNo = model.OrderNo,
                Amount = model.Amount,
                Status = "待支付",
                CreateTime = DateTime.Now
            }
        });
    }

    /// <summary>
    /// 文件上传 - 文件验证演示
    /// </summary>
    /// <param name="model">文件上传模型</param>
    /// <returns></returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromBody] FileUploadModel model)
    {
        // 自动验证文件相关规则
        
        // 模拟文件上传
        await Task.Delay(200);
        
        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "文件上传成功",
            Data = new
            {
                FileId = Guid.NewGuid().ToString("N"),
                FileName = model.FileName,
                FileSize = model.FileSize,
                FileType = model.FileType,
                UploadTime = DateTime.Now,
                Url = $"/files/{Guid.NewGuid():N}.{model.FileType.ToString().ToLower()}"
            }
        });
    }

    /// <summary>
    /// 批量注册用户 - 批量验证演示
    /// </summary>
    /// <param name="models">用户模型列表</param>
    /// <returns></returns>
    [HttpPost("batch-register")]
    public async Task<IActionResult> BatchRegister([FromBody] List<RegisterModel> models)
    {
        if (models == null || !models.Any())
        {
            return BadRequest(new
            {
                Code = "EMPTY_LIST",
                Message = "用户列表不能为空"
            });
        }

        var validationErrors = new List<object>();
        
        // 批量验证
        for (int i = 0; i < models.Count; i++)
        {
            var validationResult = await models[i].ValidateModelAsync();
            if (!validationResult.IsValid)
            {
                validationErrors.Add(new
                {
                    Index = i,
                    UserName = models[i].UserName,
                    Email = models[i].Email,
                    Errors = validationResult.ToDictionary()
                });
            }
        }

        if (validationErrors.Any())
        {
            return BadRequest(new
            {
                Code = GResponseCodes.G_BAD_REQUEST,
                Message = "批量验证失败",
                ValidationErrors = validationErrors,
                SuccessCount = models.Count - validationErrors.Count,
                FailureCount = validationErrors.Count
            });
        }

        // 模拟批量注册
        await Task.Delay(300);
        
        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = $"批量注册成功，共{models.Count}个用户",
            Data = models.Select((model, index) => new
            {
                Index = index,
                UserId = new Random().Next(1000, 9999),
                UserName = model.UserName,
                Email = model.Email
            }).ToList()
        });
    }

    /// <summary>
    /// 获取验证规则信息 - 用于前端动态验证
    /// </summary>
    /// <returns></returns>
    [HttpGet("validation-rules")]
    public IActionResult GetValidationRules()
    {
        var rules = new
        {
            RegisterModel = new
            {
                UserName = new[] 
                { 
                    "required:用户名不能为空", 
                    "length:3-20:用户名长度必须在3-20个字符之间",
                    "pattern:^[a-zA-Z0-9_]+$:用户名只能包含字母、数字和下划线"
                },
                Email = new[] 
                { 
                    "required:邮箱不能为空", 
                    "email:邮箱格式不正确" 
                },
                Password = new[] 
                { 
                    "required:密码不能为空", 
                    "minLength:6:密码最少6个字符",
                    "complexity:密码必须包含大小写字母和数字"
                },
                ConfirmPassword = new[] 
                { 
                    "required:确认密码不能为空", 
                    "compare:Password:两次输入的密码不一致" 
                },
                Age = new[] 
                { 
                    "range:18-100:年龄必须在18-100之间" 
                }
            },
            ProductModel = new
            {
                Name = new[] 
                { 
                    "required:产品名称不能为空", 
                    "maxLength:100:产品名称最多100个字符" 
                },
                Price = new[] 
                { 
                    "range:0.01-999999.99:价格必须在0.01-999999.99之间" 
                },
                Stock = new[] 
                { 
                    "range:0-2147483647:库存数量不能为负数" 
                }
            }
        };

        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "获取验证规则成功",
            Data = rules
        });
    }

    /// <summary>
    /// 测试验证 - 用于测试各种验证场景
    /// </summary>
    /// <param name="testData">测试数据</param>
    /// <returns></returns>
    [HttpPost("test-validation")]
    public async Task<IActionResult> TestValidation([FromBody] dynamic testData)
    {
        // 这个接口可以用来测试各种验证场景
        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "验证测试通过",
            Data = new
            {
                ReceivedData = testData,
                TestTime = DateTime.Now,
                Tips = new[]
                {
                    "可以使用 /api/demo/register 测试用户注册验证",
                    "可以使用 /api/demo/login 测试手动验证",
                    "可以使用 /api/demo/products 测试数值验证",
                    "可以使用 /api/demo/orders 测试复杂验证",
                    "可以使用 /api/demo/batch-register 测试批量验证",
                    "可以使用 /api/demo/advanced-user 测试高级用户验证",
                    "可以使用 /api/demo/company 测试公司信息验证"
                }
            }
        });
    }

    /// <summary>
    /// 高级用户注册（展示复杂验证特性）
    /// </summary>
    /// <param name="model">高级用户模型</param>
    /// <returns></returns>
    [HttpPost("advanced-user")]
    public async Task<IActionResult> CreateAdvancedUser([FromBody] AdvancedUserModel model)
    {
        // 自动验证已通过，直接处理业务逻辑
        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "高级用户创建成功",
            Data = new
            {
                model.UserName,
                model.Email,
                model.UserType,
                HasCreditCard = !string.IsNullOrEmpty(model.CreditCardNumber),
                TagCount = model.Tags?.Count ?? 0,
                HasIdCard = !string.IsNullOrEmpty(model.IdCard),
                HasPhone = !string.IsNullOrEmpty(model.Phone),
                CreateTime = DateTime.Now
            }
        });
    }

    /// <summary>
    /// 公司信息注册
    /// </summary>
    /// <param name="model">公司信息模型</param>
    /// <returns></returns>
    [HttpPost("company")]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyModel model)
    {
        // 自动验证已通过，直接处理业务逻辑
        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "公司信息注册成功",
            Data = new
            {
                model.CompanyName,
                model.UnifiedSocialCreditCode,
                model.EstablishedDate,
                model.Website,
                model.ContactEmail,
                model.ContactPhone,
                DepartmentCount = model.Departments?.Count ?? 0,
                CreateTime = DateTime.Now
            }
        });
    }

    /// <summary>
    /// 测试手动验证高级用户
    /// </summary>
    /// <param name="model">高级用户模型</param>
    /// <returns></returns>
    [HttpPost("advanced-user/manual-validation")]
    public async Task<IActionResult> ManualValidateAdvancedUser([FromBody] AdvancedUserModel model)
    {
        // 手动验证
        var validationResult = await model.ValidateModelAsync();
        
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Code = GResponseCodes.G_BAD_REQUEST,
                Message = "验证失败",
                Errors = validationResult.ToDictionary(),
                FirstError = validationResult.FirstErrorMessage
            });
        }

        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "高级用户验证通过",
            Data = new
            {
                model.UserName,
                model.Email,
                model.UserType,
                ValidationInfo = new
                {
                    IsValid = validationResult.IsValid,
                    ErrorCount = validationResult.Errors?.Count() ?? 0
                },
                TestTime = DateTime.Now
            }
        });
    }

    /// <summary>
    /// 获取验证规则示例
    /// </summary>
    /// <param name="modelName">模型名称</param>
    /// <returns></returns>
    [HttpGet("validation-examples/{modelName}")]
    public IActionResult GetValidationExamples(string modelName)
    {
        object examples = modelName.ToLowerInvariant() switch
        {
            "register" => new
            {
                ModelName = "RegisterModel",
                Description = "用户注册模型验证示例",
                ValidExample = new
                {
                    UserName = "testuser",
                    Email = "test@example.com",
                    Password = "Test123",
                    ConfirmPassword = "Test123",
                    Age = 25,
                    Phone = "13812345678",
                    IdCard = "110101199001011234",
                    Website = "https://www.example.com",
                    BirthDate = "1990-01-01"
                },
                InvalidExample = new
                {
                    UserName = "ab", // 太短
                    Email = "invalid-email", // 格式错误
                    Password = "123", // 太短
                    ConfirmPassword = "456", // 不匹配
                    Age = 17, // 太小
                    Phone = "123", // 格式错误
                    IdCard = "123", // 格式错误
                    Website = "not-a-url", // 格式错误
                    BirthDate = "2030-01-01" // 未来时间
                }
            },
            "advanceduser" => new
            {
                ModelName = "AdvancedUserModel",
                Description = "高级用户模型验证示例",
                ValidExample = new
                {
                    UserName = "vipuser",
                    Email = "vip@example.com",
                    UserType = 2, // VIP
                    CreditCardNumber = "4111111111111111", // VIP用户必填
                    ConfirmEmail = "vip@example.com",
                    Tags = new[] { "tag1", "tag2" },
                    IdCard = "110101199001011234",
                    Phone = "13812345678"
                },
                InvalidExample = new
                {
                    UserName = "ab", // 太短
                    Email = "invalid-email", // 格式错误
                    UserType = 2, // VIP
                    CreditCardNumber = "", // VIP用户必填但为空
                    ConfirmEmail = "different@example.com", // 与email不匹配
                    Tags = new string[0], // 空集合
                    IdCard = "123", // 格式错误
                    Phone = "123" // 格式错误
                }
            },
            "company" => new
            {
                ModelName = "CompanyModel",
                Description = "公司信息模型验证示例",
                ValidExample = new
                {
                    CompanyName = "示例科技有限公司",
                    UnifiedSocialCreditCode = "91110000123456789X",
                    EstablishedDate = "2020-01-01",
                    Website = "https://www.example.com",
                    ContactEmail = "contact@example.com",
                    ContactPhone = "13812345678",
                    Departments = new[]
                    {
                        new { Name = "技术部", EmployeeCount = 10, Email = "tech@example.com" },
                        new { Name = "市场部", EmployeeCount = 5, Email = "market@example.com" }
                    }
                },
                InvalidExample = new
                {
                    CompanyName = "A", // 太短
                    UnifiedSocialCreditCode = "123", // 格式错误
                    EstablishedDate = "2030-01-01", // 未来时间
                    Website = "not-a-url", // 格式错误
                    ContactEmail = "invalid-email", // 格式错误
                    ContactPhone = "123", // 格式错误
                    Departments = new object[0] // 空集合
                }
            },
            _ => new
            {
                Error = "未知的模型名称",
                AvailableModels = new[] { "register", "advanceduser", "company" }
            }
        };

        return Ok(new
        {
            Code = GResponseCodes.G_SUCCESS,
            Message = "获取验证示例成功",
            Data = examples
        });
    }
}