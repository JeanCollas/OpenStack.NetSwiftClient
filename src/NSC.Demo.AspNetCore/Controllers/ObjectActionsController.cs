using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NetSwiftClient.Demo.AspNetCore.Controllers
{
    public class ObjectActionsController : BaseController
    {
        private readonly SwiftClientService _SwiftService;
        private readonly CookieTokenService _TokenService;

        public ObjectActionsController(
            SwiftClientService swiftClientService,
            CookieTokenService tokenService,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _SwiftService = swiftClientService;
            _TokenService = tokenService;
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


    }
}
