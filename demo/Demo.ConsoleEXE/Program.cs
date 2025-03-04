using AiUo;
using AiUo.Collections;
using AiUo.Reflection;
using System.Reflection;

namespace AiUo.Demos;

internal static class Program
{
    static async Task Main(string[] args)
    {
        AiUoHost.Start();

        var demoId = string.Empty;
        demoId = "TestDemo";

        var types = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsSubclassOf(typeof(DemoBase))
            select t;
        var demos = new Dictionary<string, DemoBase>();
        types.ForEach(t =>
        {
            var demo = (DemoBase)ReflectionUtil.CreateInstance(t);
            demos.Add(demo.DemoId, demo);
        });

        if (!string.IsNullOrEmpty(demoId))
        {
            try
            {
                await demos[demoId].Execute();
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteLineError(ex.Message);
            }
        }

        while (true)
        {
            ConsoleEx.Write($"请输入DemoId(默认类名，q退出):", ConsoleColor.Yellow);
            string input = Console.ReadLine()?.Trim();
            if (input?.ToLower() == "q")
                break;
            if (string.IsNullOrEmpty(input))
                input = demoId;
            if (!demos.ContainsKey(input))
            {
                ConsoleEx.WriteLine("未找到此DemoId，请重新输入！", ConsoleColor.Red);
                continue;
            }
            try
            {
                await demos[input].Execute();
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteLineError(ex.Message);
            }
        }
    }
}
internal abstract class DemoBase
{
    public string DemoId { get; }
    public abstract Task Execute();
    public DemoBase()
    {
        DemoId = GetType().Name;
    }
}