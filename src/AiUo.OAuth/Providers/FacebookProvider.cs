using System.Threading.Tasks;
using AiUo.Net;

namespace AiUo.OAuth.Providers;

internal class FacebookProvider : BaseOAuthProvider
{
    public override OAuthProviders Provider => OAuthProviders.Facebook;
    protected override string OAuthUrl => "https://www.facebook.com/v17.0/dialog/oauth";

    protected override string TokenUrl => "https://graph.facebook.com/oauth/access_token";

    protected override string UserInfoUrl => "https://graph.facebook.com/me";

    //  protected override string AppendOAuthUrl() => null;

    protected override string AppendUserUrl() => "&fields=id,name,birthday,gender,hometown,email,devices,picture.width(400)";
    public override async Task<ResponseResult<OAuthUserDto>> GetUserInfo(OAuthUserIpo ipo)
    {
        return await RequestUser<SuccessRsp, ErrorRsp>(ipo, srsp =>
        {
            return new OAuthUserDto
            {
                OAuthId = srsp.id,
                UserName = "",
                NickName = srsp.name,
                Avatar = srsp.picture.data.url,
                Location = srsp.locale,
                Email = srsp.email,
            };
        });
    }
    protected override string AppendOAuthUrl(){
        return "scope=openid,public_profile,email";
    }

    #region req & rsp
    class SuccessRsp
    {
        public string id { get; set; }
        public string name { get; set; }
        public string locale { get; set; }
        public string email { get; set; }
        public Picture picture { get; set; }
        public class Picture
        {
            public PictureData data { get; set; }
        }
        public class PictureData
        {
            public int height { get; set; }
            public int width { get; set; }
            public bool is_silhouette { get; set; }
            public string url { get; set; }
        }
    }
    class ErrorRsp
    {
        public string message { get; set; }
        public string type { get; set; }
        public int code { get; set; }
        public string fbtrace_id { get; set; }
    }
    #endregion
}

/*
success:
{
"id": "122125719806031296",
"name": "\u9ad8\u6d2a\u9501",
"picture": {
"data": {
  "height": 180,
  "is_silhouette": true,
  "url": "https://scontent-sea1-1.xx.fbcdn.net/v/t1.30497-1/84628273_176159830277856_972693363922829312_n.jpg?stp=dst-jpg_s480x480&_nc_cat=1&ccb=1-7&_nc_sid=810bd0&_nc_ohc=PMtp3IjkGpMAX_Crqc6&_nc_ht=scontent-sea1-1.xx&edm=AP4hL3IEAAAA&oh=00_AfA-Qi7Iu03_LuesYBJuJD3Bv3x9I-KoB4QNXoz6Ct8CMQ&oe=65581099",
  "width": 180
}
}
}
error:
{
"error": {
"message": "Invalid OAuth access token - Cannot parse access token",
"type": "OAuthException",
"code": 190,
"fbtrace_id": "Adk_-asJluJMJDhpofabTgz"
}
}
*/