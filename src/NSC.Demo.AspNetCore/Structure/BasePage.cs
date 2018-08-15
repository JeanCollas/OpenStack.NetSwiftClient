using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore
{
    public abstract class BasePage<TModel> : RazorPage<TModel>
    { 
        //[RazorInject]
        //public TranslationService Translator { get; set; }
        [RazorInject]
        public IMemoryCache Cache { get; set; }
        [RazorInject]
        public IHtmlHelper Html2 { get; set; }
        [RazorInject]
        public CookieTokenService TokenService { get; set; }

#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

        public bool IsLocalHost => Context?.Request?.IsLocalHost() ?? false;// (Context?.Request?.Host.HasValue ?? false) && (Context.Request.Host.Value?.StartsWith("localhost") ?? false);

        public bool ShowKeys { get => _ShowKeys ?? (bool)(_ShowKeys = GetQueryParameter("showkeys") == "1"); }
        private bool? _ShowKeys;
        public bool LogKeys => ShowKeys;


        //bool? _IsSignedIn;
        //public bool IsSignedIn { get { if (_IsSignedIn == null) _IsSignedIn = SignInManager?.IsSignedIn(User); return _IsSignedIn ?? false; } }

        const string PAGE_ALL_KEYS = "PAGE_ALL_KEYS";

        protected BasePage()
        {
        }
        public override void BeginContext(int position, int length, bool isLiteral)
        {
            var allKeys = ViewData[PAGE_ALL_KEYS] as List<(string Page, Dictionary<string, string> Keys)>;
            if (allKeys==null) ViewData[PAGE_ALL_KEYS] = allKeys = new List<(string Page, Dictionary<string, string> Keys)>();
            if (!allKeys.Any(p=>p.Page == ViewFullFileName))
                allKeys.Add((ViewFullFileName, Texts));

            base.BeginContext(position, length, isLiteral);
        }

        public Dictionary<string, string> Texts { get; } = new Dictionary<string, string>();
        public List<(string Page, string Key)> TextsUsed => ViewData[PAGE_USED_KEYS] as List<(string Page, string Key)>??(List<(string Page, string Key)>)(ViewData[PAGE_USED_KEYS]=new List<(string Page, string Key)>()); 
        public List<(string Page, Dictionary<string, string> Keys)> AllTexts => ViewData[PAGE_ALL_KEYS] as List<(string Page, Dictionary<string,string> Keys)>??(List<(string Page, Dictionary<string,string> Keys)>)(ViewData[PAGE_USED_KEYS]=new List<(string Page, Dictionary<string, string> Keys)>()); 


        public string ViewPath => ViewContext.ExecutingFilePath;
        public string ViewFullFileName { get { var path = ViewPath; if (path.StartsWith("/Views/")) path = path.Substring("/Views/".Length); if (path.EndsWith(".cshtml")) path = path.Substring(0, path.Length - ".cshtml".Length); return path; } }
        public string ViewFileName => System.IO.Path.GetFileName(ViewContext.ExecutingFilePath);
        public string ViewFileNameNoExt => System.IO.Path.GetFileNameWithoutExtension(ViewContext.ExecutingFilePath);


        //string _Lang = null;
        //public string Lang { get { if (_Lang == null) _Lang = base.Context.GetLang(); return _Lang; } set { _Lang = value; _LangPrefix = null; } }
        public string Lang => "en";
        //public bool IsFrOrEn => IsFr || IsEn;
        //public bool IsFr => Lang == "fr";
        //public bool IsEn => Lang == "en";

        public static System.Globalization.CultureInfo CultureENUS = new System.Globalization.CultureInfo("en-US");
        System.Globalization.CultureInfo _Culture;

        public System.Globalization.CultureInfo Culture
        {
            get
            {
                if (_Culture == null)
                {
                    try
                    {
                        _Culture = new System.Globalization.CultureInfo(Lang);
                    }
                    catch
                    {
                        _Culture = new System.Globalization.CultureInfo("en");
                    }
                }
                return _Culture;
            }
            set
            {
                _Culture = value;
            }
        }


        //public async Task TranslateAsync(string forceLang = null)
        //{
        //    var lang = forceLang ?? Lang;
        //    List<string> nok = null;
        //    if (lang != "en" && lang != "fr")
        //    {
        //        if (Translator == null) throw new Exception("Translator null");
        //        nok = await Translator.GetTranslations(Texts, lang, Context?.Request);
        //    }
        //}
        public string T(string key, params object[] prms)
        {
            try
            {
                //if (!Texts.ContainsKey(key)) return "";
                var str = Texts[key];
                if (prms != null && prms.Length > 0) str = String.Format(str, prms);
                if (LogKeys) AddUsedKey(key);
                return str;
            }
            catch (KeyNotFoundException exc)
            {
                throw new KeyNotFoundException($"'{key}' not found in the dictionary", exc);
            }
        }

        public Microsoft.AspNetCore.Html.IHtmlContent T_JS(string key, params object[] prms)
        {
            var str = Texts[key];
            if (prms != null && prms.Length > 0) str = String.Format(str, prms);
            if (LogKeys) AddUsedKey(key);
            return Html2.Raw(str?.Replace("'", "\\'"));
        }
        public Microsoft.AspNetCore.Html.IHtmlContent T_Html(string key, params object[] prms)
        {
            var str = T(key, prms);
            var htmlStr = System.Net.WebUtility.HtmlDecode(str);
            if (LogKeys) AddUsedKey(key);
            return Html2.Raw(htmlStr);
        }

        const string PAGE_USED_KEYS = "PAGE_USED_KEYS";
        private void AddUsedKey(string key)
        {
            var keys = TextsUsed;
            if (!keys.Any(t => t.Page == ViewFullFileName && t.Key == key))
                keys.Add((ViewFullFileName, key));
        }



        public StringValues GetQueryParameter(string key)
            => Context.Request.GetQueryParameter(key);

        public void SetTitle(string title) { ViewData["Title"] = title; }
        public void SetDescription(string descr) { ViewData["Description"] = descr; }
        public List<string> Keywords
        {
            get
            {
                if (!ViewData.ContainsKey("Keywords")) ViewData["Keywords"] = new List<string>();
                return (List<String>)ViewData["Keywords"];
            }
        }

        public string SiteBasePath { get { return $"{Context.Request.Scheme}://{Context.Request.Host}"; } }
    }
}
