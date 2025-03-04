namespace AiUo.Net;

/// <summary>
/// 平台约定的错误码:
///     CustomException.Code
///     ApiResult.Code
///     ProtoResponse.Code
/// </summary>
public class GResponseCodes
{
    #region 基础
    /// <summary>
    /// 请求错误
    /// </summary>
    public const string G_BAD_REQUEST = "G_BAD_REQUEST";
    /// <summary>
    /// 服务器未处理异常
    /// </summary>
    public const string G_INTERNAL_SERVER_ERROR = "G_INTERNAL_SERVER_ERROR";
    #endregion

    #region 认证和安全
    /// <summary>
    /// 授权错误:请求的资源需要身份验证
    /// </summary>
    public const string G_UNAUTHORIZED = "G_UNAUTHORIZED";
    /// <summary>
    /// Jwt Token无效
    /// </summary>
    public const string G_JWT_TOKEN_INVALID = "G_JWT_TOKEN_INVALID";
    /// <summary>
    /// Jwt Token过期
    /// </summary>
    public const string G_JWT_TOKEN_EXPIRED = "G_JWT_TOKEN_EXPIRED";
    /// <summary>
    /// Ticket无效
    /// </summary>
    public const string G_TICKET_INVALID = "G_TICKET_INVALID";
    /// <summary>
    /// 用户请求限制(ip,user等)
    /// </summary>
    public const string G_REQUEST_LIMIT = "G_REQUEST_LIMIT";
    /// <summary>
    /// 用户请求频率限制
    /// </summary>
    public const string G_REQUEST_RATE_LIMIT = "G_REQUEST_RATE_LIMIT";
    #endregion

    #region 服务器
    /// <summary>
    /// 服务连接异常
    /// </summary>
    public const string G_SERVER_CONNECT_ERROR = "G_SERVER_CONNECT_ERROR";
    /// <summary>
    /// 服务准备停止
    /// </summary>
    public const string G_SERVER_STOPPING = "G_SERVER_STOPPING";
    #endregion
}