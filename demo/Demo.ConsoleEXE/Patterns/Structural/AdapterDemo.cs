namespace AiUo.Demos.Patterns.Structural;

/// <summary>
/// 适配器模式
/// 把一个类的接口变换成客户端所期待的另一种接口
/// </summary>
internal class AdapterDemo : DemoBase
{
    public override async Task Execute()
    {
        // 现在客户端可以通过电适配要使用2个孔的插头了
        ThreeHole threehole = new PowerAdapter();
        threehole.Request();
        Console.ReadLine();

    }
}
/// <summary>
/// 三个孔的插头，也就是适配器模式中的目标(Target)角色
/// </summary>
public class ThreeHole
{
    // 客户端需要的方法
    public virtual void Request()
    {
        // 可以把一般实现放在这里
    }
}

/// <summary>
/// 两个孔的插头，源角色——需要适配的类
/// </summary>
public class TwoHole
{
    public void SpecificRequest()
    {
        Console.WriteLine("我是两个孔的插头");
    }
}

/// <summary>
/// 适配器类，这里适配器类没有TwoHole类，
/// 而是引用了TwoHole对象，所以是对象的适配器模式的实现
/// </summary>
public class PowerAdapter : ThreeHole
{
    // 引用两个孔插头的实例,从而将客户端与TwoHole联系起来
    public TwoHole twoholeAdaptee = new TwoHole();

    /// <summary>
    /// 实现三个孔插头接口方法
    /// </summary>
    public override void Request()
    {
        twoholeAdaptee.SpecificRequest();
    }
}