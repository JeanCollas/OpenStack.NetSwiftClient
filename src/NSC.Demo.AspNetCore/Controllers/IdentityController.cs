using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NetSwiftClient.Demo.AspNetCore.Controllers
{
    public class IdentityController : BaseController
    {
        private readonly SwiftClientService _SwiftService;
        private readonly CookieTokenService _TokenService;

        public IdentityController(
            SwiftClientService swiftClientService,
            CookieTokenService tokenService,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _SwiftService = swiftClientService;
            _TokenService = tokenService;
        }

        public class AuthResult : StandardResult
        {
            public string AuthResponse { get; set; }
        }
        [HttpPost("/Authenticate", Name = Routes.POST_Authenticate_Route)]
        public async Task<IActionResult> Authenticate([FromBody]Authenticate_POSTModel model)
        {
            var response = new JsonResult<AuthResult>() { Success = false, Result = new AuthResult() };

            var valids = model.Validate();
            if (valids.Any(r => !r.Success))
            {
                response.ErrorCodes = valids.Where(r => !r.Success).Select(r => r.ErrorCode).ToList();
                response.Success = false; response.StatusCode = StatusCodes.Status400BadRequest;
                return JsonResult(response);
            }

            var authRes = await _SwiftService.AuthenticateAsync(model.AuthAPIV3EndPoint, model.AuthName, model.Password, model.Domain);
            if (!GenericCheck(() => authRes.IsSuccess, response, (int)authRes.StatusCode, SiteErrorCodes.InvalidCredentials))
            {
                response.Error += "\n" + authRes.Reason;
                return JsonResult(response);
            }

            // store token
            _TokenService.Token = new OpenStackAuthCookie()
            {
                AuthAPIV3EndPoint = model.SaveAuthEndpoint ? model.AuthAPIV3EndPoint : null,
                Name = model.SaveName ? model.AuthName : null,
                Domain = model.SaveDomain ? model.Domain : null,
                ExpirationTime = authRes.TokenExpires ?? DateTime.MaxValue,
                Token = authRes.Token,
                SaveAuthEndPoint = model.SaveAuthEndpoint,
                SaveName = model.SaveName,
                SaveDomain = model.SaveDomain,
            };

            response.Result.AuthResponse = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(authRes.ContentStr), Formatting.Indented);
            response.Success = true;
            //response.Result.RedirectLink = Url.RouteUrl(Routes.GET_Explore_Route);
            return JsonResult(StatusCodes.Status200OK, response);

        }

        [HttpPost("/AuthenticateToken", Name = Routes.POST_AuthenticateToken_Route)]
        public async Task<IActionResult> AuthenticateToken()
        {
            var response = new JsonResult<AuthResult>() { Success = false, Result = new AuthResult() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized))
                return JsonResult(response);

            var authRes = await _SwiftService.AuthenticateTokenAsync(_TokenService.Token.AuthAPIV3EndPoint, _TokenService.Token.Token);
            if (!GenericCheck(() => authRes.IsSuccess, response, (int)authRes.StatusCode, SiteErrorCodes.InvalidCredentials))
            {
                response.Error += "\n" + authRes.Reason;
                return JsonResult(response);
            }

            response.Result.AuthResponse = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(authRes.ContentStr), Formatting.Indented);
            response.Success = true;
            //response.Result.RedirectLink = Url.RouteUrl(Routes.GET_Explore_Route);
            return JsonResult(StatusCodes.Status200OK, response);

        }


    }
}
