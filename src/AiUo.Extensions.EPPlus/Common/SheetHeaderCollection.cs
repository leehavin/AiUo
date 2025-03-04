using System.Collections;
using System.Collections.Generic;

namespace AiUo.Extensions.EPPlus;

/// <summary>
/// Sheet 中的 Headers 信息, ColumnIndex从1开始
/// </summary>
public class SheetHeaderCollection : IEnumerable<(int ColumnIndex, string Title)>
{
    private SortedDictionary<int, string> _indexs = new SortedDictionary<int, string>();
    private Dictionary<string, int> _titles = new Dictionary<string, int>();
    public int this[string title] => _titles[title];
    public string this[int columnIndex] => _indexs[columnIndex];
    public int Count => _indexs.Count;

    public bool Contains(string title) => _titles.ContainsKey(title);
    public bool Contains(int columnIndex) => _indexs.ContainsKey(columnIndex);
    public void Add(int columnIndex, string title)
    {

        _indexs.Add(columnIndex, title);
        _titles.Add(title, columnIndex);
    }
    public void Remove(int columnIndex)
    {
        _titles.Remove(_indexs[columnIndex]);
        _indexs.Remove(columnIndex);
    }
    public void Remove(string title)
    {
        _indexs.Remove(_titles[title]);
        _titles.Remove(title);
    }
    public void Clear()
    {
        _indexs.Clear();
        _titles.Clear();
    }

    public IEnumerator<(int ColumnIndex, string Title)> GetEnumerator()
    {
        foreach (var item in _indexs)
            yield return (item.Key, item.Value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _indexs.GetEnumerator();
    }

}