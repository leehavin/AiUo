using Demo.ConsoleEXE.DAL;
using System.Diagnostics;
using AiUo.Demos;
using AiUo.DbCaching;
using AiUo.Data.SqlSugar;

namespace Demo.ConsoleEXE;

internal class DbCachingDemo : DemoBase
{
    public override async Task Execute()
    {
        var list = await DbCachingUtil.GetAllCacheItem();
        var eo1 = DbCachingUtil.GetSingle<Ss_providerEO>("own");
        //await DbCachingUtil.PublishUpdate(new List<DbCacheItem> 
        //{
        //    new DbCacheItem
        //    {
        //        ConfigId = null,
        //        TableName="s_provider"
        //    }
        //});
        await Task.Delay(10000);
        var eo2 = DbCachingUtil.GetSingle<Ss_providerEO>("own");
        Console.WriteLine(eo1.ProviderName == eo2.ProviderName);
    }
    private async Task Run2()
    {
        var operatorId = "own_lobby_bra";
        var a = DbCachingUtil.GetList<Ssf_promoter_comm_configEO>(it => new { it.OperatorID }, new Ssf_promoter_comm_configEO
        {
            OperatorID = operatorId
        });
        var b = DbCachingUtil.GetList<Ssf_promoter_comm_configEO>(it => it.OperatorID, "own_lobby_bra");
    }
    private async Task Run1()
    {
        var stopwatch = new Stopwatch();
        var appList = await DbUtil.GetRepository<Ss_appEO>().GetListAsync();
        var operList = await DbUtil.GetRepository<Ss_operator_appEO>().GetListAsync();
        foreach (var app in appList)
        {
            var i = 0;
            stopwatch.Reset();
            stopwatch.Start();
            var sAppEo = DbCacheUtil.GetApp(app.AppID);
            var provider = DbCacheUtil.GetProvider(sAppEo.ProviderID);
            foreach (var oper in operList)
            {
                var item = DbCacheUtil.GetOperatorApp(oper.OperatorID, app.AppID);
                if (item == null)
                    i++;
            }
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds} count:{i}");
            stopwatch.Stop();
        }
    }
    private async Task Run3()
    {
        var a = DbUtil.GetDb<Ssf_promoter_comm_configEO>();
        var b = DbUtil.GetRepository<Ssf_promoter_comm_configEO>();
        var tm = new DbTransactionManager();
        try
        {
            tm.Begin();
            var db = tm.GetDb<Ssf_promoter_comm_configEO>();
            var repo = tm.GetRepository<Ssf_promoter_comm_configEO>();
            tm.Commit();
        }
        catch (Exception ex)
        {
            tm.Rollback();
        }
    }
    private async Task Run4()
    {
        var eo = DbCachingUtil.GetSingle<Ss_providerEO>("own");
        Console.WriteLine(eo);
    }
    private async Task Run5()
    {
        var eo = DbCachingUtil.GetSingle<Ss_providerEO>(it => it.ProviderName, "Hub88");
        Console.WriteLine(eo);
    }
    private async Task Run6()
    {
        var eo = DbCachingUtil.GetSingle(() => new Ss_providerEO
        {
            ProviderName = "自有供应商",
            UseBonus = false
        });
        Console.WriteLine(eo);
    }
    private async Task Run7()
    {
        var eo = DbCachingUtil.GetSingle<Ss_providerEO>(it => new { it.ProviderName, it.UseBonus }, new Ss_providerEO
        {
            ProviderName = "自有供应商",
            UseBonus = false
        });
        Console.WriteLine(eo);
    }
    private async Task Run8()
    {
        var eo = DbCachingUtil.GetList<Ss_providerEO>(it => it.ProviderType, 2);
        Console.WriteLine(eo);
    }
    private async Task Run9()
    {
        var eo = DbCachingUtil.GetList(() => new Ss_providerEO
        {
            ProviderType = 2,
            UseBonus = false
        });
        Console.WriteLine(eo);
    }
    private async Task Run10()
    {
        DbCachingUtil.GetList(() => new Ss_providerEO
        {
            ProviderType = 2,
            UseBonus = false
        });
        var eo = DbCachingUtil.GetList<Ss_providerEO>(it => new { it.ProviderType, it.UseBonus }, new Ss_providerEO
        {
            ProviderType = 2,
            UseBonus = false
        });
        Console.WriteLine(eo);
    }
}