using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore
{
    public static class Routes
    {
        public const string GET_Home_Route = nameof(GET_Home_Route);
        public const string GET_Explore_Route = nameof(GET_Explore_Route);
        public const string GET_DownloadObject_Route = nameof(GET_DownloadObject_Route);
        public const string POST_TempLink_Route = nameof(POST_TempLink_Route);
        public const string POST_SetTempKey_Route = nameof(POST_SetTempKey_Route);

        public const string DELETE_DeleteObject_Route = nameof(DELETE_DeleteObject_Route);
        public const string POST_UploadFile_Route = nameof(POST_UploadFile_Route);
        public const string POST_UploadFiles_Route = nameof(POST_UploadFiles_Route);

        public const string POST_Authenticate_Route = nameof(POST_Authenticate_Route);
        public const string POST_Authenticate_JSONRoute = nameof(POST_Authenticate_JSONRoute);
    }
}
