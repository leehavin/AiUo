using System;

namespace AiUo.Randoms;

public class RandomException : Exception
{
    public RandomException(string msg) : base(msg)
    { }
}