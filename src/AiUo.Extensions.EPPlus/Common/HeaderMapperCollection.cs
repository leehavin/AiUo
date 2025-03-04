using System;
using System.Collections;
using System.Collections.Generic;

namespace AiUo.Extensions.EPPlus;

public class HeaderMapperCollection<TMapper> : IEnumerable<TMapper>
    where TMapper : HeaderMapperBase
{
    private List<TMapper> _list = new List<TMapper>();
    private bool _isComplate = false;
    private Dictionary<int, TMapper> _indexs = new Dictionary<int, TMapper>();
    private Dictionary<string, TMapper> _mapNames = new Dictionary<string, TMapper>();
    private Dictionary<string, TMapper> _titles = new Dictionary<string, TMapper>();

    public void Add(TMapper mapper)
    {
        if (mapper.ColumnIndex < 1 && string.IsNullOrEmpty(mapper.Title))
            throw new Exception($"定义{typeof(TMapper).Name}类型时，ColumnIndex>0或Title不为空");
        _list.Add(mapper);
    }
    public int Count => _list.Count;
    public bool ContainsColumnIndex(int columnIndex) => TryGetByColumnIndex(columnIndex, out _);
    public bool ContainsMapName(string mapName) => TryGetByMapName(mapName, out _);
    public bool ContainsTitle(string title) => TryGetByTitle(title, out _);
    public bool TryGetByColumnIndex(int columnIndex, out TMapper mapper)
    {
        mapper = _isComplate ? _indexs[columnIndex]
            : _list.Find(item => item.ColumnIndex > 0 && item.ColumnIndex == columnIndex);
        return mapper != null;
    }
    public bool TryGetByMapName(string mapName, out TMapper mapper)
    {
        mapper = _isComplate ? _mapNames[mapName]
            : _list.Find(item => !string.IsNullOrEmpty(item.MapName) && item.MapName == mapName);
        return mapper != null;
    }
    public bool TryGetByTitle(string title, out TMapper mapper)
    {
        mapper = _isComplate ? _titles[title]
            : _list.Find(item => !string.IsNullOrEmpty(item.Title) && item.Title == title);
        return mapper != null;
    }
    public TMapper GetByListIndex(int listIndex)
        => _list[listIndex];
    public Dictionary<string, TMapper> GetMapNameDic()
    {
        var ret = new Dictionary<string, TMapper>();
        foreach (var item in _list)
            ret.Add(item.MapName, item);
        return ret;
    }
    public void Clear() => _list.Clear();
    public void CheckAndComplete()
    {
        if (_list.Count == 0)
            throw new Exception("执行CheckAndComplete方法时，Headers不能空");
        _list.Sort();
        _indexs.Clear();
        _mapNames.Clear();
        _titles.Clear();
        foreach (var item in _list)
        {
            if (item.ColumnIndex < 1 || string.IsNullOrEmpty(item.Title) || string.IsNullOrEmpty(item.MapName))
                throw new Exception($"HeaderMappers定义时ColumnIndex,Title,MapName都需要定义");
            if (_indexs.ContainsKey(item.ColumnIndex))
                throw new Exception($"CheckAndComplete时，Headers中的ColumnIndex不能重复。ColumnIndex:{item.ColumnIndex}");
            _indexs.Add(item.ColumnIndex, item);
            if (_mapNames.ContainsKey(item.MapName))
                throw new Exception($"CheckAndComplete时，定义的Headers中的MapName不能重复。MapName:{item.MapName}");
            _mapNames.Add(item.MapName, item);
            if (_titles.ContainsKey(item.Title))
                throw new Exception($"CheckAndComplete时，定义的Headers中的Title不能重复。Title:{item.Title}");
            _titles.Add(item.Title, item);
        }
        _isComplate = true;
    }

    public IEnumerator<TMapper> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }
}