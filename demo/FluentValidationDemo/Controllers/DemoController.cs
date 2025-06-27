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
                Url = $"/files/{Guid.NewGuid():N}{model.FileExtension}"
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
                    "可以使用 /api/demo/batch-register 测试批量验证"
                }
            }
        });
    }
}