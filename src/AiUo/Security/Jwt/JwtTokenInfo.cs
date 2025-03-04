using System;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AiUo.Security;

public class JwtTokenInfo
{
    public JwtTokenStatus Status { get; set; } = JwtTokenStatus.Invalid;

    public string UserId { get; set; }
    public string RoleString { get; set; }
    public UserRole Role { get; set; }
    public string UserIp { get; set; }
    public string SplitDbKey { get; set; }
    public string Meta { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? Expires { get; set; }

    [JsonIgnore]
    public ClaimsPrincipal Principal { get;set;}
}
public class JwtTokenData
{
    public string UserId { get; set; }
    public string Role { get; set; }
    public string UserIp { get; set; }
    public string SplitDbKey { get; set; }
    public string Meta { get; set; }

    public string SigningKey { get; set; }
    public TimeSpan? Expires { get; set; }
}