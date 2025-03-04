using System.ComponentModel.DataAnnotations;
using AiUo.Net;

namespace AiUo.AspNet;

public class CompareExAttribute : CompareAttribute
{
    public string Code { get; set; }
    public CompareExAttribute(string otherProperty, string code, string message = null)
        : base(otherProperty)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }
    public override string FormatErrorMessage(string name)
    {
        return $"{ErrorMessage}|{Code}";
    }
}