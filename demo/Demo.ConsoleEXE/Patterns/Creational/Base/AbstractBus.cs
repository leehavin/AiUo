namespace AiUo.Demos.Patterns.Creational.Base;

public abstract class AbstractBus
{
    protected abstract void Do();

    public void GetInfo()
    {
        Console.WriteLine(string.Format("I am {0}.", this.GetType().Name));
    }
}

public class BusA : AbstractBus
{

    protected override void Do()
    {

        throw new System.NotImplementedException();
    }
}

public class BusB : AbstractBus
{
    protected override void Do()
    {
        throw new System.NotImplementedException();
    }
}