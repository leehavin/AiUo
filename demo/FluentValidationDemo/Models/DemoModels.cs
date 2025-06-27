using AiUo.AspNet.Validations;

namespace FluentValidationDemo.Models;

/// <summary>
/// 用户注册模型
/// </summary>
public class RegisterModel
{
    /// <summary>
    /// 用户名
    /// </summary>
    [FluentRequired("USER_NAME_REQUIRED", "用户名不能为空")]
    [FluentLength(3, 20, "USER_NAME_LENGTH", "用户名长度必须在3-20个字符之间")]
    [FluentRegularExpression(@"^[a-zA-Z0-9_]+$", "USER_NAME_FORMAT", "用户名只能包含字母、数字和下划线")]
    public string UserName { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [FluentRequired("EMAIL_REQUIRED", "邮箱不能为空")]
    [FluentEmail("EMAIL_FORMAT", "邮箱格式不正确")]
    public string Email { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [FluentRequired("PASSWORD_REQUIRED", "密码不能为空")]
    [FluentMinLength(6, "PASSWORD_MIN_LENGTH", "密码最少6个字符")]
    [FluentRegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{6,}$", 
        "PASSWORD_COMPLEXITY", "密码必须包含大小写字母和数字")]
    public string Password { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    [FluentRequired("CONFIRM_PASSWORD_REQUIRED", "确认密码不能为空")]
    [FluentCompare(nameof(Password), "PASSWORD_MISMATCH", "两次输入的密码不一致")]
    public string ConfirmPassword { get; set; }

    /// <summary>
    /// 年龄
    /// </summary>
    [FluentRange(18, 100, "AGE_RANGE", "年龄必须在18-100之间")]
    public int Age { get; set; }

    /// <summary>
    /// 手机号（可选）
    /// </summary>
    [FluentRegularExpression(@"^1[3-9]\d{9}$", "PHONE_FORMAT", "手机号格式不正确")]
    public string Phone { get; set; }
}

/// <summary>
/// 用户登录模型
/// </summary>
public class LoginModel
{
    /// <summary>
    /// 用户名或邮箱
    /// </summary>
    [FluentRequired("LOGIN_NAME_REQUIRED", "用户名或邮箱不能为空")]
    public string LoginName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [FluentRequired("PASSWORD_REQUIRED", "密码不能为空")]
    public string Password { get; set; }

    /// <summary>
    /// 记住我
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// 产品模型
/// </summary>
public class ProductModel
{
    /// <summary>
    /// 产品名称
    /// </summary>
    [FluentRequired("PRODUCT_NAME_REQUIRED", "产品名称不能为空")]
    [FluentMaxLength(100, "PRODUCT_NAME_MAX_LENGTH", "产品名称最多100个字符")]
    public string Name { get; set; }

    /// <summary>
    /// 产品描述
    /// </summary>
    [FluentMaxLength(500, "PRODUCT_DESC_MAX_LENGTH", "产品描述最多500个字符")]
    public string Description { get; set; }

    /// <summary>
    /// 价格
    /// </summary>
    [FluentRange(0.01, 999999.99, "PRICE_RANGE", "价格必须在0.01-999999.99之间")]
    public decimal Price { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [FluentRange(0, int.MaxValue, "STOCK_RANGE", "库存数量不能为负数")]
    public int Stock { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    [FluentRange(1, int.MaxValue, "CATEGORY_ID_INVALID", "分类ID必须大于0")]
    public int CategoryId { get; set; }
}

/// <summary>
/// 订单模型 - 展示复杂验证
/// </summary>
public class OrderModel
{
    /// <summary>
    /// 订单号
    /// </summary>
    [FluentRequired("ORDER_NO_REQUIRED", "订单号不能为空")]
    [FluentRegularExpression(@"^ORD\d{10}$", "ORDER_NO_FORMAT", "订单号格式：ORD + 10位数字")]
    public string OrderNo { get; set; }

    /// <summary>
    /// 客户邮箱
    /// </summary>
    [FluentRequired("CUSTOMER_EMAIL_REQUIRED", "客户邮箱不能为空")]
    [FluentEmail("CUSTOMER_EMAIL_FORMAT", "客户邮箱格式不正确")]
    public string CustomerEmail { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    [FluentRange(0.01, 999999.99, "ORDER_AMOUNT_RANGE", "订单金额必须大于0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 下单时间
    /// </summary>
    [FluentRequired("ORDER_DATE_REQUIRED", "下单时间不能为空")]
    public DateTime OrderDate { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    [FluentMaxLength(200, "REMARK_MAX_LENGTH", "备注最多200个字符")]
    public string Remark { get; set; }
}

/// <summary>
/// 文件上传模型
/// </summary>
public class FileUploadModel
{
    /// <summary>
    /// 文件名
    /// </summary>
    [FluentRequired("FILE_NAME_REQUIRED", "文件名不能为空")]
    [FluentMaxLength(255, "FILE_NAME_MAX_LENGTH", "文件名最多255个字符")]
    public string FileName { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [FluentRange(1, 10 * 1024 * 1024, "FILE_SIZE_RANGE", "文件大小必须在1字节到10MB之间")]
    public long FileSize { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [FluentRequired("FILE_TYPE_REQUIRED", "文件类型不能为空")]
    [FluentRegularExpression(@"^(image|document|video|audio)$", "FILE_TYPE_INVALID", "文件类型只能是：image、document、video、audio")]
    public string FileType { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    [FluentRequired("FILE_EXT_REQUIRED", "文件扩展名不能为空")]
    [FluentRegularExpression(@"^\.(jpg|jpeg|png|gif|pdf|doc|docx|xls|xlsx|mp4|mp3)$", "FILE_EXT_INVALID", "不支持的文件扩展名")]
    public string FileExtension { get; set; }
}