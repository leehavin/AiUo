using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiUo.BIZ.DataSplit;
using AiUo.BIZ.DataSplit.DAL;

namespace AiUo.BIZ.DataSplit.JOB.DataMove
{
    internal class DeleteJob : BaseDataMoveJob
    {
        public DeleteJob(S_split_tablePO item, string defaultConfigId, DateTime execTime) 
            : base(item, defaultConfigId, execTime)
        {
            if ((HandleMode)item.HandleMode != HandleMode.Delete)
                throw new Exception($"{GetType().FullName}时HandleMode必须是Delete");
        }
        protected override async Task ExecuteJob()
        {
            var end = _columnHelper.GetKeepEndValue(_execTime);
            var begin = await GetBeginValue(end);
            if (string.IsNullOrEmpty(begin.Value))
                return;

            _logEo.BeginDate = begin.Date;
            _logEo.BeginValue = begin.Value;
            _logEo.EndDate = end.Date;
            _logEo.EndValue = end.Value;

            AddHandleLog($"==> 删除开始{_item.TableName} {begin.Date.ToString("yyyy-MM-dd")} => {end.Date.ToString("yyyy-MM-dd")}");
            var where = _columnHelper.GetColumnWhere(null, end.Value);
            var sql = $"DELETE FROM `{_item.TableName}` WHERE {where}";
            AddHandleLog($"SQL: {sql}");
            if (BATCH_PAGE_SIZE > 0)
                sql += $" LIMIT {BATCH_PAGE_SIZE}";
            while (true)
            {
                //var rows = 0;
                var rows = await GetItemDb().Ado.ExecuteCommandAsync(sql);
                if (rows == 0) break;
                _logEo.RowNum += rows;
                await Task.Delay(200);
            }
            AddHandleLog($"==> 删除完成{_item.TableName} {begin.Date.ToString("yyyy-MM-dd")} => {end.Date.ToString("yyyy-MM-dd")} count: {_logEo.RowNum}");
        }
    }
}
