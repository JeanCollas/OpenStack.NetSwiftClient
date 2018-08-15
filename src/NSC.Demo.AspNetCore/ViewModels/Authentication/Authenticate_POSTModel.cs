using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore
{
    public class Authenticate_POSTModel
    {
        public string AuthAPIV3EndPoint { get; set; }
        public bool SaveAuthEndpoint { get; set; }
        public string Name { get; set; }
        public bool SaveName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; } = "Default";
        public bool SaveDomain { get; set; }

        public class ErrorCodes
        {
            public const string InvalidName = SiteErrorCodes.InvalidName;
            public const string InvalidPassword = SiteErrorCodes.InvalidPassword;
            public const string InvalidAuthEndpoint = SiteErrorCodes.InvalidAuthEndpoint;
        }

        public List<(string Field, bool Success, string ErrorCode)> Validate()
        {
            List<(string, bool, string)> results = new List<(string, bool, string)>();

            if (AuthAPIV3EndPoint.IsNullOrEmpty())
                results.Add((nameof(Authenticate_POSTModel.AuthAPIV3EndPoint), false, ErrorCodes.InvalidAuthEndpoint));
            else results.Add((nameof(Authenticate_POSTModel.AuthAPIV3EndPoint), true, null));
            if (Name.IsNullOrEmpty())
                results.Add((nameof(Authenticate_POSTModel.Name), false, ErrorCodes.InvalidName));
            else results.Add((nameof(Authenticate_POSTModel.Name), true, null));
            if (Password.IsNullOrEmpty())
                results.Add((nameof(Authenticate_POSTModel.Password), false, ErrorCodes.InvalidPassword));
            else results.Add((nameof(Authenticate_POSTModel.Password), true, null));

            return results;
        }
    }
}
