namespace AiUo.Demos.Patterns.Behavioral;

/// <summary>
/// 备忘录模式
/// 对某个类的状态进行保存下来，等到需要恢复的时候，可以从备忘录中进行恢复
/// </summary>
internal class MementoDemo : DemoBase
{
    public override async Task Execute()
    {
        List<ContactPerson> persons = new List<ContactPerson>()
        {
            new ContactPerson() { Name= "Learning Hard", MobileNum = "123445"},
            new ContactPerson() { Name = "Tony", MobileNum = "234565"},
            new ContactPerson() { Name = "Jock", MobileNum = "231455"}
        };
        MobileOwner mobileOwner = new MobileOwner(persons);
        mobileOwner.Show();

        // 创建备忘录并保存备忘录对象
        Caretaker caretaker = new Caretaker();
        caretaker.ContactM = mobileOwner.CreateMemento();

        // 更改发起人联系人列表
        Console.WriteLine("----移除最后一个联系人--------");
        mobileOwner.ContactPersons.RemoveAt(2);
        mobileOwner.Show();

        // 恢复到原始状态
        Console.WriteLine("-------恢复联系人列表------");
        mobileOwner.RestoreMemento(caretaker.ContactM);
        mobileOwner.Show();
    }
}
// 联系人
public class ContactPerson
{
    public string Name { get; set; }
    public string MobileNum { get; set; }
}

// 发起人
public class MobileOwner
{
    // 发起人需要保存的内部状态
    public List<ContactPerson> ContactPersons { get; set; }

    public MobileOwner(List<ContactPerson> persons)
    {
        ContactPersons = persons;
    }

    // 创建备忘录，将当期要保存的联系人列表导入到备忘录中 
    public ContactMemento CreateMemento()
    {
        // 这里也应该传递深拷贝，new List方式传递的是浅拷贝，
        // 因为ContactPerson类中都是string类型,所以这里new list方式对ContactPerson对象执行了深拷贝
        // 如果ContactPerson包括非string的引用类型就会有问题，所以这里也应该用序列化传递深拷贝
        return new ContactMemento(new List<ContactPerson>(this.ContactPersons));
    }

    // 将备忘录中的数据备份导入到联系人列表中
    public void RestoreMemento(ContactMemento memento)
    {
        // 下面这种方式是错误的，因为这样传递的是引用，
        // 则删除一次可以恢复，但恢复之后再删除的话就恢复不了.
        // 所以应该传递contactPersonBack的深拷贝，深拷贝可以使用序列化来完成
        this.ContactPersons = memento.contactPersonBack;
    }

    public void Show()
    {
        Console.WriteLine("联系人列表中有{0}个人，他们是:", ContactPersons.Count);
        foreach (ContactPerson p in ContactPersons)
        {
            Console.WriteLine("姓名: {0} 号码为: {1}", p.Name, p.MobileNum);
        }
    }
}

// 备忘录
public class ContactMemento
{
    // 保存发起人的内部状态
    public List<ContactPerson> contactPersonBack;

    public ContactMemento(List<ContactPerson> persons)
    {
        contactPersonBack = persons;
    }
}

// 管理角色
public class Caretaker
{
    public ContactMemento ContactM { get; set; }
}