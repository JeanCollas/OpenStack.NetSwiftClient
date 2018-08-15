using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NetSwiftClient
{
    public static class HttpMessageExts
    {
        public static void FillTokenHeader(this HttpRequestMessage req, string token)
        {
            req.Headers.Add(SwiftHeaders.AuthToken, token);
        }
    }
}
