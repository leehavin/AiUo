namespace AiUo.OAuth;

public class OAuthUserIpo
{
    /// <summary>
    ///  Google = 2
    ///  Facebook = 1,
    /// </summary>
    public OAuthProviders OAuthProvider { get; set; }
    /// <summary>
    /// 访问AuthorizeUrl后回调时带的参数code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 访问AuthorizeUrl后回调时带的参数state，用于和请求AuthorizeUrl前的state比较，防止CSRF攻击
    /// </summary>
    public string State { get; set; }

    /// <summary>
    ///  回调后返回的oauth_token
    /// </summary>
    public string AccessToken { get; set; }

    public string Uuid { get; set; }
}

/// <summary>
/// 三方返回用户信息
/// </summary>
public class OAuthUserDto
{
    /// <summary>
    ///  用户第三方系统的唯一id。在调用方集成改组件时，可以用uuid + source唯一确定一个用户
    /// </summary>
    public string OAuthId { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }
    /// <summary>
    /// 用户昵称
    /// </summary>
    public string NickName { get; set; }
    /// <summary>
    /// 用户头像
    /// </summary>
    public string Avatar { get; set; }
    /// <summary>
    /// 用户网址
    /// </summary>
    public string Blog { get; set; }
    /// <summary>
    /// 所在公司
    /// </summary>
    public string Company { get; set; }
    /// <summary>
    /// 位置
    /// </summary>
    public string Location { get; set; }
    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// 用户备注（各平台中的用户个人介绍）
    /// </summary>
    public string Remark { get; set; }
}