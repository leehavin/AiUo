using AiUo.Common;

namespace AiUo.Demos.Core;

internal class CSharpCodeExecutorDemo : DemoBase
{
    public override async Task Execute()
    {
        var src = @"
using System;
namespace Sample
{
    public class Program
    {
        public static void Main(string str)
        {
            Console.WriteLine(str);
        }
    }
}
";
        //动态编译
        var cs = new CSharpCodeExecutor(src);
        cs.ExecuteStaticMethod("Sample.Program", "Main", "aaa");
    }
}