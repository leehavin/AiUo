using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using AiUo.Reflection;

namespace AiUo.Net;

/// <summary>
/// UriBuilder继承扩展类
/// </summary>
public class UriBuilderEx : UriBuilder
{
    /// <summary>
    /// URL中的QueryString键值对
    /// </summary>
    public NameValueCollection QueryString { get; private set; }
    public Encoding Encoding { get; set; }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="encoding">字符集，默认UTF8</param>
    public UriBuilderEx(string uri, Encoding encoding = null) : base(uri) 
    {
        QueryString = HttpUtility.ParseQueryString(Query);
        Encoding = encoding ?? Encoding.UTF8;
    }

    public void RemoveQueryString(string name)
    {
        QueryString.Remove(name);
    }

    /// <summary>
    /// 添加QueryString值，使用HttpUtility.UrlEncode编码
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void AppendQueryString(string name, string value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException("name");
        if (!string.IsNullOrEmpty(value))
            value = HttpUtility.UrlEncode(value, Encoding);
        QueryString[name] = value;
    }

    /// <summary>
    /// 使用参数对象反射得到QueryString
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    public void AppendQueryString<T>(T item)
    {
        var type = item.GetType();
        if (ReflectionUtil.IsSimpleType(type))
            throw new Exception("简单类型无法直接给URL赋值。");
        foreach (var property in type.GetProperties())
        {
            var value = ReflectionUtil.GetPropertyValue(item, property.Name);
            AppendQueryString(property.Name, Convert.ToString(value));
        }
    }
    /// <summary>
    /// 添加QueryString值
    /// </summary>
    /// <param name="paramData"></param>
    public void AppendQueryString(IDictionary<string, string> paramData)
    {
        foreach (var item in paramData)
        {
            AppendQueryString(item.Key, item.Value);
        }
    }
    /// <summary>
    /// 获取QueryString值，如果指定Encoding则使用HttpUtility.UrlDecode解码
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetQueryStringValue(string name)
    {
        if (!string.IsNullOrEmpty(name))
            throw new ArgumentNullException("name");
        var value = QueryString.Get(name);
        return string.IsNullOrEmpty(value) ? value : HttpUtility.UrlDecode(value, Encoding);
    }

    private void ModifyQuery()
    {
        if (QueryString.HasKeys())
        {
            var sb = new StringBuilder();
            sb.Append('?');
            for (int i = 0; i < QueryString.Count; i++)
            {
                sb.Append($"{QueryString.Keys[i]}={QueryString[i]}");
                if (i < QueryString.Count - 1)
                    sb.Append('&');
            }
            Query = sb.ToString();
        }
    }
    public override string ToString()
    {
        ModifyQuery();
        return Uri.ToString();
    }
}