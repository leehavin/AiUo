using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AiUo.Configuration;

namespace AiUo.Security;

public static class JwtUtil
{
    /// <summary>
    /// 生成JWT Token
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="role"></param>
    /// <param name="userIp"></param>
    /// <param name="splitDbKey"></param>
    /// <param name="meta"></param>
    /// <returns></returns>
    public static string CreateJwtToken(object userId, string role = null, string userIp = null, string splitDbKey = null, string meta = null)
    {
        return CreateJwtToken(new JwtTokenData
        {
            UserId = Convert.ToString(userId),
            Role = role,
            UserIp = userIp,
            SplitDbKey = splitDbKey,
            Meta = meta
        });
    }
    /// <summary>
    /// 生成JWT Token
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string CreateJwtToken(JwtTokenData data)
    {
        var section = GetSection(data.SigningKey);
        var signKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section.SigningKey));
        //
        var uid = Convert.ToString(data.UserId);
        if (string.IsNullOrEmpty(uid))
            throw new Exception("生成Jwt Token时userId不能为空");
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, uid)
            }),
            Issuer = section?.Issuer,
            Audience = section?.Audience,
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256Signature)
        };
        if (!string.IsNullOrEmpty(data.Role))
            tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, data.Role));
        if (!string.IsNullOrEmpty(data.UserIp))
            tokenDescriptor.Subject.AddClaim(new Claim("uip", data.UserIp));
        if (!string.IsNullOrEmpty(data.Meta))
            tokenDescriptor.Subject.AddClaim(new Claim("meta", data.Meta));
        if (!string.IsNullOrEmpty(data.SplitDbKey))
            tokenDescriptor.Subject.AddClaim(new Claim("splitdbkey", data.SplitDbKey));

        var expire = data.Expires.HasValue
            ? data.Expires.Value.TotalMinutes
            : (section?.ExpireMinutes) ?? 0;
        if (expire > 0)
            tokenDescriptor.Expires = DateTime.UtcNow.AddMinutes(expire);

        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// 解码（读取）JWT Token
    /// </summary>
    /// <param name="token"></param>
    /// <param name="signingKey"></param>
    /// <returns></returns>
    public static JwtTokenInfo ReadJwtToken(string token, string signingKey = null)
    {
        var ret = new JwtTokenInfo();
        try
        {
            // signSecret
            var section = GetSection(signingKey);
            var parameters = GetParameters(section);
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, parameters, out SecurityToken stoken);
            ret = ReadJwtToken(principal);
            if (ret.Expires.HasValue && ret.Expires.Value < DateTime.UtcNow)
                ret.Status = JwtTokenStatus.Expired;
        }
        catch (SecurityTokenExpiredException)
        {
            ret.Status = JwtTokenStatus.Expired;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            ret.Status = JwtTokenStatus.Invalid;
        }
        catch (Exception)
        {
            ret.Status = JwtTokenStatus.Invalid;
        }
        return ret;
    }

    public static JwtTokenInfo ReadJwtToken(ClaimsPrincipal principal)
    {
        var ret = new JwtTokenInfo()
        {
            Status = JwtTokenStatus.Success,
            Principal = principal,
        };
        var claimDict = ret.Principal.Claims.ToDictionary(x => x.Type);

        // userId
        ret.UserId = principal.Identity.Name;
        ret.RoleString = claimDict.TryGetValue(ClaimTypes.Role, out var v1) && v1 != null ? v1.Value : null;
        ret.Role = !string.IsNullOrEmpty(ret.RoleString) ? ret.RoleString.ToEnum(UserRole.Unknow) : UserRole.Unknow;
        ret.UserIp = claimDict.TryGetValue("uip", out var v3) && v3 != null
            ? v3.Value : null;
        ret.SplitDbKey = claimDict.TryGetValue("splitdbkey", out var v4) && v4 != null
            ? v4.Value : null;
        ret.Meta = claimDict.TryGetValue("meta", out var v5) && v5 != null
            ? v5.Value : null;

        ret.IssuedAt = claimDict.TryGetValue("iat", out var v2) && v2 != null
            ? DateTimeUtil.ParseTimestamp(v2.Value) : null;
        ret.Expires = claimDict.TryGetValue("exp", out var v6) && v6 != null
            ? DateTimeUtil.ParseTimestamp(v6.Value, true) : null;
        return ret;
    }
    private static JwtAuthSection GetSection(string signingKey = null)
    {
        var section = ConfigUtil.GetSection<JwtAuthSection>() ?? new JwtAuthSection();
        if (!string.IsNullOrEmpty(signingKey))
            section.SigningKey = signingKey;
        if (string.IsNullOrEmpty(section.SigningKey))
            throw new Exception("请在配置文件中配置JwtAuth:SignSecret");
        return section;
    }

    public static TokenValidationParameters GetParameters(JwtAuthSection section)
    {
        var ret = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.FromMinutes(10), // 时钟偏斜可补偿服务器时间漂移
            ValidateIssuerSigningKey = true, //是否验证SigningKey
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section.SigningKey)),
            RequireSignedTokens = true,
        };

        //是否验证失效时间
        ret.ValidateLifetime = section.ValidateLifetime;
        if (section.ValidateLifetime)
        {
            ret.RequireExpirationTime = true;
        }
        // 验证颁发者
        if (!string.IsNullOrEmpty(section.Issuer))
        {
            ret.ValidateIssuer = true;
            ret.ValidIssuer = section.Issuer;
        }

        // 验证授权
        if (!string.IsNullOrEmpty(section.Audience))
        {
            ret.ValidateAudience = true;
            ret.ValidAudience = section.Audience;
        }

        return ret;
    }
}