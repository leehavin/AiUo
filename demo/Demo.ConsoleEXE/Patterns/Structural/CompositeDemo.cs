namespace AiUo.Demos.Patterns.Structural;

/// <summary>
/// 组合模式
/// 允许你将对象组合成树形结构来表现”部分-整体“的层次结构，
/// 使得客户以一致的方式处理单个对象以及对象的组合
/// </summary>
internal class CompositeDemo : DemoBase
{
    public override async Task Execute()
    {
        ComplexGraphics complexGraphics = new ComplexGraphics("一个复杂图形和两条线段组成的复杂图形");
        complexGraphics.Add(new Line("线段A"));
        ComplexGraphics CompositeCG = new ComplexGraphics("一个圆和一条线组成的复杂图形");
        CompositeCG.Add(new Circle("圆"));
        CompositeCG.Add(new Circle("线段B"));
        complexGraphics.Add(CompositeCG);
        Line l = new Line("线段C");
        complexGraphics.Add(l);

        // 显示复杂图形的画法
        Console.WriteLine("复杂图形的绘制如下：");
        Console.WriteLine("---------------------");
        complexGraphics.Draw();
        Console.WriteLine("复杂图形绘制完成");
        Console.WriteLine("---------------------");
        Console.WriteLine();

        // 移除一个组件再显示复杂图形的画法
        complexGraphics.Remove(l);
        Console.WriteLine("移除线段C后，复杂图形的绘制如下：");
        Console.WriteLine("---------------------");
        complexGraphics.Draw();
        Console.WriteLine("复杂图形绘制完成");
        Console.WriteLine("---------------------");
    }
}
/// <summary>
/// 图形抽象类，
/// </summary>
public abstract class Graphics
{
    public string Name { get; set; }
    public Graphics(string name)
    {
        this.Name = name;
    }

    public abstract void Draw();
    // 移除了Add和Remove方法
    // 把管理子对象的方法放到了ComplexGraphics类中进行管理
    // 因为这些方法只在复杂图形中才有意义
}

/// <summary>
/// 简单图形类——线
/// </summary>
public class Line : Graphics
{
    public Line(string name)
        : base(name)
    { }

    // 重写父类抽象方法
    public override void Draw()
    {
        Console.WriteLine("画  " + Name);
    }
}

/// <summary>
/// 简单图形类——圆
/// </summary>
public class Circle : Graphics
{
    public Circle(string name)
        : base(name)
    { }

    // 重写父类抽象方法
    public override void Draw()
    {
        Console.WriteLine("画  " + Name);
    }
}

/// <summary>
/// 复杂图形，由一些简单图形组成,这里假设该复杂图形由一个圆两条线组成的复杂图形
/// </summary>
public class ComplexGraphics : Graphics
{
    private List<Graphics> complexGraphicsList = new List<Graphics>();
    public ComplexGraphics(string name)
        : base(name)
    { }

    /// <summary>
    /// 复杂图形的画法
    /// </summary>
    public override void Draw()
    {
        foreach (Graphics g in complexGraphicsList)
        {
            g.Draw();
        }
    }

    public void Add(Graphics g)
    {
        complexGraphicsList.Add(g);
    }
    public void Remove(Graphics g)
    {
        complexGraphicsList.Remove(g);
    }
}