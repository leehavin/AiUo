using AiUo.Demos.Patterns.Creational.Base;

namespace AiUo.Demos.Patterns.Creational;

internal class AbstractFactoryDemo : DemoBase
{
    /// <summary>
    /// 抽象工厂模式：
    /// 应对产品族概念的，比如说，每个汽车公司可能要同时生产轿车，货车，客车，那么每一个工厂都要有创建轿车，货车和客车的方法。
    /// 应对产品族概念而生，增加新的产品线很容易，但是无法增加新的产品。
    /// </summary>
    public override async Task Execute()
    {
        Console.WriteLine("抽象工厂模式：");

        var bmwFactory = new AFactory();
        bmwFactory.CreateCar().GetInfo();
        bmwFactory.CreateBus().GetInfo();

        var bydFactory = new BFactory();
        bydFactory.CreateCar().GetInfo();
        bydFactory.CreateBus().GetInfo();
    }
}
internal interface IAbstractFactory
{
    AbstractCar CreateCar();
    AbstractBus CreateBus();
}

/// <summary>
/// 宝马工厂
/// </summary>
internal class AFactory : IAbstractFactory
{
    public AbstractCar CreateCar()
    {
        return new CarA();
    }

    public AbstractBus CreateBus()
    {
        return new BusA();
    }
}

/// <summary>
/// 比亚迪工厂
/// </summary>
internal class BFactory : IAbstractFactory
{
    public AbstractCar CreateCar()
    {
        return new CarB();
    }

    public AbstractBus CreateBus()
    {
        return new BusB();
    }
}