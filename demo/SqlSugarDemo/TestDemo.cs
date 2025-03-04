using AiUo.Data.SqlSugar;
using SqlSugarDemo.DAL;

namespace SqlSugarDemo;

internal class TestDemo : DemoBase
{
    public override async Task Execute()
    {
        DbUtil.GetRepository<Sdemo_classEO>().GetFirst(it => it.Name == "");

        var a = DbUtil.GetDbById("demo").Queryable<Sdemo_classEO>().InSingle("A001");
        var b = DbUtil.GetDbById("demo").Queryable<Sdemo_classEO>().InSingle("A002");

        //DbUtil.GetDb().Updateable<object>().AS("test")
        //    .SetColumns("name", "bbb").Where("id=1").ExecuteCommand();
        //DbUtil.GetNewDb("demo").Updateable<object>().AS("demo_class")
        //    .SetColumns("name", "aaa").Where("classId='A002'").ExecuteCommand();
        DbUtil.GetDbById("demo").Updateable<object>().AS("demo_class")
            .SetColumns("name", "aaa").Where("classId='A002'").ExecuteCommand();
        //DbUtil.GetDb("demo").Updateable<object>().AS("demo_class")
        //    .SetColumns("name", "bbb").Where("classId='A002'").ExecuteCommand();


        var tm = new DbTransactionManager();
        try
        {
            tm.Begin();
            tm.GetDbById().Updateable<object>().AS("test").SetColumns("name", "ddd").Where("id=1").ExecuteCommand();
            tm.GetDbById("demo").Updateable<object>().AS("demo_class").SetColumns("name", "ddd").Where("classId='A002'").ExecuteCommand();
            tm.Rollback();
            //tm.Commit();
        }
        catch (Exception ex)
        {
            tm.Rollback();
        }
    }
}
/*
public class TaskAttribute : Attribute
{
    public TaskAttribute(string itemId, int index)
    { }
}
public interface ITaskService
{
    string ItemId { get; }
    Task<T> CheckUpdate<T>(T data, DataIpo state = null);
}
public class TaskEndEmail
{
    public string Email { get; set; }
}
public class TaskEndNotify
{
    public bool Enabled { get; set; }
}
public class DataIpo
{
    public string UserId { get; set; }
    public object State { get; set; }
}
public abstract class DataService<TTaskService>
    where TTaskService : ITaskService
{
    private TTaskService _svc;
    public DataService()
    {
    }
    public abstract string ItemId { get; }
    public Dictionary<Type, >

    public Task<T> Get<T>(DataIpo ipo)
                    where T : IBaseData
    {
        T oldValue = default;
        var value = _svc.CheckUpdate<T>(oldValue,state);
        if (value != null)
        {
            // 保存value并返回
        }
        else
        {
            //
        }
    }
    public Task Set<T>(T data)
        where T: IBaseData
    {

    }
    public Task SendEmail(TaskEndNotify email)
    { }
}
public class ATaskService : ITaskService
{
    public Task<T> CheckUpdate<T>(T data, object state = null)
    {
        return null;
    }
    public Task<bool> Execute(object aata)
    {

    }
}
[Task("",1)]
public class AData1
{
    public long DepositAmount { get; set; }
    public List<ADataItem> Items { get; set; }
}
public class ADataItem
{
    public int DayId { get; set; }
    public int Level { get; set; }
}
*/