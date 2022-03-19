using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NetSwiftClient.Models
{
    public class SwiftBaseResponse
    {
        private const string DefaultDecimalSeparator = ".";
        private const string TimestampHeaderName = "X-Timestamp";

        public SwiftBaseResponse() { }
        public SwiftBaseResponse(HttpResponseMessage resp) { PrefillFromResponse(resp); }

        public HttpStatusCode StatusCode { get; set; }

        public string Reason { get; set; }

        [HasHeader(SwiftHeaders.ContentLength)]
        public long ContentLength { get; set; }

        [HasHeader(SwiftHeaders.ContentType)]
        public string ContentType { get; set; }

        [HasHeader(SwiftHeaders.TransactionId)]
        public string TransactionId { get; set; }

        [HasHeader(SwiftHeaders.OpenstackRequestId)]
        public string OpenstackRequestId { get; set; }

        [HasHeader(SwiftHeaders.Date)]
        public string Date { get; set; }


        public bool IsSuccess { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public void PrefillFromResponse(HttpResponseMessage resp)
        {
            ContentLength = resp.Content?.Headers?.ContentLength ?? 0;
            ContentType = resp.Content?.Headers?.ContentType?.ToString() ?? "application/octet-stream";
            IsSuccess = resp.IsSuccessStatusCode;
            Headers = resp.Headers.ToDictionary();
            Reason = resp.ReasonPhrase;
            StatusCode = resp.StatusCode;

            var decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (!decimalSeparator.Equals(DefaultDecimalSeparator) &&
                Headers.ContainsKey(TimestampHeaderName) && Headers[TimestampHeaderName].Contains(DefaultDecimalSeparator))
            {
                Headers[TimestampHeaderName] = Headers[TimestampHeaderName].Replace(DefaultDecimalSeparator, decimalSeparator);
            }

            foreach (var prop in this.GetType().GetProperties())
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (!(attr is HasHeaderAttribute a)) continue;
                    if (Headers.Keys.Any(k => k.ToLower() == a.Name.ToLower()))
                        prop.SetValue(this, Headers[Headers.Keys.First(h => h.ToLower() == a.Name.ToLower())]
                            .SafeConvert(prop.PropertyType, () => prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null));
                }
            }

            if (this is ISwiftResponseWithStream ws)
            {
                /// ... To make better
                ws.ObjectStreamContent = resp.Content.ReadAsStreamAsync().Result;
            }
        }
    }

    public class SwiftBaseResponse<T> : SwiftBaseResponse
    {
        public SwiftBaseResponse() { }
        /// <summary>Prefills the response headers, but not the content</summary>
        public SwiftBaseResponse(HttpResponseMessage resp) : base(resp) { }

        public string ContentStr { get; set; }
        public T ContentObject => JsonConvert.DeserializeObject<T>(ContentStr);
        public async Task PrefillFromResponse(HttpResponseMessage resp, bool includeContent = false)
        {
            base.PrefillFromResponse(resp);
            if (includeContent) await PrefillContentAsync(resp);
        }
        public async Task PrefillContentAsync(HttpResponseMessage resp)
        {
            ContentStr = await resp.Content.ReadAsStringAsync();
        }
    }
}
