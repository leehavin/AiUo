namespace AiUo.Demos.Patterns.Structural;

internal class FacadeDemo : DemoBase
{
    public override async Task Execute()
    {
        var facade = new RegistrationFacade();
        if (facade.RegisterCourse("设计模式", "Learning Hard"))
        {
            Console.WriteLine("选课成功");
        }
        else
        {
            Console.WriteLine("选课失败");
        }
    }
}
// 外观类
public class RegistrationFacade
{
    private RegisterCourse registerCourse;
    private NotifyStudent notifyStu;
    public RegistrationFacade()
    {
        registerCourse = new RegisterCourse();
        notifyStu = new NotifyStudent();
    }

    public bool RegisterCourse(string courseName, string studentName)
    {
        if (!registerCourse.CheckAvailable(courseName))
        {
            return false;
        }

        return notifyStu.Notify(studentName);
    }
}

#region 子系统
// 相当于子系统A
public class RegisterCourse
{
    public bool CheckAvailable(string courseName)
    {
        Console.WriteLine("正在验证课程 {0}是否人数已满", courseName);
        return true;
    }
}

// 相当于子系统B
public class NotifyStudent
{
    public bool Notify(string studentName)
    {
        Console.WriteLine("正在向{0}发生通知", studentName);
        return true;
    }
}
#endregion