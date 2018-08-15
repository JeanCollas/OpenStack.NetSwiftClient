using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System;

namespace NetSwiftClient.Demo.AspNetCore
{
    public class OpenStackAuthCookie
    {
        public const string CookieName = "OSAC";

        public string AuthAPIV3EndPoint { get; set; }
        public bool SaveAuthEndPoint { get; set; }

        public string Name { get; set; }
        public bool SaveName { get; set; }

        public string Domain { get; set; }
        public bool SaveDomain { get; set; }

        public string Token { get; set; }
        public DateTime ExpirationTime { get; set; }

        // Ensure 10s to complete the operations
        public bool IsExpired => ExpirationTime < DateTime.UtcNow.AddSeconds(10);

        public string Encode(IDataProtector protector) => protector.Protect(JsonConvert.SerializeObject(this));
        public static OpenStackAuthCookie Decode(IDataProtector protector, string cookieStr) => JsonConvert.DeserializeObject<OpenStackAuthCookie>(protector.Unprotect(cookieStr));
    }
}
