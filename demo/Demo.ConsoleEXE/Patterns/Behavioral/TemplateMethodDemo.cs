namespace AiUo.Demos.Patterns.Behavioral;

/// <summary>
/// 模板方法
/// 抽象类中定义一个操作中的算法骨架,步骤延迟到子类中去实现
/// </summary>
internal class TemplateMethodDemo : DemoBase
{
    public override async Task Execute()
    {
        // 创建一个菠菜实例并调用模板方法
        Spinach spinach = new Spinach();
        spinach.CookVegetabel();
    }
}
public abstract class Vegetabel
{
    // 模板方法，不要把模版方法定义为Virtual或abstract方法，避免被子类重写，防止更改流程的执行顺序
    public void CookVegetabel()
    {
        Console.WriteLine("抄蔬菜的一般做法");
        this.pourOil();
        this.HeatOil();
        this.pourVegetable();
        this.stir_fry();
    }

    // 第一步倒油
    public void pourOil()
    {
        Console.WriteLine("倒油");
    }

    // 把油烧热
    public void HeatOil()
    {
        Console.WriteLine("把油烧热");
    }

    // 油热了之后倒蔬菜下去，具体哪种蔬菜由子类决定
    public abstract void pourVegetable();

    // 开发翻炒蔬菜
    public void stir_fry()
    {
        Console.WriteLine("翻炒");
    }
}

// 菠菜
public class Spinach : Vegetabel
{

    public override void pourVegetable()
    {
        Console.WriteLine("倒菠菜进锅中");
    }
}

// 大白菜
public class ChineseCabbage : Vegetabel
{
    public override void pourVegetable()
    {
        Console.WriteLine("倒大白菜进锅中");
    }
}