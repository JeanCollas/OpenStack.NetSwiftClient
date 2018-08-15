using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore
{
    public class CookieTokenService
    {
        private HttpContext _HttpContext;
        IDataProtector _Protector;

        public CookieTokenService(
                    IDataProtectionProvider protectorProvider,
                    IHttpContextAccessor httpContextAccessor)
        {
            _HttpContext = httpContextAccessor.HttpContext;
            _Protector = protectorProvider.CreateProtector("Encode OpenStack Token");
        }

        private OpenStackAuthCookie _Token;
        private bool _CookieChecked = false;

        public OpenStackAuthCookie Token
        {
            get => _Token; set
            {
                _Token = value;
                if (_Token != null) _HttpContext.Response.Cookies.Append(OpenStackAuthCookie.CookieName, _Token.Encode(_Protector));
            }
        }

        public bool HasToken
        {
            get
            {
                if (!_CookieChecked)
                {
                    if (!RawOpenStackAuthCookie.IsNullOrEmpty())
                    {
                        var tok = OpenStackAuthCookie.Decode(_Protector, RawOpenStackAuthCookie);
                        if (!tok.IsExpired) _Token = tok;
                    }
                    _CookieChecked = true;
                }
                if (_Token != null && _Token.IsExpired && !_Token.Token.IsNullOrEmpty()) { _Token.Token = null; if (!_Token.SaveName) _Token.Name = null; if (!_Token.SaveAuthEndPoint) _Token.AuthAPIV3EndPoint = null; }
                return _Token != null && !_Token.IsExpired;
            }
        }

        string RawOpenStackAuthCookie => _HttpContext.Request.Cookies[OpenStackAuthCookie.CookieName];

    }
}
