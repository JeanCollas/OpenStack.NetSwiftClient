using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NetSwiftClient.Demo.AspNetCore.Controllers
{
    public class BaseController : Controller
    {
        public string Lang => "en";
        private IServiceProvider _Services;

        public IHostingEnvironment Env { get; }

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            _Services = httpContextAccessor.HttpContext.RequestServices;
            Env = _Services.GetService<IHostingEnvironment>();
        }



        #region JsonResults

        /// <summary>Standard way to return an AJAX response</summary>
        /// <param name="statusCode">Microsoft.AspNetCore.Http.StatusCodes</param>
        protected IActionResult JsonResult<T>(JsonResult<T> o = null)
            => JsonResult(o?.StatusCode ?? StatusCodes.Status200OK, o);

        // TODO: Async
        /// <summary>Standard way to return an AJAX response</summary>
        /// <param name="statusCode">Microsoft.AspNetCore.Http.StatusCodes</param>
        protected IActionResult JsonResult<T>(int statusCode = StatusCodes.Status200OK, JsonResult<T> o = null)
        {
            if (o != null && o.StatusCode == 0 && statusCode != 0) o.StatusCode = statusCode;
            if (o.StatusCode == 0) o.StatusCode = StatusCodes.Status200OK;

            if (!o.Success)
            {
                if (!o.ErrorCode.IsNullOrEmpty())
                {
                    o.Error = GetErrorMessage(o.ErrorCode).GetAwaiter().GetResult();
                }
                if (o.ErrorCodes != null && o.ErrorCodes.Count > 0)
                {
                    o.Errors = GetErrorMessages(o.ErrorCodes).GetAwaiter().GetResult();
                    //                    o.Error = GetErrorMessages(o.ErrorCodes[0]).GetAwaiter().GetResult();
                }
            }
            return JsonResult(o.StatusCode, (object)o);
        }

        /// <summary>Use JSonResult<T></summary>
        IActionResult JsonResult(int statusCode = StatusCodes.Status200OK, object o = null)
        {
            Response.StatusCode = statusCode;
            return new Microsoft.AspNetCore.Mvc.JsonResult(o);// { ContentType = "application/json; charset=utf-8" };
        }

        private async Task<List<string>> GetErrorMessages(List<string> errorCodes)
        {
            List<string> nok = new List<string>();
            List<string> msgs = new List<string>();
            foreach (var ec in errorCodes)
            {
                var ok = GetErrorMessage(ec, out string msg);
                if (ok) msgs.Add(msg);
                else nok.Add(ec);
            }
            if (nok.Count == 0)
                return msgs;

            //Dictionary<string, string> txts = new Dictionary<string, string>();
            //foreach (var error in nok)
            //    txts.Add("Errors." + error, null);
            //await TranslateAsync(txts);
            //msgs.AddRange(txts.Select(t => t.Value).Where(t => !t.IsNullOrEmpty()));
            return msgs;
        }

        private async Task<string> GetErrorMessage(string errorCode)
        {
            if (GetErrorMessage(errorCode, out string msg)) return msg;
            return errorCode;
                //await Translator.GetTranslationAsync("Errors." + errorCode, Lang, null, HttpContext);
        }

        /// <summary>Can be overriden to provide custom error messages</summary>
        protected virtual bool GetErrorMessage(string errorCode, out string msg, string lang = null)
        {
            lang = lang ?? Lang;
            //            if (lang != "en" && lang != "fr") { msg = null; return false; }
            return SiteErrorCodes.GetErrorMessage(errorCode, out msg, lang);
        }


        #endregion JsonResults


        protected bool GenericCheck<T>(Func<bool> ensureTrue, NetSwiftClient.Demo.AspNetCore.JsonResult<T> result, int statusCode, string errorCode, Func<string> redirectLink = null)
        {
            if (ensureTrue()) return true;
            if (redirectLink != null && !redirectLink().IsNullOrEmpty() && result.Result as StandardResult != null) (result.Result as StandardResult).RedirectLink = redirectLink();
            result.Success = false;
            result.StatusCode = statusCode;
            result.ErrorCode = errorCode;
            return false;
        }

    }
}
