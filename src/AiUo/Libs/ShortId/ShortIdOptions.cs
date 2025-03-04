namespace AiUo.ShortId;

public class ShortIdOptions
{
    public bool UseUuidSpecials { get; set; } = true;
    public bool UseNumbers { get; set; } = true;
    public bool UseLowerLetters { get; set; } = true;
    public bool UseUpperLetters { get; set; } = true;
    /// <summary>
    /// 增加用户自定义的字典
    /// </summary>
    public string CustomAlphabet { get; set; }
}