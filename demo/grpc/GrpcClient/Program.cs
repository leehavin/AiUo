using GrpcClient;
using AiUo;

AiUoHost.Start();

await new TestDemo().Execute();

while (true)
{
    ConsoleEx.Write($"请输入(q退出):", ConsoleColor.Yellow);
    string input = Console.ReadLine()?.Trim();
    if (input?.ToLower() == "q")
        break;
    try
    {
        await new TestDemo().Execute();
    }
    catch (Exception ex)
    {
        ConsoleEx.WriteLineError(ex.Message);
    }
}
