using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiUo.BIZ.DataSplit;
using AiUo.BIZ.DataSplit.DAL;
using AiUo.BIZ.DataSplit.JOB.DataMove;
using AiUo.Data.SqlSugar;
using AiUo.DbCaching;


namespace AiUo.BIZ.DataSplit
{
    /// <summary>
    /// 数据分库分表定时任务
    /// </summary>
    public class DataSplitJob
    {
        public async Task Execute(List<S_split_tablePO> list = null, string defaultConfigId = null)
        {
            if (list == null || list.Count == 0)
            {
                list = await DbUtil.GetDbById(defaultConfigId).Queryable<S_split_tablePO>()
                    .Where(it => it.Status == 1).OrderBy(it => it.HandleOrder).ToListAsync();
            }
            var execTime = DateTime.UtcNow;
            // 执行
            foreach (var item in list)
            {
                var mode = item.HandleMode.ToEnum(HandleMode.None);
                switch (mode)
                {
                    case HandleMode.Delete:
                        await new DeleteJob(item, defaultConfigId, execTime).Execute();
                        break;
                    case HandleMode.Backup:
                        await new BackupJob(item, defaultConfigId, execTime).Execute();
                        break;
                    case HandleMode.MaxRows:
                        await new SplitMaxRowsJob(item, defaultConfigId, execTime).Execute();
                        break;
                }
            }
            // 缓存
            var msg = new DbCacheChangeMessage();
            msg.Changed.Add(new DbCacheItem
            {
                ConfigId = DbUtil.DefaultConfigId,
                TableName = "tfx_split_table"
            });
            msg.Changed.Add(new DbCacheItem
            {
                ConfigId = DbUtil.DefaultConfigId,
                TableName = "tfx_split_table_detail"
            });
            await DbCachingUtil.PublishUpdate(msg);
        }
        public Task Execute(S_split_tablePO item, string defaultConfigId = null)
            => Execute(new List<S_split_tablePO> { item }, defaultConfigId);
    }
}
