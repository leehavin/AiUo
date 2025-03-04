using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace AiUo.Xml;

/// <summary>
/// .net 注释xml解析器
/// </summary>
public class XmlDocumentParser
{
    private XPathNavigator _documentNavigator;
    private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
    private const string MethodExpression = "/doc/members/member[@name='M:{0}']";
    private const string PropertyExpression = "/doc/members/member[@name='P:{0}']";
    private const string FieldExpression = "/doc/members/member[@name='F:{0}']";
    private const string ParameterExpression = "param[@name='{0}']";

    public XPathDocument Document { get; private set; }

    #region Constructors
    public XmlDocumentParser(Stream stream)
    {
        Init(stream);
    }
    public XmlDocumentParser(List<string> xmlFiles)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<?xml version='1.0'?>");
        sb.AppendLine("<doc>");
        sb.AppendLine("<members>");
        foreach (var docFile in xmlFiles)
        {
            var xml = new XmlDocument();
            xml.Load(docFile);
            var nodes = xml.SelectNodes("/doc/members/member");
            foreach (XmlNode item in nodes)
                sb.AppendLine(item.OuterXml);
        }
        sb.AppendLine("</members>");
        sb.AppendLine("</doc>");
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        Init(stream);
    }
    private void Init(Stream stream)
    {
        Document = new XPathDocument(stream);
        _documentNavigator = Document.CreateNavigator();
    }
    #endregion

    public string GetSummary(Type type)
    {
        XPathNavigator typeNode = GetTypeNode(type);
        return GetTagValue(typeNode, "summary");
    }

    public string GetSummary(MemberInfo member)
    {
        string memberName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(member.DeclaringType), member.Name);
        string expression = member.MemberType == MemberTypes.Field ? FieldExpression : PropertyExpression;
        string selectExpression = string.Format(CultureInfo.InvariantCulture, expression, memberName);
        XPathNavigator propertyNode = _documentNavigator.SelectSingleNode(selectExpression);
        return GetTagValue(propertyNode, "summary");
    }

    #region MethodInfo
    public string GetSummary(MethodInfo method)
    {
        var methodNode = GetMethodNavigator(method);
        return GetTagValue(methodNode, "summary");
    }
    public string GetParameter(MethodInfo method, string parameterName)
    {
        var methodNode = GetMethodNavigator(method);
        if (methodNode != null)
        {
            XPathNavigator parameterNode = methodNode.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, ParameterExpression, parameterName));
            if (parameterNode != null)
            {
                return parameterNode.Value.Trim();
            }
        }
        return null;
    }
    public string GetReturn(MethodInfo method)
    {
        var methodNode = GetMethodNavigator(method);
        return GetTagValue(methodNode, "returns");
    }
    #endregion

    #region Utils

    private XPathNavigator GetMethodNavigator(MethodInfo method)
    {
        string selectExpression = string.Format(CultureInfo.InvariantCulture, MethodExpression, GetMemberName(method));
        return _documentNavigator.SelectSingleNode(selectExpression);
    }
    private static string GetMemberName(MethodInfo method)
    {
        string name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(method.DeclaringType), method.Name);
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 0)
        {
            string[] parameterTypeNames = parameters.Select(param => GetTypeName(param.ParameterType)).ToArray();
            name += string.Format(CultureInfo.InvariantCulture, "({0})", string.Join(",", parameterTypeNames));
        }

        return name;
    }

    private static string GetTagValue(XPathNavigator parentNode, string tagName)
    {
        if (parentNode != null)
        {
            XPathNavigator node = parentNode.SelectSingleNode(tagName);
            if (node != null)
            {
                return node.Value.Trim();
            }
        }

        return null;
    }

    private XPathNavigator GetTypeNode(Type type)
    {
        string controllerTypeName = GetTypeName(type);
        string selectExpression = string.Format(CultureInfo.InvariantCulture, TypeExpression, controllerTypeName);
        return _documentNavigator.SelectSingleNode(selectExpression);
    }

    private static string GetTypeName(Type type)
    {
        string name = type.FullName;
        if (type.IsGenericType)
        {
            // Format the generic type name to something like: Generic{System.Int32,System.String}
            Type genericType = type.GetGenericTypeDefinition();
            Type[] genericArguments = type.GetGenericArguments();
            string genericTypeName = genericType.FullName;

            // Trim the generic parameter counts from the name
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
            string[] argumentTypeNames = genericArguments.Select(t => GetTypeName(t)).ToArray();
            name = string.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", genericTypeName, string.Join(",", argumentTypeNames));
        }
        if (type.IsNested)
        {
            // Changing the nested type name from OuterType+InnerType to OuterType.InnerType to match the XML documentation syntax.
            name = name?.Replace("+", ".");
        }

        return name;
    }
    #endregion
}
