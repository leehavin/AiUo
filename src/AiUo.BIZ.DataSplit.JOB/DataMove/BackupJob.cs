using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AiUo.BIZ.DataSplit;
using AiUo.BIZ.DataSplit.DAL;
using AiUo.Data.SqlSugar;
using AiUo.Text;


namespace AiUo.BIZ.DataSplit.JOB.DataMove
{
    internal class BackupJob : BaseDataMoveJob
    {
        public BackupJob(S_split_tablePO item, string defaultConfigId, DateTime execTime) : base(item, defaultConfigId, execTime)
        {
            if ((HandleMode)item.HandleMode != HandleMode.Backup)
                throw new Exception("DataMove.BackupJob时HandleMode必须是Backup");
        }

        protected override async Task ExecuteJob()
        {
            // 执行待删除原始表
            await DeleteWaitData();

            // 执行备份
            var list = await GetBackupList();
            if (list == null || list.Count == 0)
                return;
            var first = list.First();
            var last = list.Last();
            _logEo.BeginDate = first.Begin.Date;
            _logEo.BeginValue = first.Begin.Value;
            _logEo.EndDate = last.End.Date;
            _logEo.EndValue = last.End.Value;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                AddHandleLog($"[任务{i + 1}]-开始备份: [{_item.TableName} => {item.BackupTableName}] [{item.Begin.Value} => {item.End.Value}]");
                await CreateTable(item.BackupTableName);
                await BackupItem(list[i]);
                _logEo.HandleTables += $"{item.BackupTableName}{Environment.NewLine}";
                AddHandleLog($"[任务{i + 1}]-完成备份: [{_item.TableName} => {item.BackupTableName}] [{item.Begin.Value} => {item.End.Value}] count: {_logEo.RowNum} {Environment.NewLine}");
            }
        }
        private async Task<int> BackupItem(BackupData item)
        {
            var watch = new Stopwatch();
            // 1) 获取备份的数据
            AddHandleLog($"==> [{item.BackupTableName}]备份数据开始: {_item.TableName} pageSize:{BATCH_PAGE_SIZE}");
            AddHandleLog($"  1) 读取备份数据开始...");
            var readCount = 0;
            var dtList = new List<DataTable>();
            var selectSql = $"SELECT * FROM `{_item.TableName}`";
            var where = _columnHelper.GetColumnWhere(item.Begin.Value, item.End.Value);
            var pageIndex = 1;
            while (true)
            {
                watch.Restart();
                var dt = GetItemDb().SqlQueryable<object>(selectSql).With(SqlWith.NoLock)
                    .Where(where).ToDataTablePage(pageIndex, BATCH_PAGE_SIZE);
                watch.Stop();
                if (dt == null || dt.Rows.Count == 0)
                    break;
                pageIndex++;
                readCount += dt.Rows.Count;
                dtList.Add(dt);
                AddHandleLog($"  SQL:[{(int)watch.Elapsed.TotalSeconds}秒] {selectSql} pageIndex:{pageIndex} readCount:{readCount}");
            }
            AddHandleLog($"  1) 读取备份数据结束... readCount:{readCount} pageCount: {pageIndex}");
            if (readCount == 0)
                return 0;

            var tm = new DbTransactionManager();
            try
            {
                AddHandleLog($"  ======== 事务开始 ========");

                // 2) 删除备份表旧数据
                AddHandleLog($"  2) 删除备份表旧数据开始: {item.BackupTableName}");
                var oldDeleteSql = $"DELETE FROM `{item.BackupTableName}` WHERE {where}";
                watch.Restart();
                var oldDeleteCount = await GetItemDb(tm).Ado.ExecuteCommandAsync(oldDeleteSql);
                watch.Stop();
                AddHandleLog($"  SQL:[{(int)watch.Elapsed.TotalSeconds}秒] {oldDeleteSql}");
                AddHandleLog($"  2) 删除备份表旧数据结束: {item.BackupTableName} deleteCount: {oldDeleteCount}");

                // 3) 插入备份表数据
                AddHandleLog($"  3) 插入备份表数据开始: {item.BackupTableName}");
                var insertCount = 0;
                foreach (var dt in dtList)
                {
                    watch.Restart();
                    insertCount += await GetItemDb(tm).Fastest<DataTable>().AS(item.BackupTableName).BulkCopyAsync(dt);
                    watch.Stop();
                    AddHandleLog($"  SQL:[{(int)watch.Elapsed.TotalSeconds}秒] INSERT INTO `{item.BackupTableName}` total: {insertCount}");
                    //await Task.Delay(100);
                }
                if (readCount != insertCount)
                    throw new Exception($"DataMove插入时总记录数不相同。fromTable: {_item.TableName} toTable:{item.BackupTableName}");
                AddHandleLog($"  3) 插入备份表数据结束: {item.BackupTableName} insertCount: {insertCount}");

                // 4) 删除原始表SQL保存
                var deleteData = new BackupDeleteData
                {
                    Count = insertCount,
                    SQL = $"DELETE FROM `{_item.TableName}` WHERE {where}"
                };

                _logEo.RowNum += insertCount;

                // 5) 保存detail记录
                var oid = ObjectId.NextId();
                var detailEo = new S_split_table_detailPO
                {
                    DetailID = oid.Id,
                    LogID = _logEo.LogID,
                    DatabaseId = _item.DatabaseId,
                    TableName = _item.TableName,
                    ColumnName = _item.ColumnName,
                    ColumnType = _item.ColumnType,
                    HandleMode = _item.HandleMode,
                    SplitTableName = item.BackupTableName,
                    EndValue = item.End.Value,
                    EndDate = item.End.Date,
                    BeginValue = item.Begin.Value,
                    BeginDate = item.Begin.Date,
                    RowNum = readCount,
                    WaitDeleteData = SerializerUtil.SerializeJson(deleteData),
                    Status = 2, // 状态(0-无效1-有效2-备份待删除)
                    RecDate = oid.UtcDate,
                };
                await GetMainDb(tm).Insertable(detailEo).ExecuteCommandAsync();

                // 提交事务
                AddHandleLog($"  ======== 事务提交 ========");
                tm.Commit();
            }
            catch
            {
                tm.Rollback();
                AddHandleLog($"  ==>事务失败: {_item.TableName} => {item.BackupTableName} count: {readCount}");
                throw;
            }
            AddHandleLog($"==> [{item.BackupTableName}]备份数据结束: rowCount:{readCount} pageSize:{BATCH_PAGE_SIZE} pageCount:{pageIndex}");
            return readCount;

        }

        private async Task DeleteWaitData()
        {
            var list = GetMainDb().Queryable<S_split_table_detailPO>()
                .Where(it => it.DatabaseId == _item.DatabaseId && it.TableName == _item.TableName && it.Status == 2)
                .ToList();
            if (list == null || list.Count == 0)
                return;
            _isRetainLogEo = true;
            for (int i = 0; i < list.Count; i++)
            {
                AddHandleLog($"[任务{i + 1}]-开始删除原始表待删除数据: [{_item.TableName}");
                var item = list[i];
                var data = SerializerUtil.DeserializeJson<BackupDeleteData>(item.WaitDeleteData);
                var deleteCount = 0;
                var watch = new Stopwatch();

                var tm = new DbTransactionManager();
                try
                {
                    while (true)
                    {
                        watch.Restart();
                        var sql = $"{data.SQL} LIMIT {BATCH_PAGE_SIZE}";
                        var rows = await GetItemDb(tm).Ado.ExecuteCommandAsync(sql);
                        watch.Stop();
                        AddHandleLog($"  SQL:[{(int)watch.Elapsed.TotalSeconds}秒] {sql}");

                        if (rows == 0) break;
                        deleteCount += rows;
                    }
                    if (deleteCount != data.Count)
                        throw new Exception($"DataMove删除记录数不等于插入记录数。{_item.TableName} => {item.SplitTableName} insertCount: {data.Count} deleteCount: {deleteCount}");

                    await GetMainDb(tm).Updateable<S_split_table_detailPO>()
                        .SetColumns(it => it.Status == 1)
                        .Where(it => it.DetailID == item.DetailID)
                        .ExecuteCommandAsync();
                    AddHandleLog($"[任务{i + 1}]-结束删除原始表待删除数据: [{_item.TableName} deleteCount: {deleteCount}{Environment.NewLine}");
                    tm.Commit();
                }
                catch
                {
                    tm.Rollback();
                    throw;
                }
            }
        }

        #region GetBackupDataList
        private async Task<List<BackupData>> GetBackupList()
        {
            List<BackupData> list;
            switch ((MoveTableMode)_item.MoveTableMode)
            {
                case MoveTableMode.Day:
                    list = await BuildBackupList((endDate) =>
                    {
                        return endDate.AddDays(-1);
                    });
                    break;
                case MoveTableMode.Week:
                    list = await BuildBackupList((endDate) =>
                    {
                        var date = endDate.AddDays(-1);
                        return DateTimeUtil.BeginDayOfWeek(date);
                    });
                    break;
                case MoveTableMode.Month:
                    list = await BuildBackupList((endDate) =>
                    {
                        var date = endDate.AddDays(-1);
                        return new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    });
                    break;
                case MoveTableMode.Quarter:
                    list = await BuildBackupList((endDate) =>
                    {
                        var date = endDate.AddDays(-1);
                        var quarterMonth = Math.DivRem(date.Month - 1, 3, out int _) * 3 + 1;
                        return new DateTime(date.Year, quarterMonth, 1, 0, 0, 0, DateTimeKind.Utc);
                    });
                    break;
                case MoveTableMode.Year:
                    list = await BuildBackupList((endDate) =>
                    {
                        var date = endDate.AddDays(-1);
                        return new DateTime(date.Year, 1, 1);
                    });
                    break;
                default:
                    throw new Exception($"DataMove不支持此MoveTableMode: {_item.MoveTableMode}");
            }
            return list;
        }
        private async Task<List<BackupData>> BuildBackupList(Func<DateTime, DateTime> calcBeginDate)
        {
            var ret = new List<BackupData>();
            var end = _columnHelper.GetKeepEndValue(_execTime);
            var min = await GetBeginValue(end);
            if (min.Value == null)
                return ret;

            while (end.Date > min.Date)
            {
                var beginDate = calcBeginDate(end.Date);
                var begin = new ColumnValue();
                begin.Date = beginDate < min.Date ? min.Date : beginDate;
                begin.Value = _columnHelper.ColumnDateToValue(begin.Date);
                var item = new BackupData
                {
                    BackupTableName = $"{_item.TableName}_{beginDate:yyyyMMdd}",
                    Begin = begin,
                    End = end
                };
                ret.Add(item);
                end = begin;
            }
            ret = ret.OrderBy(x => x.Begin.Date).ToList();
            return ret;
        }
        #endregion
    }
    internal class BackupData
    {
        public string BackupTableName { get; set; }
        public ColumnValue Begin { get; set; }
        public ColumnValue End { get; set; }
    }
    internal class BackupDeleteData
    {
        public int Count { get; set; }
        public string SQL { get; set; }
    }
}
