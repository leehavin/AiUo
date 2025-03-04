namespace AiUo.ChinaArea;

/// <summary>
/// 中国区域信息，包含省市区信息
/// </summary>
public class ChinaAreaInfo
{
    /// <summary>
    /// 行政区划码
    /// </summary>
    public int AreaID { get; set; }
        
    /// <summary>
    /// 省市名称，如北京市
    /// </summary>
    public string Name { get; set; }
        
    /// <summary>
    /// 父级编码(行政区划码,0表示根)
    ///          
    /// </summary>
    public int ParentId { get; set; }
        
    /// <summary>
    /// 名称简写，如北京
    /// </summary>
    public string ShortName { get; set; }
        
    /// <summary>
    /// 级别
    ///             0-根
    ///             1-省，自治区，直辖市，特别行政区
    ///             2-市，地区，自治州，盟，直辖市所属市辖区和县
    ///             3-县，市辖区，县级市，旗
    ///             4-乡镇（街道办事处）
    /// </summary>
    public int Level { get; set; }
        
    /// <summary>
    /// 城市代码（电话区号）
    /// </summary>
    public string CityCode { get; set; }
        
    /// <summary>
    /// 邮政编码
    /// </summary>
    public string ZipCode { get; set; }
        
    /// <summary>
    /// 长名称，如中国,河北省,石家庄市,平山县
    /// </summary>
    public string MergerName { get; set; }
        
    /// <summary>
    /// 经度
    /// </summary>
    public double Lng { get; set; }
        
    /// <summary>
    /// 纬度
    /// </summary>
    public double Lat { get; set; }
        
    /// <summary>
    /// 拼音
    /// </summary>
    public string Pinyin { get; set; }
        
    /// <summary>
    /// 简拼
    /// </summary>
    public string Jianpin { get; set; }
        
    /// <summary>
    /// 别名
    /// </summary>
    public string Alias { get; set; }
        
    /// <summary>
    /// 其他别名
    /// </summary>
    public string OtherAlias { get; set; }

    /// <summary>
    /// 是否是直辖市
    /// </summary>
    public bool IsDirect { get; set; }
        
    /// <summary>
    /// 有效状态
    ///             0-有效
    ///             1-无效
    ///             2-变更
    ///             3-删除
    /// </summary>
    public int Status { get; set; }
}