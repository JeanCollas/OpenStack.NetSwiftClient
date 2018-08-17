using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetSwiftClient.Demo.AspNetCore.Models;
using NetSwiftClient.Models;
using Newtonsoft.Json;

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
            if (accountUrl.IsNullOrEmpty())
            {
                var authRes = await _SwiftService.AuthenticateTokenAsync(_TokenService.Token.AuthAPIV3EndPoint, _TokenService.Token.Token);
                return View("/Views/Explorer/EnterAccountUrl.cshtml", authRes);
            }
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

        public class ContainerAdd_PUTModel: NetSwiftClient.Models.SwiftContainerPutParameters
        {
            public string Container { get; set; }
        }
        [HttpPut("/ContainerAdd", Name = Routes.PUT_ContainerAdd_Route)]
        public async Task<IActionResult> NewContainer(string accountUrl, [FromBody]ContainerAdd_PUTModel model)
        {
            var response = new JsonResult<StandardResult>() { Success = false, Result = new StandardResult() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized))
                return JsonResult(response);

            if (!GenericCheck(() => !accountUrl.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidAccountUrl))
                return JsonResult(response);
            if (!GenericCheck(() => !model.Container.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidContainer))
                return JsonResult(response);

            if (!GenericCheck(() => model != null, response, StatusCodes.Status400BadRequest, SiteErrorCodes.BadRequest))
                return JsonResult(response);

            _SwiftService.InitToken(_TokenService.Token.Token);
            var swiftResult = await _SwiftService.ContainerPutAsync(accountUrl, model.Container, model);

            if (!GenericCheck(() => swiftResult.IsSuccess, response, StatusCodes.Status400BadRequest, swiftResult.Reason))
                return JsonResult(response);

            response.Success = true;
            return JsonResult(StatusCodes.Status200OK, response);
        }

        [HttpDelete("/Delete/{container}/{objectName}", Name = Routes.DELETE_DeleteObject_Route)]
        [HttpDelete("/Delete/{container}", Name = Routes.DELETE_DeleteContainer_Route)]
        public async Task<IActionResult> Delete(string accountUrl, string container, string objectName=null)
        {
            var response = new JsonResult<StandardResult>() { Success = false, Result = new StandardResult() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized))
                return JsonResult(response);

            if (!GenericCheck(() => !accountUrl.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidAccountUrl))
                return JsonResult(response);
            if (!GenericCheck(() => !container.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidContainer))
                return JsonResult(response);
            //if (!GenericCheck(() => !objectName.IsNullOrEmpty(), response, StatusCodes.Status400BadRequest, SiteErrorCodes.InvalidObject))
            //    return JsonResult(response);

            _SwiftService.InitToken(_TokenService.Token.Token);

            SwiftBaseResponse swiftResult;
            
            if(objectName.IsNullOrEmpty()) 
                // If object name is empty then it is a delete on container. If the container is not empty, it will fail.
                swiftResult = await _SwiftService.ContainerDeleteAsync(accountUrl, container);
            else 
                // If object name not empty, try to delete the file.
                swiftResult = await _SwiftService.ObjectDeleteAsync(accountUrl, container, objectName);

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
