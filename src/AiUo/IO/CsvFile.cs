using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.IO;

namespace AiUo.IO;

/// <summary>
/// CSV操作类
/// </summary>
public class CsvFile
{
    /// <summary>
    /// CSV文件
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 字符集
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// 字段间隔
    /// </summary>
    public string FieldTerminator { get; set; } = "\t";

    /// <summary>
    /// 是否有Header
    /// </summary>
    public bool HasHeader { get; set; } = true;

    /// <summary>
    /// 行间隔
    /// </summary>
    public string LineTerminator { get; set; } = "\r\n";

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="csvFile"></param>
    public CsvFile(string csvFile)
        => FileName = csvFile;

    /// <summary>
    /// Headers
    /// </summary>
    public Dictionary<string, (int Index, string Name)> Headers = new Dictionary<string, (int Index, string Name)>();
    /// <summary>
    /// 按Index排序后的HeaderNames
    /// </summary>
    public List<string> HeaderNames
    {
        get
        {
            var values = Headers.Values.ToList();
            Comparison<(int index, string name)> comparison = new Comparison<(int index, string name)>(
                ((int index, string name) x, (int index, string name) y) => {
                    return x.index.CompareTo(y.index);
                });
            values.Sort(comparison);
            return values.Select(item => item.Name).ToList();
        }
    }
    /// <summary>
    /// 添加Header
    /// </summary>
    /// <param name="name"></param>
    /// <param name="index"></param>
    public void AddHeader(string name, int index=-1)
    {
        index = (index == -1) ? Headers.Count : index;
        Headers.Add(name, (index, name));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="convertor"></param>
    public void Write(IDataReader reader, Func<string, object, string> convertor = null)
    {
        using (StreamWriter writer = new StreamWriter(FileName, false, Encoding))
        {
            // Headers
            if (HasHeader)
            {
                Headers.Clear();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    writer.Write(name);
                    if (i < reader.FieldCount - 1)
                        writer.Write(FieldTerminator);
                    AddHeader(name, i);
                }
                writer.Write(LineTerminator);
            }
            // Values
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string field = reader.GetName(i);
                    object value = reader.GetValue(i);
                    string str = string.Empty;
                    if (convertor != null)
                    {
                        str = convertor(field, value);
                    }
                    else {
                        if (value != DBNull.Value || value != null)
                            str = Convert.ToString(value);
                    }
                    if (!string.IsNullOrEmpty(str))
                        writer.Write(str);
                    if (i < reader.FieldCount - 1)
                        writer.Write(FieldTerminator);
                }
                writer.Write(LineTerminator);
            }
        }
    }
}