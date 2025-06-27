using AiUo.AspNet.Validations;
using AiUo.AspNet.Validations.FluentValidation;
using System.ComponentModel.DataAnnotations;

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
    [FluentChinesePhone("PHONE_FORMAT", "手机号格式不正确")]
    public string Phone { get; set; }

    /// <summary>
    /// 身份证号（可选）
    /// </summary>
    [FluentChineseIdCard("ID_CARD_FORMAT", "身份证号格式不正确")]
    public string IdCard { get; set; }

    /// <summary>
    /// 个人网站（可选）
    /// </summary>
    [FluentUrl("WEBSITE_FORMAT", "个人网站URL格式不正确")]
    public string Website { get; set; }

    /// <summary>
    /// 出生日期（可选）
    /// </summary>
    [FluentDateRange(allowFuture: false, code: "BIRTH_DATE_RANGE", message: "出生日期不能是未来时间")]
    public DateTime? BirthDate { get; set; }
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
    [FluentMaxLength(255, "FILE_NAME_MAX_LENGTH", "文件名长度不能超过255个字符")]
    [FluentFileExtension(".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx", "FILE_EXTENSION", "不支持的文件格式")]
    public string FileName { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [FluentRequired("FILE_SIZE_REQUIRED", "文件大小不能为空")]
    [FluentRange(1, 10485760, "FILE_SIZE_RANGE", "文件大小必须在1B-10MB之间")] // 10MB = 10 * 1024 * 1024
    public long FileSize { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [FluentRequired("FILE_TYPE_REQUIRED", "文件类型不能为空")]
    [FluentEnum(typeof(FileTypeEnum), "FILE_TYPE_ENUM", "文件类型必须是有效的枚举值")]
    public FileTypeEnum FileType { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    [FluentJson("METADATA_JSON", "元数据必须是有效的JSON格式")]
    public string Metadata { get; set; }
}

/// <summary>
/// 文件类型枚举
/// </summary>
public enum FileTypeEnum
{
    Image = 1,
    Document = 2,
    Video = 3,
    Audio = 4
}

/// <summary>
/// 高级用户模型（展示复杂验证）
/// </summary>
public class AdvancedUserModel
{
    [FluentRequired("USER_NAME_REQUIRED", "用户名不能为空")]
    [FluentLength(3, 20, "USER_NAME_LENGTH", "用户名长度必须在3-20个字符之间")]
    public string UserName { get; set; }

    [FluentRequired("EMAIL_REQUIRED", "邮箱不能为空")]
    [FluentEmail("EMAIL_FORMAT", "邮箱格式不正确")]
    public string Email { get; set; }

    [FluentRequired("USER_TYPE_REQUIRED", "用户类型不能为空")]
    public UserType UserType { get; set; }

    // 当用户类型为VIP时，信用卡号为必填
    [FluentWhenAdvanced(nameof(UserType), UserType.VIP, code: "CREDIT_CARD_REQUIRED_FOR_VIP", message: "VIP用户必须提供信用卡号")]
    [FluentCreditCard("CREDIT_CARD_FORMAT", "信用卡号格式不正确")]
    public string CreditCardNumber { get; set; }

    // 依赖验证：确认邮箱必须与邮箱相同
    [FluentDependent(nameof(Email), null, ComparisonType.NotEqual, "CONFIRM_EMAIL_REQUIRED", "请确认邮箱地址")]
    [FluentCompare(nameof(Email), "CONFIRM_EMAIL_MATCH", "确认邮箱与邮箱不匹配")]
    public string ConfirmEmail { get; set; }

    [FluentCollection(minCount: 1, maxCount: 5, allowEmpty: false, code: "TAGS_COUNT", message: "标签数量必须在1-5个之间")]
    public List<string> Tags { get; set; }

    [FluentChineseIdCard("ID_CARD_FORMAT", "身份证号格式不正确")]
    public string IdCard { get; set; }

    [FluentChinesePhone("PHONE_FORMAT", "手机号格式不正确")]
    public string Phone { get; set; }
}

/// <summary>
/// 用户类型枚举
/// </summary>
public enum UserType
{
    Regular = 1,
    VIP = 2,
    Premium = 3
}

/// <summary>
/// 公司信息模型
/// </summary>
public class CompanyModel
{
    [FluentRequired("COMPANY_NAME_REQUIRED", "公司名称不能为空")]
    [FluentLength(2, 100, "COMPANY_NAME_LENGTH", "公司名称长度必须在2-100个字符之间")]
    public string CompanyName { get; set; }

    [FluentUnifiedSocialCreditCode("CREDIT_CODE_FORMAT", "统一社会信用代码格式不正确")]
    public string UnifiedSocialCreditCode { get; set; }

    [FluentRequired("ESTABLISHED_DATE_REQUIRED", "成立日期不能为空")]
    [FluentDateRange(maxDate: "2024-12-31", allowFuture: false, code: "ESTABLISHED_DATE_RANGE", message: "成立日期不能是未来时间")]
    public DateTime EstablishedDate { get; set; }

    [FluentUrl("WEBSITE_FORMAT", "公司网站URL格式不正确")]
    public string Website { get; set; }

    [FluentEmail("CONTACT_EMAIL_FORMAT", "联系邮箱格式不正确")]
    public string ContactEmail { get; set; }

    [FluentChinesePhone("CONTACT_PHONE_FORMAT", "联系电话格式不正确")]
    public string ContactPhone { get; set; }

    [FluentCollection(minCount: 1, maxCount: 10, allowEmpty: false, code: "DEPARTMENTS_COUNT", message: "部门数量必须在1-10个之间")]
    public List<DepartmentModel> Departments { get; set; }
}

/// <summary>
/// 部门模型
/// </summary>
public class DepartmentModel
{
    [FluentRequired("DEPT_NAME_REQUIRED", "部门名称不能为空")]
    [FluentLength(2, 50, "DEPT_NAME_LENGTH", "部门名称长度必须在2-50个字符之间")]
    public string Name { get; set; }

    [FluentRange(1, 1000, "EMPLOYEE_COUNT_RANGE", "员工数量必须在1-1000之间")]
    public int EmployeeCount { get; set; }

    [FluentEmail("DEPT_EMAIL_FORMAT", "部门邮箱格式不正确")]
    public string Email { get; set; }
}