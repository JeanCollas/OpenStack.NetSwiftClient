using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore
{
    public static class HttpRequestExts
    {
        public static bool IsLocalHost(this HttpRequest req) => (req?.Host.HasValue ?? false) && (req.Host.Host == "localhost");

        public static StringValues GetQueryParameter(this HttpRequest req, string key)
            => req.Query.TryGetValue(key, out StringValues val) ? val : StringValues.Empty;

        public static string GetBodyString(this HttpRequest req, bool rewind = true)
        {
            var bodyStr = "";

            // Allows using several time the stream in ASP.Net Core
            if (rewind)
                req.EnableRewind();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (StreamReader reader
                      = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEnd();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            if (rewind)
                req.Body.Position = 0;
            return bodyStr;
        }
    }
}
