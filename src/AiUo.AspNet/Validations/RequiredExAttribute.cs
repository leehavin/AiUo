using System.ComponentModel.DataAnnotations;
using AiUo.Net;

namespace AiUo.AspNet;

public class RequiredExAttribute : RequiredAttribute
{
    public string Code { get; set; }
    public RequiredExAttribute(string code, string message = null)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }
    public override string FormatErrorMessage(string name)
    {
        return $"{ErrorMessage}|{Code}";
    }
}