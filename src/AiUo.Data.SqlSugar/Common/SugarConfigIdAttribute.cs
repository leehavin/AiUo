namespace AiUo.Data.SqlSugar;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class SugarConfigIdAttribute: Attribute
{
    public string ConfigId { get; set; }
    public SugarConfigIdAttribute(string configId=null) 
    {
        ConfigId = configId;
    }
}