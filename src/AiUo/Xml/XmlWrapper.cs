using System;
using System.IO;
using System.Text;
using System.Xml;

namespace AiUo.Xml;

/// <summary>
/// XML文档操作包装类，主要使用xpath
/// </summary>
public class XmlWrapper
{
    /// <summary>
    /// XML文件名
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// XmlDocument
    /// </summary>
    public XmlDocument XmlDoc { get; private set; }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="filename"></param>
    public XmlWrapper(string filename)
    {
        if (!File.Exists(filename))
            throw new FileNotFoundException($"xml文件不存在。{filename}");
        FileName = filename;
        XmlDoc = new XmlDocument();
        // 注意，去除namespace
        using (XmlTextReader reader = new XmlTextReader(filename))
        {
            reader.Namespaces = false;
            XmlDoc.Load(reader);
        }
    }
    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="filename"></param>
    public void Save(string filename = null)
    {
        using (var writer = new XmlTextWriter(filename ?? FileName, Encoding.UTF8))
        {
            writer.Namespaces = false;
            writer.Formatting = Formatting.Indented;
            XmlDoc.DocumentElement.WriteTo(writer);
        }
    }
    private XmlElement GetElement(string xpath, bool checkExist = true)
    {
        var ret = XmlDoc.DocumentElement.SelectSingleNode(xpath) as XmlElement;
        if (checkExist && ret == null)
            throw new Exception($"xml节点不存在. xpath: {xpath}");
        return ret;
    }
    /// <summary>
    /// 添加子节点
    /// </summary>
    /// <param name="xpath"></param>
    /// <param name="nodeName"></param>
    /// <param name="innerText"></param>
    public void AppendChildNode(string xpath, string nodeName, string innerText = null)
    {
        var newNode = XmlDoc.CreateNode("element", nodeName, null);
        newNode.InnerText = innerText;
        var node = GetElement(xpath);
        node.AppendChild(newNode);
    }
    /// <summary>
    /// 设置或添加属性值
    /// </summary>
    /// <param name="xpath"></param>
    /// <param name="attributeName"></param>
    /// <param name="attributeValue"></param>
    public void SetAttribute(string xpath, string attributeName, object attributeValue)
        => GetElement(xpath).SetAttribute(attributeName, Convert.ToString(attributeValue));

    public void SetInnerText(string xpath, string text)
        => GetElement(xpath).InnerText = text;

    /// <summary>
    /// 删除节点
    /// </summary>
    /// <param name="xpath"></param>
    /// <param name="nodeName"></param>
    public void DeleteNode(string xpath, string nodeName)
    {
        var node = GetElement($"{xpath}\\{nodeName}");
        node.ParentNode?.RemoveChild(node);
    }
    /// <summary>
    /// 删除属性
    /// </summary>
    /// <param name="xpath"></param>
    /// <param name="attributeName"></param>
    public void DeleteAttribute(string xpath, string attributeName)
        => GetElement(xpath).RemoveAttribute(attributeName);

    /// <summary>
    /// 获取节点集合
    /// </summary>
    /// <param name="xpath"></param>
    /// <returns></returns>
    public XmlNodeList GetNodes(string xpath)
        => XmlDoc.SelectNodes(xpath);

    /// <summary>
    /// 获取节点InnerText
    /// </summary>
    /// <param name="xpath"></param>
    /// <returns></returns>
    public string GetInnerText(string xpath)
        => GetElement(xpath, false)?.InnerText;

    /// <summary>
    /// 获取节点属性值
    /// </summary>
    /// <param name="xpath"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    public string GetAttributeValue(string xpath, string attributeName)
        => GetElement(xpath, false)?.GetAttribute(attributeName);
}
