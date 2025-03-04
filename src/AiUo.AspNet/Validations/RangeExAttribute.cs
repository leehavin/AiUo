using System.ComponentModel.DataAnnotations;
using AiUo.Net;

namespace AiUo.AspNet;

public class RangeExAttribute : RangeAttribute
{
    public string Code { get; set; }
    public RangeExAttribute(int minimum, int maximum, string code, string message = null)
        : base(minimum, maximum)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }
    public RangeExAttribute(double minimum, double maximum, string code, string message = null)
        : base(minimum, maximum)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }
    public RangeExAttribute(Type type, string minimum, string maximum, string code, string message = null)
        : base(type, minimum, maximum)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }
    public override string FormatErrorMessage(string name)
    {
        return $"{ErrorMessage}|{Code}";
    }
}