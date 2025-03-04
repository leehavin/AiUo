using SqlSugar;

namespace AiUo.Data.SqlSugar;

public class BaseSplitTableService : ISplitTableService
{
    public List<SplitTableInfo> GetAllTables(ISqlSugarClient db, EntityInfo EntityInfo, List<DbTableInfo> tableInfos)
    {
        var currentTables = tableInfos.Where(it => it.Name.StartsWith("abc"))
            .Select(x=>x.Name).Reverse().ToList();
        var ret = new List<SplitTableInfo>();
        foreach (var item in currentTables)
        {
            var tableInfo = new SplitTableInfo();
            tableInfo.TableName = item;
            tableInfo.Long = 100;
            ret.Add(tableInfo);
        }
        ret = ret.OrderByDescending(it => it.Long).ToList();
        return ret;
    }

    public object GetFieldValue(ISqlSugarClient db, EntityInfo entityInfo, SplitType splitType, object entityValue)
    {
        throw new NotImplementedException();
    }

    public string GetTableName(ISqlSugarClient db, EntityInfo EntityInfo)
    {
        throw new NotImplementedException();
    }

    public string GetTableName(ISqlSugarClient db, EntityInfo EntityInfo, SplitType type)
    {
        throw new NotImplementedException();
    }

    public string GetTableName(ISqlSugarClient db, EntityInfo entityInfo, SplitType splitType, object fieldValue)
    {
        throw new NotImplementedException();
    }
}