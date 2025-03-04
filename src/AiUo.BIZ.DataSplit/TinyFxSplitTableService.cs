using Microsoft.CodeAnalysis.CSharp.Syntax;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AiUo.BIZ.DataSplit.Caching;
using AiUo.BIZ.DataSplit.DAL;
using AiUo.Reflection;
using AiUo.Text;

namespace AiUo.BIZ.DataSplit
{
    public class TinyFxSplitTableService : ISplitTableService
    {
        //返回数据库中所有分表
        public List<SplitTableInfo> GetAllTables(ISqlSugarClient db, EntityInfo entityInfo, List<DbTableInfo> tableInfos)
        {
            var ret = new List<SplitTableInfo>();
            var data = GetSplitTableData(db, entityInfo);
            foreach (var item in data.GetItems())
            {
                ret.Add(new SplitTableInfo
                {
                    TableName = item.SplitTableName
                });
            }
            ret = ret.OrderBy(it => it.TableName).ToList();
            return ret;
        }
        //获取分表字段的值
        public object GetFieldValue(ISqlSugarClient db, EntityInfo entityInfo, SplitType splitType, object entityValue)
        {
            if (entityValue == null)
                return null;
            var data = GetSplitTableData(db, entityInfo);
            if (!ReflectionUtil.TryGetPropertyValue(entityValue, data.ColumnName, out var ret))
                throw new Exception("Entity没有ColumnName");
            return ret;
        }

        //默认表名
        public string GetTableName(ISqlSugarClient db, EntityInfo entityInfo)
        {
            return entityInfo.DbTableName;
        }

        public string GetTableName(ISqlSugarClient db, EntityInfo entityInfo, SplitType type)
        {
            return entityInfo.DbTableName;
        }

        public string GetTableName(ISqlSugarClient db, EntityInfo entityInfo, SplitType splitType, object fieldValue)
        {
            var fvalue = Convert.ToString(fieldValue);
            var data = GetSplitTableData(db, entityInfo);
            var items = data.GetItems();
            string ret = data.TableName;
            switch (data.HandleMode)
            {
                case HandleMode.Backup:
                    foreach (var item in items.OrderBy(x=>x.EndDate))
                    {
                        if (fvalue.CompareTo(item.EndValue) < 0)
                        {
                            ret = item.SplitTableName; 
                            break;
                        }
                    }
                    break;
                case HandleMode.MaxRows:
                    foreach (var item in items.OrderByDescending(x => x.BeginValue))
                    {
                        if (fvalue.CompareTo(item.BeginValue) >= 0)
                        {
                            ret = item.SplitTableName;
                            break;
                        }
                    }
                    break;
            }
            return ret;
        }

        #region Utils
        private string GetDatabaseId(ISqlSugarClient db)
        {
            var ret = db.Ado.Context.CurrentConnectionConfig.ConfigId;
            return Convert.ToString(ret);
        }
        private SplitTableData GetSplitTableData(ISqlSugarClient db, EntityInfo entityInfo)
        {
            var databaseId = GetDatabaseId(db);
            var srcTableName = entityInfo.DbTableName;
            var ret = DbCacheUtil.GetSplitTableData(databaseId, srcTableName); 
            if (ret == null)
                throw new Exception("没有分表");
            return ret;
        }
        #endregion
    }
}
