using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiUo.BIZ.DataSplit.DAL;
using AiUo.Data;
using AiUo.DbCaching;

namespace AiUo.BIZ.DataSplit.Caching
{
    internal class DbCacheUtil
    {
        public static SplitTableData GetSplitTableData(string databaseId, string tableName)
        {
            var dict = DbCachingUtil.GetOrAddCustom<S_split_table_detailPO, Dictionary<string, SplitTableData>>("DICT_SPLIT_TABLE_DATA", (items) =>
            {
                var values = new Dictionary<string, SplitTableData>();
                foreach (var item in items)
                {
                    if (item.Status != 1 && item.Status != 2)
                        continue;
                    var dictKey = $"{item.DatabaseId}|{item.TableName}";
                    if (values.ContainsKey(dictKey))
                    {
                        values[dictKey].AddItem(new SplitTableItem
                        {
                            SplitTableName = item.SplitTableName,
                            BeginValue = item.BeginValue,
                            BeginDate = item.BeginDate,
                            EndValue = item.EndValue,
                            EndDate = item.EndDate,
                        });
                    }
                    else
                    {
                        var v = new SplitTableData()
                        {
                            DatabaseId = item.DatabaseId,
                            TableName = item.TableName,
                            HandleMode = (HandleMode)item.HandleMode,
                            ColumnType = (ColumnType)item.ColumnType,
                            ColumnName = item.ColumnName,
                        };
                        v.AddItem(new SplitTableItem
                        {
                            SplitTableName = item.SplitTableName,
                            BeginValue = item.BeginValue,
                            BeginDate = item.BeginDate,
                            EndValue = item.EndValue,
                            EndDate = item.EndDate,
                        });
                        values.Add(dictKey, v);
                    }
                }
                return values;
            });
            var key = $"{databaseId}|{tableName}";
            return dict.ContainsKey(key)
                ? dict[key]
                : null;
        }
    }
    public class SplitTableData
    {
        public string DatabaseId { get; set; }
        public string TableName { get; set; }
        public HandleMode HandleMode { get; set; }
        public ColumnType ColumnType { get; set; }
        public string ColumnName { get; set; }
        public Dictionary<string, SplitTableItem> ItemDict { get; set; } = new();

        public void AddItem(SplitTableItem item)
        {
            if (ItemDict.ContainsKey(item.SplitTableName))
            {
                var curr = ItemDict[item.SplitTableName];
                if (item.BeginValue.CompareTo(curr.BeginValue) < 0)
                {
                    curr.BeginValue = item.BeginValue;
                    curr.BeginDate = item.BeginDate;
                }
                if (item.EndValue.CompareTo(curr.EndValue) > 0)
                {
                    curr.EndValue = item.EndValue;
                    curr.EndDate = item.EndDate;
                }

            }
            else
            {
                ItemDict.Add(item.SplitTableName, item);
            }
        }

        private List<SplitTableItem> _items;
        public List<SplitTableItem> GetItems()
        {
            if (_items == null)
            {
                _items = ItemDict.Values.ToList().OrderBy(x => x.SplitTableName).ToList();
            }
            return _items;
        }
    }
    public class SplitTableItem
    {
        public string SplitTableName { get; set; }
        public string BeginValue { get; set; }
        public DateTime? BeginDate { get; set; }
        public string EndValue { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
