using System;

namespace AiUo;

public class EnumMapAttribute:Attribute
{
    public string MapName { get; set; }
    public EnumMapAttribute(string mapName) 
    {
        MapName = mapName;
    }
}