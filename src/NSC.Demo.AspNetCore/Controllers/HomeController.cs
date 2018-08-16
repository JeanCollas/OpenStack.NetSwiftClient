using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetSwiftClient.Demo.AspNetCore.Models;

namespace NetSwiftClient.Demo.AspNetCore.Controllers
{
    public partial class HomeController : BaseController
    {
        private readonly SwiftClientService _SwiftService;
        private readonly CookieTokenService _TokenService;

        public HomeController(
            SwiftClientService swiftClientService,
            CookieTokenService tokenService,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _SwiftService = swiftClientService;
            _TokenService = tokenService;
        }

        [HttpGet("/", Name = Routes.GET_Home_Route)]
        [HttpGet("/Explore", Name = Routes.GET_Explore_Route)]
        public async Task<IActionResult> Index(string accountUrl = null, string container = null, string objectName = null)
        {
            if (!_TokenService.HasToken) return View("/Views/Home/Index.cshtml");
            if (accountUrl.IsNullOrEmpty()) return View("/Views/Explorer/EnterAccountUrl.cshtml");
            _SwiftService.InitToken(_TokenService.Token.Token);
            if (container.IsNullOrEmpty())
            {
                var ss = await _SwiftService.AccountListContainersAsync(accountUrl);
                return View("/Views/Explorer/GetAccount.cshtml", ss);
            }
            if (objectName.IsNullOrEmpty())
            {
                var ss = await _SwiftService.ContainerGetAsync(accountUrl, container);
                return View("/Views/Explorer/GetContainer.cshtml", ss);
            }

            {
                var ss = await _SwiftService.ObjectHeadAsync(accountUrl, container, objectName);
                return View("/Views/Explorer/GetObject.cshtml", ss);
            }
        }

        [HttpPost("/Authenticate", Name = Routes.POST_Authenticate_JSONRoute)]
        public async Task<IActionResult> Authenticate([FromBody]Authenticate_POSTModel model)
        {
            var response = new JsonResult<StandardResult>() { Success = false, Result = new StandardResult() };

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

            response.Success = true;
            response.Result.RedirectLink = Url.RouteUrl(Routes.GET_Explore_Route);
            return JsonResult(StatusCodes.Status200OK, response);

        }

        [HttpGet("/ObjectDownload", Name = Routes.GET_DownloadObject_Route)]
        public async Task<IActionResult> Download(string accountUrl, string container, string objectName)
        {
            if (!_TokenService.HasToken || accountUrl.IsNullOrEmpty() || container.IsNullOrEmpty() || objectName.IsNullOrEmpty()) return RedirectToRoute(Routes.GET_Home_Route);

            _SwiftService.InitToken(_TokenService.Token.Token);

            var ss = await _SwiftService.ObjectGetAsync(accountUrl, container, objectName);
            if (!ss.IsSuccess)
                return RedirectToRoute(Routes.GET_Home_Route);
            if (Request.GetQueryParameter("disposition").Count > 0)
                Response.Headers["Content-Disposition"] = Request.GetQueryParameter("disposition");
            Response.ContentType = ss.ContentType;
            Response.ContentLength = ss.ContentLength;

            return File(ss.ObjectStreamContent, ss.ContentType.IfNullOrEmpty("application/octet-stream"), objectName);
        }

        [HttpDelete("/ObjectDelete", Name = Routes.DELETE_DeleteObject_Route)]
        public async Task<IActionResult> Delete(string accountUrl, string container, string objectName)
        {
            var response = new JsonResult<StandardResult>() { Success = false, Result = new StandardResult() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized))
                return JsonResult(response);

            if (!GenericCheck(() => !accountUrl.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidAccountUrl))
                return JsonResult(response);
            if (!GenericCheck(() => !container.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidContainer))
                return JsonResult(response);
            if (!GenericCheck(() => !objectName.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidObject))
                return JsonResult(response);

            _SwiftService.InitToken(_TokenService.Token.Token);

            var swiftResp = await _SwiftService.ObjectDeleteAsync(accountUrl, container, objectName);

            if (!GenericCheck(() => swiftResp.IsSuccess, response, StatusCodes.Status400BadRequest, swiftResp.Reason))
                return JsonResult(response);

            response.Success = true;
            return JsonResult(StatusCodes.Status200OK, response);
        }


        class TempLinkResult : StandardResult { public string Link { get; set; } }

        public class ObjectCreateTempLink_POSTModel { public int ValidityMinutes { get; set; } }
        [HttpPost("/ObjectTempLink", Name = Routes.POST_TempLink_Route)]
        public async Task<IActionResult> TempLink(string accountUrl, string container, string objectName, [FromBody]ObjectCreateTempLink_POSTModel model)
        {
            var response = new JsonResult<TempLinkResult>() { Success = false, Result = new TempLinkResult() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized))
                return JsonResult(response);

            if (!GenericCheck(() => !accountUrl.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidAccountUrl))
                return JsonResult(response);
            if (!GenericCheck(() => !container.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidContainer))
                return JsonResult(response);
            if (!GenericCheck(() => !objectName.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidObject))
                return JsonResult(response);

            _SwiftService.InitToken(_TokenService.Token.Token);
            var account = await _SwiftService.AccountHeadAsync(accountUrl);
            bool keysMissing = account.TempKey.IsNullOrEmpty() && account.TempKey2.IsNullOrEmpty();
            if (!GenericCheck(() => !keysMissing, response, StatusCodes.Status400BadRequest, SiteErrorCodes.TempUrlKeysNotSet))
                return JsonResult(response);

            var key = account.TempKey.IfNullOrEmpty(account.TempKey2);

            var url = _SwiftService.ObjectGetTmpUrlAsync(accountUrl, container, objectName, TimeSpan.FromMinutes(model.ValidityMinutes), key);

            response.Success = true;
            response.Result.Link = url;
            return JsonResult(StatusCodes.Status200OK, response);
        }


        public class AccountSetTempKey_POSTModel { public string NewKey { get; set; } }
        [HttpPost("/AccountSetTempKey", Name = Routes.POST_SetTempKey_Route)]
        public async Task<IActionResult> TempLink(string accountUrl, int number, [FromBody]AccountSetTempKey_POSTModel model)
        {
            var response = new JsonResult<StandardResult>() { Success = false, Result = new StandardResult() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized))
                return JsonResult(response);

            if (!GenericCheck(() => !accountUrl.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidAccountUrl))
                return JsonResult(response);
            if (!GenericCheck(() => number == 1 || number == 2, response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidKeyNumber))
                return JsonResult(response);

            if (!GenericCheck(() => model != null, response, StatusCodes.Status400BadRequest, SiteErrorCodes.BadRequest))
                return JsonResult(response);

            _SwiftService.InitToken(_TokenService.Token.Token);
            var swiftResult = await _SwiftService.AccountSetTempUrlKeyAsync(accountUrl, model.NewKey, number == 1);

            if (!GenericCheck(() => swiftResult.IsSuccess, response, StatusCodes.Status400BadRequest, swiftResult.Reason))
                return JsonResult(response);

            response.Success = true;
            return JsonResult(StatusCodes.Status200OK, response);
        }

        public class ContainerAdd_POSTModel: NetSwiftClient.Models.SwiftContainerPutParameters
        {
        }
        [HttpPut("/ContainerAdd", Name = Routes.PUT_ContainerAdd_Route)]
        public async Task<IActionResult> NewContainer(string accountUrl, string container, [FromBody]ContainerAdd_POSTModel model)
        {
            var response = new JsonResult<StandardResult>() { Success = false, Result = new StandardResult() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized))
                return JsonResult(response);

            if (!GenericCheck(() => !accountUrl.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidAccountUrl))
                return JsonResult(response);
            if (!GenericCheck(() => !container.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidContainer))
                return JsonResult(response);

            if (!GenericCheck(() => model != null, response, StatusCodes.Status400BadRequest, SiteErrorCodes.BadRequest))
                return JsonResult(response);

            _SwiftService.InitToken(_TokenService.Token.Token);
            var swiftResult = await _SwiftService.ContainerPutAsync(accountUrl, container, model);

            if (!GenericCheck(() => swiftResult.IsSuccess, response, StatusCodes.Status400BadRequest, swiftResult.Reason))
                return JsonResult(response);

            response.Success = true;
            return JsonResult(StatusCodes.Status200OK, response);
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
