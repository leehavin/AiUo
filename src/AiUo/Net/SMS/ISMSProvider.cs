using System.Threading.Tasks;

namespace AiUo.Net.SMS;

public interface ISMSProvider
{
    /// <summary>
    ///  手机下发验证码方法
    /// </summary>
    /// <param name="phoneNumber">手机号</param>
    /// <returns></returns>
    Task<SMSResult> SendCodeAsync(string phoneNumber);

    /// <summary>
    ///  验证手机验证码是否正确方法
    /// </summary>
    /// <param name="phoneNumber">手机号</param>
    /// <param name="code">验证码</param>
    /// <returns></returns>
    bool Validate(string phoneNumber, string code);
}

/// <summary>
/// 短信结果实体类
/// </summary>
public class SMSResult
{
    /// <summary>
    /// 短信验证码是否发送成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 短信验证码返回数据
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// 短信失败原因提示信息
    /// </summary>
    public string Msg { get; set; }
}