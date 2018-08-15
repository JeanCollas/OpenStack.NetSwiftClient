using System;

namespace NetSwiftClient
{
    public static class SwiftHeaders
    {
        public const string ContentType = "Content-Type";
        public const string ContentLength = "Content-Length";
        /// <summary>If present, specifies the override behavior for the browser. For example, this header might specify that the browser use a download program to save this file rather than show the file, which is the default. If not set, this header is not returned by this operation.</summary>
        public const string ContentDisposition = "Content-Disposition";
        public const string AcceptRanges = "Accept-Ranges";
        public const string LastModified = "Last-Modified";
        public const string Date = "Date";
        /// <summary>For objects smaller than 5 GB, this value is the MD5 checksum of the object content. The value is not quoted. For manifest objects, this value is the MD5 checksum of the concatenated string of ETag values for each of the segments in the manifest, and not the MD5 checksum of the content that was downloaded. Also the value is enclosed in double-quote characters. You are strongly recommended to compute the MD5 checksum of the response body as it is received and compare this value with the one in the ETag header. If they differ, the content was corrupted, so retry the operation.</summary>
        public const string ETag = "ETag";
        //public const string Expires = "expires";
        //public const string Key = "key";
        //public const string HmacBody = "hmac_body";
        //public const string Sig = "sig";
        //public const string IPRange = "ip_range";

        public const string DeleteAt = "X-Delete-At";
        public const string XTimestamp = "X-Timestamp";
        public const string TransactionId = "X-Trans-Id";
        public const string OpenstackRequestId = "X-Openstack-Request-Id";
        

        public const string AccountMetaPrefix = "X-Account-Meta-";
        public const string AccountMetaTempUrlKey = "X-Account-Meta-Temp-URL-Key";
        public const string AccountMetaTempUrlKey2 = "X-Account-Meta-Temp-URL-Key-2";
        public const string AccountContainerCount = "X-Account-Container-Count";
        public const string AccountObjectCount = "X-Account-Object-Count";
        public const string AccountBytesUsed = "X-Account-Bytes-Used";


        public const string AuthToken = "X-Auth-Token";
        public const string AuthTokenReponse = "X-Subject-Token";

        public const string ContainerMetaPrefix = "X-Container-Meta-";
        public const string ContainerMetaName = "X-Container-Meta-name";
        public const string ContainerObjectCount = "X-Container-Object-Count";
        public const string ContainerBytesUsed = "X-Container-Bytes-Used";
        public const string ContainerAcceptRanges = AcceptRanges;

        public const string ObjectMetaPrefix = "X-Object-Meta-";
        public const string ObjectMetaName = "X-Object-Meta-name";
        public const string ObjectManifest = "X-Object-Manifest";
        /// <summary>Set to true if this object is a static large object manifest object.</summary>
        public const string ObjectStaticLargeObject = "X-Static-Large-Object";
        public const string SymlinkTarget = "X-Symlink-Target";
        public const string SymlinkTargetAccount = "X-Symlink-Target-Account";


    }
    public class HasHeaderAttribute:Attribute
    {
        public string Name { get; }

        public HasHeaderAttribute(string name)
        {
            Name = name;
        }

    }
}
