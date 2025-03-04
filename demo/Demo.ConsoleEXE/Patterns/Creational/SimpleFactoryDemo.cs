using AiUo.Demos.Patterns.Creational.Base;

namespace AiUo.Demos.Patterns.Creational;

/// <summary>
/// 简单工厂模式：
/// 使用静态方法，通过接收的参数的不同来返回不同的对象实例。
/// 不修改代码的话，是无法扩展的。（如果增加新的产品，需要增加工厂的Swith分支）
/// 不符合【开放封闭原则】
/// </summary>
internal class SimpleFactoryDemo : DemoBase
{
    public override async Task Execute()
    {
        Console.WriteLine("简单工厂模式：");
        Create(ProductEnum.B).GetInfo();
    }
    public static AbstractCar Create(ProductEnum productType)
    {
        switch (productType)
        {
            case ProductEnum.A:
                return new CarA();
            case ProductEnum.B:
                return new CarB();
            default:
                return null;
        }
    }
}
public enum ProductEnum
{
    A,
    B
}