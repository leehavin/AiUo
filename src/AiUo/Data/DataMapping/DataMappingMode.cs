namespace AiUo.Data;

/// <summary>
/// 数据与实体对象映射方式
/// </summary>
public enum DataMappingMode
{
    /// <summary>
    /// 实体对象继承接口实现数据映射
    /// </summary>
    Interface,
    /// <summary>
    /// 实体对象通过定义DataColumnMapperAttribute实现映射
    /// </summary>
    Attribute,
    /// <summary>
    /// 实体对象通过反射属性名实现映射
    /// </summary>
    Reflection,
    /// <summary>
    /// 映射成基元类型，只支持单值映射
    /// </summary>
    PrimitiveType
}