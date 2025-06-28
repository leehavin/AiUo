using Microsoft.AspNetCore.Mvc;
using AiUo.AspNet.Validations.FluentValidation.Services;
using AiUo.AspNet.Validations.FluentValidation.Extensions;
using FluentValidationDemo.Models;

namespace FluentValidationDemo.Controllers;

/// <summary>
/// FluentCustomAttribute 自定义验证演示控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomValidationController : ControllerBase
{
    private readonly IValidationService _validationService;

    public CustomValidationController(IValidationService validationService)
    {
        _validationService = validationService;
    }

    /// <summary>
    /// 测试基础自定义验证
    /// </summary>
    /// <param name="model">用户模型</param>
    /// <returns>验证结果</returns>
    [HttpPost("basic-custom-validation")]
    public async Task<IActionResult> TestBasicCustomValidation([FromBody] FluentCustomAttributeDemo model)
    {
        // 由于使用了FluentValidationActionFilter，验证会自动进行
        // 如果验证失败，会自动返回400错误
        
        return Ok(new
        {
            Message = "验证通过！",
            Data = model,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// 测试高级自定义验证
    /// </summary>
    /// <param name="model">高级验证模型</param>
    /// <returns>验证结果</returns>
    [HttpPost("advanced-custom-validation")]
    public async Task<IActionResult> TestAdvancedCustomValidation([FromBody] AdvancedCustomValidationDemo model)
    {
        return Ok(new
        {
            Message = "高级自定义验证通过！",
            Data = model,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// 手动验证示例
    /// </summary>
    /// <param name="model">用户模型</param>
    /// <returns>验证结果</returns>
    [HttpPost("manual-validation")]
    public async Task<IActionResult> TestManualValidation([FromBody] FluentCustomAttributeDemo model)
    {
        // 手动进行验证
        var validationResult = await _validationService.ValidateAsync(model);
        
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Message = "验证失败",
                Errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Code = e.ErrorCode,
                    Message = e.ErrorMessage
                }),
                Timestamp = DateTime.Now
            });
        }

        return Ok(new
        {
            Message = "手动验证通过！",
            Data = model,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// 获取自定义验证示例数据
    /// </summary>
    /// <returns>示例数据</returns>
    [HttpGet("examples")]
    public IActionResult GetExamples()
    {
        return Ok(new
        {
            Message = "FluentCustomAttribute 使用示例",
            Examples = new
            {
                BasicCustomValidation = new
                {
                    Description = "基础自定义验证示例",
                    ValidData = new FluentCustomAttributeDemo
                    {
                        UserName = "john_doe",
                        Password = "MyPassword123!",
                        Age = 24,
                        Email = "john@example.com"
                    },
                    InvalidData = new FluentCustomAttributeDemo
                    {
                        UserName = "admin_user", // 包含敏感词
                        Password = "MyPassword123", // 缺少特殊字符
                        Age = 25, // 不是偶数
                        Email = "test@10minutemail.com" // 临时邮箱
                    }
                },
                AdvancedCustomValidation = new
                {
                    Description = "高级自定义验证示例",
                    ValidData = new AdvancedCustomValidationDemo
                    {
                        ProductCode = "PRD-12345678"
                    },
                    InvalidData = new AdvancedCustomValidationDemo
                    {
                        ProductCode = "INVALID-CODE" // 格式不正确
                    }
                }
            },
            ValidationRules = new
            {
                UserName = "不能包含敏感词汇（admin, root, test, guest, anonymous）",
                Password = "必须包含至少一个特殊字符（!@#$%^&*）",
                Age = "必须是偶数",
                Email = "不能使用临时邮箱域名",
                ProductCode = "必须是PRD-开头，后跟8位数字"
            },
            Usage = new
            {
                Description = "FluentCustomAttribute 使用方法",
                Syntax = "[FluentCustom(validationFunc, code, message)]",
                Parameters = new
                {
                    validationFunc = "Func<object, bool> - 自定义验证函数，返回true表示验证通过",
                    code = "string - 错误代码（可选）",
                    message = "string - 错误消息（可选）"
                },
                Examples = new[]
                {
                    "[FluentCustom(ValidateUserName, \"USER_NAME_SENSITIVE\", \"用户名不能包含敏感词汇\")]",
                    "[FluentCustom(value => ValidateProductCode(value as string), \"PRODUCT_CODE_FORMAT\", \"产品代码格式不正确\")]"
                }
            }
        });
    }
}