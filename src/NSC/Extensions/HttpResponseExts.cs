using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace NetSwiftClient
{
    public static class HttpResponseExts
    {
        public static Dictionary<string, string> ToDictionary(this HttpResponseHeaders headers)
        {
            if (headers == null) return null;

            var result = new Dictionary<string, string>();

            foreach (var header in headers)
                result.Add(header.Key, header.Value.FirstOrDefault());

            return result;
        }
    }
}
