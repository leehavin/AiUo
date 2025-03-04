using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;

namespace AiUo;

/// <summary>
/// 枚举类型的描述信息
/// </summary>
public class EnumInfo
{
    /// <summary>
    /// 枚举类型信息
    /// </summary>
    public Type EnumType { get; private set; }

    /// <summary>
    /// 枚举名称
    /// </summary>
    public string Name => EnumType.Name;

    /// <summary>
    /// Enum类型上添加DescriptionAttribute中定义的描述
    /// </summary>
    public string Description { get; set; }

    private Dictionary<int, EnumItem> _itemsInt;
    private Dictionary<string, EnumItem> _itemsStr;
    private Dictionary<string, int> _itemsMapDic;
    /// <summary>
    /// 枚举值获取
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public EnumItem this[int enumValue] =>_itemsInt[enumValue];
    /// <summary>
    /// 枚举字符串获取
    /// </summary>
    /// <param name="enumName"></param>
    /// <returns></returns>
    public EnumItem this[string enumName] =>_itemsStr[enumName];

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Exist(int value) => _itemsInt.ContainsKey(value);
    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool Exist(string name) => _itemsStr.ContainsKey(name);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="enumType"></param>
    public EnumInfo(Type enumType)
    {
        EnumType = enumType;
        var enumAttr = Attribute.GetCustomAttribute(EnumType, typeof(DescriptionAttribute)) as DescriptionAttribute;
        Description = (enumAttr != null) ? enumAttr.Description : null;
        var fields = EnumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        _itemsInt = new Dictionary<int, EnumItem>();
        _itemsStr = new Dictionary<string, EnumItem>();
        _itemsMapDic = new Dictionary<string, int>();
        foreach (var field in fields)
        {
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            var mapName = (Attribute.GetCustomAttribute(field, typeof(EnumMapAttribute)) as EnumMapAttribute)?.MapName;
            var item = new EnumItem()
            {
                Name = field.Name,
                MapName = mapName,
                Value = (int)Enum.Parse(EnumType, field.Name),
                Description = (attr != null) ? attr.Description : null,
                FieldInfo = field
            };
            _itemsInt.Add(item.Value, item);
            _itemsStr.Add(item.Name, item);
            if(!string.IsNullOrEmpty(mapName))
                _itemsMapDic.Add(mapName, item.Value);
        }
    }

    /// <summary>
    /// 获取枚举项列表
    /// </summary>
    /// <returns></returns>
    public List<EnumItem> GetList() => _itemsInt.Values.ToList();

    /// <summary>
    /// 根据枚举值获取枚举项
    /// </summary>
    /// <param name="enumValue"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetItem(int enumValue, out EnumItem value)
        => _itemsInt.TryGetValue(enumValue, out value);

    /// <summary>
    /// 根据枚举值获取枚举项
    /// </summary>
    /// <param name="enumValue">枚举值</param>
    /// <returns></returns>
    public EnumItem GetItem(int enumValue)
        => TryGetItem(enumValue, out EnumItem ret) ? ret : null;

    /// <summary>
    /// 根据枚举名获取枚举项
    /// </summary>
    /// <param name="enumName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetItem(string enumName, out EnumItem value)
        => _itemsStr.TryGetValue(enumName, out value);
        
    /// <summary>
    /// 根据枚举名获取枚举项，不存在返回Null
    /// </summary>
    /// <param name="enumName"></param>
    /// <returns></returns>
    public EnumItem GetItem(string enumName)
        => TryGetItem(enumName, out EnumItem ret) ? ret : null;

    /// <summary>
    /// 获取枚举项信息
    /// </summary>
    /// <param name="value">枚举值</param>
    /// <returns></returns>
    public EnumItem GetItem(object value)
    {
        int key = (value is int || value.GetType() == EnumType) ? (int)value 
            : (int)Enum.Parse(EnumType, Convert.ToString(value), true);
        return GetItem(key);
    }

    public bool TryGetItemByMap(string mapName, out int value)
        => _itemsMapDic.TryGetValue(mapName, out value);
}