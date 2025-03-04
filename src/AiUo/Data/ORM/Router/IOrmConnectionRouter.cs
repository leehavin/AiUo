using System;

namespace AiUo.Data.ORM.Router;

/// <summary>
/// Connection路由顺序:
///     IOrmConnectionRouter => OrmMap => default
/// </summary>
public interface IOrmConnectionRouter
{
    /// <summary>
    /// 返回ConnectionStringName，如果返回null则不路由
    /// </summary>
    /// <param name="ormType"></param>
    /// <param name="sourceName"></param>
    /// <returns></returns>
    string Route(Type ormType, string sourceName);
}