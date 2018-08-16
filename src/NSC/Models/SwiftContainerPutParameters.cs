using System;
using System.Collections.Generic;
using System.Text;

namespace NetSwiftClient.Models
{
    public class SwiftContainerPutParameters
    {
        /// <summary>
        /// Sets a container access control list(ACL) that grants read access.The scope of the access is specific to the container. The ACL grants the ability to perform GET or HEAD operations on objects in the container or to perform a GET or HEAD operation on the container itself.
        /// The format and scope of the ACL is dependent on the authorization system used by the Object Storage service.See Container ACLs for more information.
        /// </summary>
        [HasHeader("X-Container-Read")]
        public string ContainerRead { get; set; }
        /// <summary>
        /// Sets a container access control list (ACL) that grants write access.The scope of the access is specific to the container. The ACL grants the ability to perform PUT, POST and DELETE operations on objects in the container. It does not grant write access to the container metadata.
        /// The format of the ACL is dependent on the authorization system used by the Object Storage service.See Container ACLs for more information.
        /// </summary>
        [HasHeader("X-Container-Write")]
        public string ContainerWrite { get; set; }
        /// <summary>
        /// Sets the destination for container synchronization. Used with the secret key indicated in the X -Container-Sync-Key header. If you want to stop a container from synchronizing, send a blank value for the X-Container-Sync-Key header.
        /// </summary>
        [HasHeader("X-Container-Sync-To")]
        public string ContainerSyncTo { get; set; }
        /// <summary>
        /// Sets the secret key for container synchronization. If you remove the secret key, synchronization is halted.For more information, see Container to Container Synchronization
        /// </summary>
        [HasHeader("X-Container-Sync-Key")]
        public string ContainerSyncKey { get; set; }
        /// <summary>
        /// The URL-encoded UTF-8 representation of the container that stores previous versions of objects. If neither this nor X-History-Location is set, versioning is disabled for this container.X-Versions-Location and X-History-Location cannot both be set at the same time.For more information about object versioning, see Object versioning.
        /// </summary>
        [HasHeader("X-Versions-Location")]
        public string VersionsLocation { get; set; }
        /// <summary>
        /// The URL-encoded UTF-8 representation of the container that stores previous versions of objects. If neither this nor X-Versions-Location is set, versioning is disabled for this container.X-History-Location and X-Versions-Location cannot both be set at the same time.For more information about object versioning, see Object versioning.
        /// </summary>
        [HasHeader("X-History-Location")]
        public string HistoryLocation { get; set; }
        /// <summary>
        /// Originating URLs allowed to make cross-origin requests(CORS), separated by spaces.This heading applies to the container only, and all objects within the container with this header applied are CORS-enabled for the allowed origin URLs. A browser (user-agent) typically issues a preflighted request , which is an OPTIONS call that verifies the origin is allowed to make the request. The Object Storage service returns 200 if the originating URL is listed in this header parameter, and issues a 401 if the originating URL is not allowed to make a cross-origin request. Once a 200 is returned, the browser makes a second request to the Object Storage service to retrieve the CORS-enabled object.
        /// </summary>
        [HasHeader("X-Container-Meta-Access-Control-Allow-Origin")]
        public string ContainerMetaAccessControlAllowOrigin { get; set; }
        /// <summary>
        /// Maximum time for the origin to hold the preflight results.A browser may make an OPTIONS call to verify the origin is allowed to make the request.Set the value to an integer number of seconds after the time that the request was received.
        /// </summary>
        [HasHeader("X-Container-Meta-Access-Control-Max-Age")]
        public string ContainerMetaAccessControlMaxAge { get; set; }
        /// <summary>
        /// Headers the Object Storage service exposes to the browser(technically, through the user-agent setting), in the request response, separated by spaces.By default the Object Storage service returns the following headers:
        /// All “simple response headers” as listed on http://www.w3.org/TR/cors/#simple-response-header.
        /// The headers etag, x-timestamp, x-trans-id, x-openstack-request-id.
        /// All metadata headers(X-Container-Meta-* for containers and X-Object-Meta-* for objects).
        /// headers listed in X-Container-Meta-Access-Control-Expose-Headers.
        /// </summary>
        [HasHeader("X-Container-Meta-Access-Control-Expose-Headers")]
        public string ContainerMetaAccessControlExposeHeaders { get; set; }
        /// <summary>
        /// Sets maximum size of the container, in bytes.Typically these values are set by an administrator.Returns a 413 response (request entity too large) when an object PUT operation exceeds this quota value.This value does not take effect immediately. see Container Quotas for more information.
        /// </summary>
        [HasHeader("X-Container-Meta-Quota-Bytes")]
        public string ContainerMetaQuotaBytes { get; set; }
        /// <summary>
        /// Sets maximum object count of the container.Typically these values are set by an administrator. Returns a 413 response (request entity too large) when an object PUT operation exceeds this quota value.This value does not take effect immediately. see Container Quotas for more information.
        /// </summary>
        [HasHeader("X-Container-Meta-Quota-Count")]
        public string ContainerMetaQuotaCount { get; set; }
        /// <summary>
        /// The secret key value for temporary URLs.
        /// </summary>
        [HasHeader("X-Container-Meta-Temp-URL-Key")]
        public string ContainerMetaTempURLKey { get; set; }
        /// <summary>
        /// A second secret key value for temporary URLs. The second key enables you to rotate keys by having two active keys at the same time.
        /// </summary>
        [HasHeader("X-Container-Meta-Temp-URL-Key-2")]
        public string ContainerMetaTempURLKey2 { get; set; }
        /// <summary>
        /// Extra transaction information. Use the X-Trans-Id-Extra request header to include extra information to help you debug any errors that might occur with large object upload and other Object Storage transactions.The server appends the first 32 characters of the X-Trans-Id-Extra request header value to the transaction ID value in the generated X-Trans-Id response header.You must UTF-8-encode and then URL-encode the extra transaction information before you include it in the X-Trans-Id-Extra request header.For example, you can include extra transaction information when you upload large objects such as images.When you upload each segment and the manifest, include the same value in the X-Trans-Id-Extra request header.If an error occurs, you can find all requests that are related to the large object upload in the Object Storage logs. You can also use X-Trans-Id-Extra strings to help operators debug requests that fail to receive responses. The operator can search for the extra information in the logs.
        /// </summary>
        [HasHeader("X-Trans-Id-Extra")]
        public string TransIdExtra { get; set; }
        /// <summary>
        /// In requests, specifies the name of the storage policy to use for the container. In responses, is the storage policy name. The storage policy of the container cannot be changed.
        /// </summary>
        [HasHeader("X-Storage-Policy")]
        public string StoragePolicy { get; set; }
        /// <summary>
        /// The container metadata, key will be prefixed 'X-Container-Meta-', it is the name of metadata item. You must specify an X-Container-Meta-name header for each metadata item (for each name) that you want to add or update.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        public Dictionary<string, string> GetHeaders()
        {
            var dico = new Dictionary<string, string>();
            foreach (var prop in this.GetType().GetProperties())
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (!(attr is HasHeaderAttribute a)) continue;
                    var val = prop.GetValue(this)?.ToString();
                    if (!val.IsNullOrEmpty())
                        dico.Add(a.Name, val);
                }
            }

            if (Metadata != null)
                foreach (var kvp in Metadata)
                    dico.Add(SwiftHeaders.ContainerMetaPrefix + kvp.Key, kvp.Value);

            return dico;
        }
    }
}
