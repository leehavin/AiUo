namespace Demo.WebAPI.BLL.Demo;

/*
public class DemoIpo
{
    [StringLength(50)]
    [StringLengthEx(2,"ErrorCode","Message")]
    public string Value { get; set; }
}
public class DemoDto : IMapFrom<Demo_userEO>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public List<string> Items { get; set; }
    public Dictionary<string, int> Dics { get; set; }
    public DemoDto()
    {
        Id = 1;
        Name = "TEST";
        Birthday = DateTime.Now;
        Items = new List<string> { "aaa", "bbb" };
        Dics = new Dictionary<string, int> { { "ccc", 1 }, { "ddd", 2 } };
    }

    public void MapFrom(Demo_userEO source)
    {
        this.Id = (int)source.UserID;
        this.Name = source.ClassID;
    }
}
*/