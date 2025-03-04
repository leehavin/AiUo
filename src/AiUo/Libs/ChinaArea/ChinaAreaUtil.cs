using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using AiUo.IO;

namespace AiUo.ChinaArea;

/// <summary>
/// 中国区域信息辅助类，获取省市区相关信息
/// Level：
///     0-根
///     1-省，自治区，直辖市，特别行政区
///     2-市，地区，自治州，盟，直辖市所属市辖区和县
///     3-县，市辖区，县级市，旗
///     4-乡镇（街道办事处）
/// </summary>
public static class ChinaAreaUtil
{
    private const string RESOURCE_NAME = "AiUo.Libs.ChinaArea.ChinaAreaData.7z";

    private static Dictionary<int, ChinaAreaInfo> _areaIDCache = new Dictionary<int, ChinaAreaInfo>();
    private static Dictionary<string, List<ChinaAreaInfo>> _nameCache = new Dictionary<string, List<ChinaAreaInfo>>();
    private static Dictionary<string, ChinaAreaInfo> _aliasCache = new Dictionary<string, ChinaAreaInfo>();
    private static List<ChinaAreaInfo> _provinceCache = new List<ChinaAreaInfo>();
    private static Dictionary<int, List<ChinaAreaInfo>> _cityCache = new Dictionary<int, List<ChinaAreaInfo>>();
    private static Dictionary<int, List<ChinaAreaInfo>> _townCache = new Dictionary<int, List<ChinaAreaInfo>>();
    // 电话区号
    private static Dictionary<string, List<ChinaAreaInfo>> _cityCodeCache = new Dictionary<string, List<ChinaAreaInfo>>();
    private static object _locker = new object();

    #region Init
    // 获取嵌入资源
    private static IEnumerable<ChinaAreaInfo> GetManifestItems()
    {
        var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME);
        var lines = Encoding.UTF8.GetString(CompressionUtil.UnSevenZipToBytes(resourceStream));
        foreach (var line in lines.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
        {
            if (string.IsNullOrEmpty(line)) continue;
            var items = line.Split('\t');
            yield return new ChinaAreaInfo()
            {
                AreaID = items[0].ToInt32(),
                Name = items[1],
                ParentId = items[2].ToInt32(),
                ShortName = items[3],
                Level = items[4].ToInt32(),
                CityCode = items[5],
                ZipCode = items[6],
                MergerName = items[7],
                Lng = items[8].ToDouble(),
                Lat = items[9].ToDouble(),
                Pinyin = items[10],
                Jianpin = items[11],
                Alias = items[12],
                OtherAlias = items[13],
                IsDirect = items[14].ToBoolean(),
                Status = items[15].ToInt32()
            };
        }
    }

    /// <summary>
    /// 初始化区域数据。如果使用自定义区域数据，使用此函数初始化
    /// </summary>
    /// <param name="items"></param>
    public static void InitChinaAreaData(IEnumerable<ChinaAreaInfo> items)
    {
        if (items == null) return;
        lock (_locker)
        {
            _areaIDCache.Clear();
            _nameCache.Clear();
            _aliasCache.Clear();
            _provinceCache.Clear();
            _cityCache.Clear();
            _townCache.Clear();
            _cityCodeCache.Clear();
            foreach (var item in items)
            {
                _areaIDCache.Add(item.AreaID, item);
                if (!string.IsNullOrEmpty(item.ShortName))
                {
                    if (!_nameCache.ContainsKey(item.ShortName))
                        _nameCache.Add(item.ShortName, new List<ChinaAreaInfo>() { item });
                    else
                        _nameCache[item.ShortName].Add(item);
                }
                if (!string.IsNullOrEmpty(item.Alias))
                    _aliasCache.Add(item.Alias, item);
                if (!string.IsNullOrEmpty(item.OtherAlias))
                    _aliasCache.Add(item.OtherAlias, item);
                if (item.Level == 1)
                    _provinceCache.Add(item);
                if (item.Level == 2)
                {
                    if (!_cityCache.ContainsKey(item.ParentId))
                        _cityCache.Add(item.ParentId, new List<ChinaAreaInfo>() { item });
                    else
                        _cityCache[item.ParentId].Add(item);
                }
                if (item.Level == 3)
                {
                    if (!_townCache.ContainsKey(item.ParentId))
                        _townCache.Add(item.ParentId, new List<ChinaAreaInfo>() { item });
                    else
                        _townCache[item.ParentId].Add(item);
                }
                if (!_cityCodeCache.ContainsKey(item.CityCode))
                    _cityCodeCache.Add(item.CityCode, new List<ChinaAreaInfo>() { item });
                else
                    _cityCodeCache[item.CityCode].Add(item);
            }
        }
    }
    // 静态构造函数
    static ChinaAreaUtil()
    {
        InitChinaAreaData(GetManifestItems());
    }
    #endregion // Init

    #region Methods
    /// <summary>
    /// 获取所有区域数据
    /// </summary>
    /// <returns></returns>
    public static List<ChinaAreaInfo> GetAll()
        => _areaIDCache.Values.ToList();
        
    /// <summary>
    /// 根据行政区划码获地区信息
    /// </summary>
    /// <param name="areaId">行政区划码</param>
    /// <returns></returns>
    public static ChinaAreaInfo GetById(int areaId)
        => _areaIDCache.ContainsKey(areaId) ? _areaIDCache[areaId] : null;

    /// <summary>
    /// 根据名称获地区域信息
    /// </summary>
    /// <param name="shortName">地区名称</param>
    /// <returns></returns>
    public static List<ChinaAreaInfo> GetByName(string shortName)
        => _nameCache.ContainsKey(shortName) ? _nameCache[shortName] : null;
        
    /// <summary>
    /// 根据别名获取地区信息
    /// </summary>
    /// <param name="alias">省市别名</param>
    /// <returns></returns>
    public static ChinaAreaInfo GetByAlias(string alias)
        => _aliasCache.ContainsKey(alias) ? _aliasCache[alias] : null;

    /// <summary>
    /// 根据城市代码（电话区号）获取地区信息
    /// </summary>
    /// <param name="cityCode"></param>
    /// <returns></returns>
    public static List<ChinaAreaInfo> GetByCityCode(string cityCode)
        => _cityCodeCache.ContainsKey(cityCode) ? _cityCodeCache[cityCode] : null;

    /// <summary>
    /// 获取区域信息的省，自治区，直辖市，特别行政区信息
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    public static ChinaAreaInfo GetParentProvince(ChinaAreaInfo child)
    {
        ChinaAreaInfo ret = null;
        switch (child.Level)
        {
            case 1:
                ret = child;
                break;
            case 2:
                ret = GetById(child.ParentId);
                break;
            case 3:
                ret = GetById(GetById(child.ParentId).ParentId);
                break;
            case 4:
                ret = GetById(GetById(GetById(child.ParentId).ParentId).ParentId);
                break;
        }
        return ret;
    }

    /// <summary>
    /// 获取区域的市，地区，自治州，盟，直辖市所属市辖区和县信息
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    public static ChinaAreaInfo GetParentCity(ChinaAreaInfo child)
    {
        if (child.Level < 2)
            throw new Exception("ChinaAreaUtil.GetParentCity()城市不支持二级一下信息");
        ChinaAreaInfo ret = null;
        switch (child.Level)
        {
            case 2:
                ret = child;
                break;
            case 3:
                ret = GetById(child.ParentId);
                break;
            case 4:
                ret = GetById(GetById(child.ParentId).ParentId);
                break;
        }
        return ret;
    }

    /// <summary>
    /// 获取所有省，自治区，直辖市，特别行政区
    /// </summary>
    /// <returns></returns>
    public static List<ChinaAreaInfo> GetProvinces()
        => _provinceCache;

    /// <summary>
    /// 获取省内的市，地区，自治州，盟，直辖市所属市辖区和县
    /// </summary>
    /// <param name="provinceId"></param>
    /// <returns></returns>
    public static List<ChinaAreaInfo> GetCities(int provinceId)
        => _cityCache.ContainsKey(provinceId) ? _cityCache[provinceId] : null;
        
    /// <summary>
    /// 获取市内的县，市辖区，县级市，旗
    /// </summary>
    /// <param name="cityId"></param>
    /// <returns></returns>
    public static List<ChinaAreaInfo> GetTowns(int cityId)
        => _townCache.ContainsKey(cityId) ? _townCache[cityId] : null;
    #endregion
}