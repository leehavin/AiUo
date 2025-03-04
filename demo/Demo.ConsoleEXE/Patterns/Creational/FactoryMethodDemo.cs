using AiUo.Demos.Patterns.Creational.Base;

namespace AiUo.Demos.Patterns.Creational;

/// <summary>
/// 工厂方法模式：
/// 针对每一种产品提供一个工厂类。通过不同的工厂实例来创建不同的产品实例。
/// 在同一等级结构中，支持增加任意产品。
/// 符合【开放封闭原则】，但随着产品类的增加，对应的工厂也会随之增多
/// </summary>
internal class FactoryMethodDemo : DemoBase
{
    public override async Task Execute()
    {
        Console.WriteLine("工厂方法模式：");
        var factoryB = new ConcreateFactoryB();
        var productB = factoryB.Create();
        productB.GetInfo();
    }
}
internal interface IFactoryMethod
{
    AbstractCar Create();
}

internal class ConcreateFactoryA : IFactoryMethod
{
    public AbstractCar Create()
    {
        return new CarA();
    }
}

internal class ConcreateFactoryB : IFactoryMethod
{
    public AbstractCar Create()
    {
        return new CarB();
    }
}