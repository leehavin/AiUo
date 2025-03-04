using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Json;
using AiUo.Caching;

namespace AiUo.Net.SMS.Tencent;

public class TencentSMSProvider : ISMSProvider
{
    private TencentSMSElement _element;
    private HttpClient _client;
    public TencentSMSProvider(TencentSMSElement element)
    {
        _element = element;
        if (string.IsNullOrEmpty(_element.RequestUrl))
            _element.RequestUrl = "https://yun.tim.qq.com/v5/tlssmssvr/sendsms";
        _client = HttpClientExFactory.CreateClient("SMS");
    }

    public async Task<SMSResult> SendCodeAsync(string phoneNumber)
    {
        // post https://yun.tim.qq.com/v5/tlssmssvr/sendsms?sdkappid=xxxxx&random=xxxx
        var timestamp = (DateTimeOffset.UtcNow.Ticks - 621355968000000000) / 10000000;
        var uri = new UriBuilderEx(_element.RequestUrl);
        uri.AppendQueryString("sdkappid", _element.AppId);
        uri.AppendQueryString("random", timestamp.ToString());
        var postData = new TencentSendData()
        {
            Sig = Sign(timestamp, phoneNumber),
            Sign = _element.Sign,
            Time = timestamp,
            Tel = new TencentPhone() { Mobile = phoneNumber },
            Tpl_id = _element.TplId
        };
        var code = TicketCacheUtil.GenerateTicket(phoneNumber, 4, CharsScope.Numbers, _element.ExpireMinutes);
        postData.Params.Add(code);
        postData.Params.Add(_element.ExpireMinutes.ToString());

        var result = _element.Debug
            ? await Task.FromResult(new TencenResponse() { Result = 0 })
            : await RequestSendCodeUrl(uri.ToString(), phoneNumber, postData);
        var ret = new SMSResult() { Success = result.Result == 0, Msg = result.Errmsg };

        // debug 模式下发验证码到客户端
        if (_element.Debug)
            ret.Data = code;
        return ret;
    }
    private async Task<TencenResponse> RequestSendCodeUrl(string url, string phoneNumber, TencentSendData postData)
    {
        var req = await _client.PostAsJsonAsync(url, postData, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = await req.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TencenResponse>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        if (result.Result != 0)
        {
            var err = new Exception(result.Errmsg);
            err.AddData(url);
            err.AddData(phoneNumber);
            err.AddData(content);
            throw err;
        }
        return result;
    }
    /// <summary>
    /// 验证手机验证码是否正确方法
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public bool Validate(string phoneNumber, string code)
    {
        return TicketCacheUtil.ValidateTicket(phoneNumber, code);
    }

    private string Sign(long timestamp, string phoneNumber)
    {
        return Hash($"appkey={_element.AppKey}&random={timestamp}&time={timestamp}&mobile={phoneNumber}");
    }

    private static string Hash(string data)
    {
        using (var algo = SHA256.Create())
        {
            var sign = BitConverter.ToString(algo.ComputeHash(Encoding.UTF8.GetBytes(data)));
            sign = sign.Replace("-", "").ToLowerInvariant();
            return sign;
        }
    }

    #region docs

    /// <summary>
    /// 文档 https://cloud.tencent.com/document/product/382/5976
    /// </summary>
    private class TencentSendData
    {
        public string Ext { get; set; } = "";

        public string Extend { get; set; } = "";

        public ICollection<string> Params { get; } = new HashSet<string>();

        public string Sig { get; set; } = "";

        public string Sign { get; set; } = "";

        public TencentPhone Tel { get; set; } = new TencentPhone();

        public long Time { get; set; }

        public int Tpl_id { get; set; }
    }

    private class TencentPhone
    {
        public string Mobile { get; set; } = "";

        public string Nationcode { get; set; } = "86";
    }

    private class TencenResponse
    {
        public int Result { get; set; } = -1;

        public string Errmsg { get; set; } = "";

        public string Ext { get; set; } = "";

        public int Fee { get; set; }

        public string Sid { get; set; } = "";
    }
    #endregion
}