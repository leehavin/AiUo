using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AiUo.Collections;
using AiUo.Configuration;

namespace AiUo.AspNet.RequestLogging;

public class RequestLoggingSection : ConfigSection
{
    public override string SectionName => "RequestLogging";

    public bool Enabled { get; set; }
    public RequestLoggingRule DefaultRule { get; set; } = new();
    public RequestLoggingRule MatchRule { get; set; } = new();
    public Dictionary<string, RequestLoggingRule> NamingRules = new();

    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);

        DefaultRule.Name = "#DEFAULT_RULE#";
        DefaultRule.Type = RequestLoggingRuleType.Default;
        MatchRule.Name = "#MATCH_RULE#";
        MatchRule.Type = RequestLoggingRuleType.Match;

        NamingRules = configuration.GetSection("NamingRules")
            .Get<Dictionary<string, RequestLoggingRule>>() ?? new();
        NamingRules.ForEach(x =>
        {
            x.Value.Name = x.Key;
            x.Value.Type = RequestLoggingRuleType.Naming;
        });
    }
}
public class RequestLoggingRule
{
    public string Name { get; set; }
    public RequestLoggingRuleType Type { get; set; } = RequestLoggingRuleType.Default;

    public LogLevel Level { get; set; } = LogLevel.Debug;
    public LogLevel CustomeExceptionLevel { get; set; } = LogLevel.Information;

    public List<string> Properties { get; set; } = new();

    public List<string> Urls { get; set; } = new();

    private object _sync = new();
    private RequestLoggingProperty _property;
    internal RequestLoggingProperty GetProperty()
    {
        if (_property == null)
        {
            lock (_sync)
            {
                if (_property == null)
                {
                    var dict = Properties?.ToHashSet() ?? new HashSet<string>();
                    _property = new RequestLoggingProperty(dict);
                }
            }
        }
        return _property;
    }
    private HashSet<string> _urlDict;
    private List<string> _urlStarts;
    internal bool IsMatch(string url)
    {
        if (Urls.Count == 0)
            return false;

        if (_urlDict == null)
        {
            lock (_sync)
            {
                if (_urlDict == null)
                {
                    _urlDict = new();
                    _urlStarts = new();
                    Urls.ForEach(x =>
                    {
                        if (x.EndsWith('*'))
                            _urlStarts.Add(x.TrimEnd('*'));
                        else
                            _urlDict.Add(x);
                    });
                }
            }
        }
        if (_urlDict.Contains("*") || _urlDict.Contains(url))
            return true;
        foreach (var start in _urlStarts)
        {
            if (url.StartsWith(start))
                return true;
        }
        return false;
    }
}
public enum RequestLoggingRuleType
{
    Default,
    Match,
    Naming
}

internal class RequestLoggingProperty
{
    public bool Referer { get; set; }
    public bool RemoteIp { get; set; }
    public bool UserId { get; set; }

    public bool RequestBody { get; set; }
    public bool RequestHeaders { get; set; }
    public bool ResponseBody { get; set; }
    public bool ResponseHeaders { get; set; }

    public RequestLoggingProperty(HashSet<string> dict)
    {
        Referer = dict.Contains("Referer");
        RemoteIp = dict.Contains("RemoteIp");
        UserId = dict.Contains("UserId");
        RequestBody = dict.Contains("RequestBody");
        RequestHeaders = dict.Contains("RequestHeaders");
        ResponseBody = dict.Contains("ResponseBody");
        ResponseHeaders = dict.Contains("ResponseHeaders");
    }
}