using AiUo.Collections;

namespace AiUo.Demos.Core;

internal class CombinatoricUtilDemo : DemoBase
{
    public override async Task Execute()
    {
        //Javascript Splice函数的功能
        Console.WriteLine("CombinatoricUtil");
        int[] a = { 1, 2, 3, 4, 5, 6 };
        CombinatoricUtil.Splice(ref a, 2, 2);
        Console.WriteLine(string.Join('-', a));
        CombinatoricUtil.Splice(ref a, 2, 0, 9, 9);
        Console.WriteLine(string.Join('-', a));
        CombinatoricUtil.Splice(ref a, 2, 1, 9, 9);
        Console.WriteLine(string.Join('-', a));

        // 排列组合

        // 不重复,指定个数,从小到大排列组合
        var list = CombinatoricUtil.GetCombination(3, 2);
        Console.WriteLine(string.Join('|',list));
        // 不重复,任意个数,从小到大排列组合
        list = CombinatoricUtil.GetCombinationForAllPicks(3);
        Console.WriteLine(string.Join('|', list));
        //不重复,指定个数,无序排列组合
        list = CombinatoricUtil.GetPermutation(3, 2);
        Console.WriteLine(string.Join('|', list));
        // 不重复,任意个数,无序排列组合
        list = CombinatoricUtil.GetPermutationForAllChoices(3);
        Console.WriteLine(string.Join('|', list));
        // 可重复,指定个数,从小到大排列组合
        list = CombinatoricUtil.GetMulticombination(3, 2);
        Console.WriteLine(string.Join('|', list));
        list = CombinatoricUtil.GetMulticombinationForPicks(3, 1,2);
        Console.WriteLine(string.Join('|', list));
        int[] sizes = { 2, 2 };
        list = CombinatoricUtil.GetProduct(sizes);
        Console.WriteLine(string.Join('|', list));
    }
}