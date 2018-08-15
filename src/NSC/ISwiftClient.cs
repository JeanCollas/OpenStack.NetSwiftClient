using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NetSwiftClient.Models;

namespace NetSwiftClient
{
    public interface ISwiftClient
    {
        Task<SwiftAccountDetailsResponse> AccountHeadAsync(string objectStoreUrl);
        Task<SwiftAccountDetailsResponse> AccountListContainersAsync(string objectStoreUrl, int? limit = null, string marker = null, string endMarker = null, string format = null, string prefix = null, string delimiter = null);
        Task<SwiftBaseResponse> AccountPostAsync(string objectStoreUrl, Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null);
        Task<SwiftBaseResponse> AccountSetTempUrlKeyAsync(string objectStoreUrl, string key, bool firstKey = true);
        Task<SwiftAuthV3Response> AuthenticateAsync(string authUrl, string name, string password, string domain = "Default");
        Task<SwiftBaseResponse> ContainerDeleteAsync(string objectStoreUrl, string container);
        Task<SwiftContainerInfoResponse> ContainerGetAsync(string objectStoreUrl, string container);
        Task<SwiftContainerInfoResponse> ContainerHeadAsync(string objectStoreUrl, string container);
        Task<SwiftBaseResponse> ContainerPostAsync(string objectStoreUrl, string container, Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null);
        Task<SwiftBaseResponse> ContainerPutAsync(string objectStoreUrl, string container, Dictionary<string, string> metadata = null);
        void InitToken(string token, DateTime? expiresAt = null);
        Task<SwiftBaseResponse> ObjectDeleteAsync(string objectStoreUrl, string container, string objectName);
        Task<SwiftObjectGetResponse> ObjectGetAsync(string objectStoreUrl, string container, string objectName);
        string ObjectGetTmpUrlAsync(string objectStoreUrl, string container, string objectName, TimeSpan expiresIn, string objectStoreKey, string fileName = null, string ipRange = null, bool? noDownloadButInline = null);
        Task<SwiftContainerInfoResponse> ObjectHeadAsync(string objectStoreUrl, string container);
        Task<SwiftBaseResponse> ObjectPostAsync(string objectStoreUrl, string container, string objectName, Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null);
        Task<SwiftBaseResponse> ObjectPutAsync(string objectStoreUrl, string container, string objectName, byte[] data, string contentType = "application/octet-stream");
        Task<SwiftBaseResponse> ObjectPutAsync(string objectStoreUrl, string container, string objectName, Stream data, string contentType = "application/octet-stream");
    }
}