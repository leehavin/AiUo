using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AiUo.IO;

/// <summary>
/// 文件内容替换器
/// </summary>
public class TextReplacer
{

    #region core
    private string _file;
    private string _content;
    private Encoding _encoding;
    public TextReplacer(string file = null, Encoding encoding = null)
    {
        _file = file;
        // 注意：不能使用Encoding.UTF8！linux不支持utf8 DOM
        _encoding = encoding ?? new UTF8Encoding(false);
        if (!string.IsNullOrEmpty(file))
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"{file}文件不存在");
            _content = File.ReadAllText(file, _encoding);
        }
    }
    public void Load(string content)
    {
        _content += content;
    }
    #endregion

    public TextReplacer Replace(string src, object desc)
    {
        _content = _content.Replace(src, Convert.ToString(desc));
        return this;
    }
    public TextReplacer AppendLine(string line)
    {
        _content += $"{line}{Environment.NewLine}";
        return this;
    }
    public TextReplacer InsertLine(string line)
    {
        _content = $"{line}{Environment.NewLine}" + _content;
        return this;
    }
    /// <summary>
    /// 添加或更新行（更新根据该行是否包含mark）
    /// </summary>
    /// <param name="mark"></param>
    /// <param name="newLine"></param>
    /// <param name="ignoreStart"></param>
    /// <returns></returns>
    public TextReplacer AppendOrUpdateLine(string mark, string newLine, string ignoreStart="#")
    {
        var lines = _content.SplitNewLine().ToList();
        var isUpdate = false;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (!string.IsNullOrEmpty(ignoreStart) && line.StartsWith(ignoreStart))
                continue;
            if (line.Contains(mark))
            {
                if (isUpdate)
                    throw new Exception($"TextReplacer.AppendOrUpdateLine()时mark标识查找到多行记录。file:{_file} mark:{mark}");
                lines[i] = newLine;
                isUpdate = true;
            }
        }
        if(!isUpdate)
        {
            lines.Add(newLine);
        }
        _content = string.Join(Environment.NewLine, lines);
        return this;
    }
    public void Save(string descFile=null, Encoding encoding = null)
    {
        descFile = descFile ?? _file;
        if (string.IsNullOrEmpty(descFile))
            throw new Exception("必须指定保存文件名");
        File.WriteAllText(descFile, _content, encoding ?? _encoding);
    }
}