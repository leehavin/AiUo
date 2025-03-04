using System.ComponentModel.DataAnnotations;
using AiUo.Net;

namespace AiUo.AspNet;

public class StringLengthExAttribute : StringLengthAttribute
{
    public string Code { get; set; }
    public StringLengthExAttribute(int maximumLength, string code, string message = null)
        : base(maximumLength)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }
    public override string FormatErrorMessage(string name)
    {
        return $"{ErrorMessage}|{Code}";
    }
}