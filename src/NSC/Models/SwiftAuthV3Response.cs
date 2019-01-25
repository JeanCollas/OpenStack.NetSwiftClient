using System;
using System.Collections.Generic;
using System.Text;

namespace NetSwiftClient.Models
{
    public class SwiftAuthV3Response : SwiftBaseResponse
    {
        public string Token { get { string token = null; if (!Headers.TryGetValue(SwiftHeaders.AuthTokenReponse, out token)) return null; return token; } }
        public DateTime? TokenExpires => ContentObject?.Token?.Expires_at;

        public string ContentStr { get; set; }
        public TokenContainerObject ContentObject { get; set; }

        public class TokenContainerObject
        {
            public TokenObject Token { get; set; }
        }
        public class TokenObject
        {
            public List<string> Methods { get; set; }
            public DateTime? Expires_at { get; set; }
            public UserObject User { get; set; }
            public List<string> Audit_ids { get; set; }
            public List<CatalogItem> Catalog { get; set; }
            public DateTime? Issued_at { get; set; }

            public SystemObject System { get; set; }
            public DomainObject Domain { get; set; }
            public ProjectObject Project { get; set; }
        }
        public class CatalogItem
        {
            public string Type { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public List<EndpointObject> EndPoints { get; set; }
        }
        public class EndpointObject
        {
            public string RegionId { get; set; }
            public string Url { get; set; }
            public string Region { get; set; }
            public string Interface { get; set; }
            public string Id { get; set; }
        }

        public class UserObject
        {
            public DomainObject Domain { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime? Password_expires_at { get; set; }
        }
        public class DomainObject
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class SystemObject
        {
            public bool All { get; set; }
        }
        public class ProjectObject
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DomainObject Domain { get; set; }
        }

    }
}

// https://developer.openstack.org/api-ref/identity/v3/

//{
//    "token": {
//        "methods": [
//            "password"
//        ],
//        "expires_at": "2015-11-06T15:32:17.893769Z",
//        "user": {
//            "domain": {
//                "id": "default",
//                "name": "Default"
//            },
//            "id": "423f19a4ac1e4f48bbb4180756e6eb6c",
//            "name": "admin",
//            "password_expires_at": null
//        },
//        "audit_ids": [
//            "ZzZwkUflQfygX7pdYDBCQQ"
//        ],
//        "issued_at": "2015-11-06T14:32:17.893797Z"
//    }
//}

//Success¶
//Code Reason
//201 - Created Resource was created and is ready to use.
//Error¶
//Code Reason
//400 - Bad Request   Some content in the request was invalid.
//401 - Unauthorized User must authenticate before making a request.
//403 - Forbidden Policy does not allow current user to do this operation.
//404 - Not Found The requested resource could not be found.
//405 - Method Not Allowed Method is not valid for this endpoint.
//409 - Conflict This operation conflicted with another operation on this resource.
//413 - Request Entity Too Large  The request is larger than the server is willing or able to process.
//415 - Unsupported Media Type The request entity has a media type which the server or resource does not support.
//503 - Service Unavailable   Service is not available. This is mostly caused by service configuration errors which prevents the service from successful start up.