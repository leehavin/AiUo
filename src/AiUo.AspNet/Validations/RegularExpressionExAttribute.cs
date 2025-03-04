using System.ComponentModel.DataAnnotations;
using AiUo.Net;

namespace AiUo.AspNet;

public class RegularExpressionExAttribute : RegularExpressionAttribute
{
    public string Code { get; set; }
    public RegularExpressionExAttribute(string pattern, string code, string message = null)
        : base(pattern)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }
    public override string FormatErrorMessage(string name)
    {
        return $"{ErrorMessage}|{Code}";
    }
}