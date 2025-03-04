using Org.BouncyCastle.Ocsp;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiUo.BIZ.DataSplit;
using AiUo.BIZ.DataSplit.DAL;
using AiUo.Data.SqlSugar;
using AiUo.Logging;
using AiUo.Text;

namespace AiUo.BIZ.DataSplit.JOB.DataMove
{
    internal abstract class BaseDataMoveJob
    {
        public int DB_TIMEOUT_SECONDS = 1800; //
        public int BATCH_PAGE_SIZE = 1000000; //100万

        protected ILogBuilder _logger;
        protected S_split_tablePO _item;
        protected DateTime _execTime;
        private string _defaultConfigId;

        protected S_split_table_logPO _logEo;
        protected bool _isRetainLogEo = false;
        protected ColumnValueHelper _columnHelper;
        public BaseDataMoveJob(S_split_tablePO item, string defaultConfigId, DateTime execTime)
        {
            _logger = new LogBuilder("DataMove")
                .AddField("DataMove.Option", item);
            _item = item;
            _execTime = execTime;
            _defaultConfigId = defaultConfigId;

            if (item.DbTimeout > 0)
                DB_TIMEOUT_SECONDS = item.DbTimeout;
            if (item.BathPageSize > 0)
                BATCH_PAGE_SIZE = item.BathPageSize;

            _columnHelper = new ColumnValueHelper(item);
        }
        #region GetDb
        protected ISqlSugarClient GetMainDb(DbTransactionManager tm = null)
            => GetDb(tm, _defaultConfigId);
        protected ISqlSugarClient GetItemDb(DbTransactionManager tm = null)
            => GetDb(tm, _item.DatabaseId);
        private ISqlSugarClient GetDb(DbTransactionManager tm, string configId)
        {
            var ret = tm == null ? DbUtil.GetDbById(configId) : tm.GetDbById(configId);
            ret.Ado.CommandTimeOut = DB_TIMEOUT_SECONDS;
            return ret;
        }
        #endregion

        protected abstract Task ExecuteJob();

        public async Task Execute()
        {
            // 有正在执行的任务就退出
            var oldLogEo = GetMainDb().Queryable<S_split_table_logPO>()
                .Where(it => it.DatabaseId == _item.DatabaseId && it.TableName == _item.TableName && it.Status == 0)
                .Where("DATE_FORMAT(recdate,'%Y-%m-%d')=DATE_FORMAT(UTC_DATE(),'%Y-%m-%d')")
                .ToList();
            if (oldLogEo?.Count > 0)
            {
                LogUtil.Debug($"DataMove时当天有正在执行的任务。databaseId:{_item.DatabaseId} tableName:{_item.TableName}");
                return;
            }

            await InsertLogEo();
            var sw = new Stopwatch();
            sw.Start();
            var logRo = new Repository<S_split_table_logPO>(GetMainDb());
            try
            {
                if (!GetItemDb().DbMaintenance.IsAnyTable(_item.TableName))
                    throw new Exception($"DataMove数据表不存在。databaseId:{_item.DatabaseId} tableName:{_item.TableName}");
                await ExecuteJob();
                sw.Stop();

                _logEo.Status = 1;
                _logEo.HandleSeconds = (int)sw.Elapsed.TotalSeconds;
                if (_logEo.RowNum == 0 && !_isRetainLogEo)
                    await logRo.DeleteByIdAsync(_logEo.LogID);
                else
                    await logRo.UpdateAsync(_logEo);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.AddMessage("执行数据备份迁移出现异常")
                    .AddField("DataMove.ElaspedTime", sw.ElapsedMilliseconds)
                    .AddField("DataMove.LogEo", _logEo)
                    .AddException(ex);

                _logEo.Status = 2;
                _logEo.HandleSeconds = (int)sw.Elapsed.TotalSeconds;
                _logEo.Exception += SerializerUtil.SerializeJsonNet(ex);
                await logRo.UpdateAsync(_logEo);
            }
            _logger.Save();
        }
        private async Task InsertLogEo()
        {
            var oid = ObjectId.NextId();
            _logEo = new S_split_table_logPO()
            {
                LogID = oid.Id,
                DatabaseId = _item.DatabaseId,
                TableName = _item.TableName,
                ColumnName = _item.ColumnName,
                ColumnType = _item.ColumnType,
                HandleMode = _item.HandleMode,
                MoveKeepMode = _item.MoveKeepMode,
                MoveKeepValue = _item.MoveKeepValue,
                MoveTableMode = _item.MoveTableMode,
                MoveWhere = _item.MoveWhere,
                MaxRowCount = _item.MaxRowCount,
                MaxRowInterval = _item.MaxRowInterval,
                HandleOrder = _item.HandleOrder,
                DbTimeout = DB_TIMEOUT_SECONDS,
                BathPageSize = BATCH_PAGE_SIZE,
                ExecTime = _execTime,
                Status = 0, //状态 0-运行中1-成功2-失败
                RecDate = oid.UtcDate, //当天仅运行一条
                RowNum = 0,
                HandleLog = string.Empty,
                Exception = string.Empty,
                HandleTables = string.Empty,
            };
            await GetMainDb().Insertable(_logEo).ExecuteCommandAsync();
        }

        protected async Task CreateTable(string backTableName, DbTransactionManager tm = null)
        {
            var db = GetItemDb(tm);
            if (db.DbMaintenance.IsAnyTable(backTableName))
                return;
            var createSql = $"CREATE TABLE if not exists `{backTableName}` like `{_item.TableName}`";
            AddHandleLog($"==> 创建备份表SQL: {createSql}");
            await db.Ado.ExecuteCommandAsync(createSql);
        }

        protected void AddHandleLog(string msg)
        {
            LogUtil.Debug(msg);
            _logEo.HandleLog += msg + Environment.NewLine;
        }

        protected async Task<ColumnValue> GetBeginValue(ColumnValue endValue)
        {
            var sql = $"SELECT MIN(`{_item.ColumnName}`) FROM `{_item.TableName}` WHERE {_columnHelper.GetColumnWhere(null, endValue.Value)}";
            var value = await GetItemDb().Ado.GetScalarAsync(sql);
            return _columnHelper.ParseColumnValue(value);
        }
    }
}
