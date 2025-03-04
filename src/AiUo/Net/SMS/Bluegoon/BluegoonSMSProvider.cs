using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AiUo.Caching;

namespace AiUo.Net.SMS.Bluegoon;

internal class BluegoonSMSProvider : ISMSProvider
{
    private BluegoonSMSElement _element;
    private HttpClient _client;
    public BluegoonSMSProvider(BluegoonSMSElement element)
    {
        _element= element;
        if (string.IsNullOrEmpty(_element.RequestUrl))
            _element.RequestUrl = "http://open.bluegoon.com/api/sms/sendcode";
        _client = HttpClientExFactory.CreateClient("SMS");
    }
    public async Task<SMSResult> SendCodeAsync(string phoneNumber)
    {
        var timestamp = (DateTimeOffset.UtcNow.Ticks - 621355968000000000) / 10000000;
        var uri = new UriBuilderEx(_element.RequestUrl);
        uri.AppendQueryString("CompanyCode", _element.CompanyCode);
        uri.AppendQueryString("Phone", phoneNumber);
        uri.AppendQueryString("TimeStamp", timestamp.ToString());
        uri.AppendQueryString("Sign", Sign(phoneNumber, timestamp));

        var req = await _client.GetAsync(uri.ToString());
        var content = await req.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DefaultSMSResult>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        var ret = new SMSResult() { Success = result.Code == 1, Msg = result.Msg };
        if (!ret.Success)
        {
            var err = new Exception(result.Msg);
            err.AddData(uri.ToString());
            err.AddData(phoneNumber);
            err.AddData(content);
            throw err;
        }
        TicketCacheUtil.SetTicket(phoneNumber, result.Data, _element.ExpireMinutes);
        return ret;
    }

    public bool Validate(string phoneNumber, string code)
    {
        return TicketCacheUtil.ValidateTicket(phoneNumber, code);
    }


    private string Sign(string phoneNumber, long timestamp)
    {
        return Hash($"{_element.CompanyCode}{phoneNumber}{timestamp}{_element.MD5Key}");
    }

    private static string Hash(string data)
    {
        using (var md5 = MD5.Create())
        {
            var sign = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(data)));
            sign = sign.Replace("-", "").ToLowerInvariant();
            return sign;
        }
    }
    private class DefaultSMSResult
    {
        public int Code { get; set; }

        public string Data { get; set; } = "";

        public string Msg { get; set; } = "";
    }
}