using System.ComponentModel.DataAnnotations;
using AiUo.Net;

namespace AiUo.AspNet;

public class MinLengthExAttribute : MinLengthAttribute
{
    public string Code { get; set; }
    public MinLengthExAttribute(int length, string code, string message = null)
        : base(length)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }
    public override string FormatErrorMessage(string name)
    {
        return $"{ErrorMessage}|{Code}";
    }
}