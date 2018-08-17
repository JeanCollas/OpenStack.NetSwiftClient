using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NetSwiftClient.Models;

namespace NetSwiftClient
{
    public class SwiftClientService : SwiftClient
    {
        public SwiftClientConfig Config { get; set; }
        public string ObjectStoreUrl => Config?.ObjectStoreUrl;
        public string Token { get; set; }
        /// <summary>If no config is provided</summary>
        public SwiftClientService()
        {
        }
        public SwiftClientService(SwiftClientConfig config)
        {
            Config = config;
            if (!config.Token.IsNullOrEmpty()) Token = config.Token;
        }
        public Task<SwiftAuthV3Response> AuthenticateAsync()
            => AuthenticateAsync(Config.AuthUrl, Config.Name, Config.Password, Config.Domain);


        #region Account
        public Task<SwiftAccountDetailsResponse> AccountListContainersAsync()
            => AccountListContainersAsync(Config.ObjectStoreUrl);

        public Task<SwiftAccountDetailsResponse> AccountHeadAsync()
            => AccountHeadAsync(Config.ObjectStoreUrl);

        public Task<SwiftBaseResponse> AccountPostAsync(Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null)
            => AccountPostAsync(Config.ObjectStoreUrl, metaValues, additionalHeaders);

        public Task<SwiftBaseResponse> AccountSetTempUrlKeyAsync(string key, bool firstKey = true)
            => AccountSetTempUrlKeyAsync(Config.ObjectStoreUrl, key, firstKey);
        #endregion Account

        #region Container

        public Task<SwiftContainerInfoResponse> ContainerGetAsync(string container)
            => ContainerGetAsync(Config.ObjectStoreUrl, container);

        public Task<SwiftBaseResponse> ContainerDeleteAsync(string container)
            => ContainerDeleteAsync(Config.ObjectStoreUrl, container);

        public Task<SwiftContainerInfoResponse> ContainerHeadAsync(string container)
            => ContainerHeadAsync(Config.ObjectStoreUrl, container);

        public Task<SwiftBaseResponse> ContainerPostAsync(string container, Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null)
            => ContainerPostAsync(Config.ObjectStoreUrl, container, metaValues, additionalHeaders);

        public Task<SwiftBaseResponse> ContainerPutAsync(string container, Dictionary<string, string> metadata = null)
            => ContainerPutAsync(Config.ObjectStoreUrl, container, metadata: metadata);

        #endregion Container

        #region Object


        public Task<SwiftObjectGetResponse> ObjectGetAsync(string container, string objectName)
            => ObjectGetAsync(Config.ObjectStoreUrl, container, objectName);

        public string ObjectGetTmpUrlAsync(string container, string objectName, TimeSpan expiresIn, string objectStoreKey, string fileName = null, string ipRange = null, bool? noDownloadButInline = null)
            => ObjectGetTmpUrlAsync(Config.ObjectStoreUrl, container, objectName, expiresIn, objectStoreKey, fileName, ipRange, noDownloadButInline);

        public Task<SwiftObjectGetResponse> ObjectHeadAsync(string container, string objectName)
            => base.ObjectHeadAsync(Config.ObjectStoreUrl, container, objectName);

        public Task<SwiftBaseResponse> ObjectPostAsync(string container, string objectName, Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null)
            => ObjectPostAsync(Config.ObjectStoreUrl, container, objectName, metaValues, additionalHeaders);

        public Task<SwiftBaseResponse> ObjectPutAsync(string container, string objectName, Stream data)
            => ObjectPutAsync(Config.ObjectStoreUrl, container, objectName, data);

        public Task<SwiftBaseResponse> ObjectPutAsync(string container, string objectName, byte[] data)
            => ObjectPutAsync(Config.ObjectStoreUrl, container, objectName, data);

        public Task<SwiftBaseResponse> ObjectDeleteAsync(string container, string objectName)
            => ObjectDeleteAsync(Config.ObjectStoreUrl, container, objectName);
        #endregion Object
    }
}
