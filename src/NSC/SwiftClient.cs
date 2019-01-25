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
    public class SwiftClient : ISwiftClient
    {
        private readonly HttpClient _Client = new HttpClient();
        private string _Token;
        private string Token { get => DateTime.UtcNow < TokenExpiresAt ? _Token : null; set => _Token = value; }
        private DateTime TokenExpiresAt { get; set; }

        #region Identity/Authentication
        public Task<SwiftAuthV3Response> AuthenticateAsync(string authUrl, string name, string password, string domain = "Default")
        {
            var reqObj = new SwiftAuthV3Request(name, password, domain);
            return AuthenticateAsync(authUrl, reqObj);
        }
        public Task<SwiftAuthV3Response> AuthenticateTokenAsync(string authUrl, string token)
        {
            var reqObj = new SwiftAuthV3Request(token);
            return AuthenticateAsync(authUrl, reqObj);
        }

        public async Task<SwiftAuthV3Response> AuthenticateAsync(string authUrl, SwiftAuthV3Request reqObj)
        {
            var tokenUrl = $"{authUrl}/auth/tokens";
            var contentStr = JsonConvert.SerializeObject(reqObj, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
            var content = new StringContent(contentStr);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            try
            {
                var resp = await _Client.PostAsync(tokenUrl, content);
                if (resp.IsSuccessStatusCode)
                {
                    var respTxt = await resp.Content.ReadAsStringAsync();

                    var result = new SwiftAuthV3Response()
                    {
                        ContentLength = resp.Content?.Headers?.ContentLength ?? 0,
                        IsSuccess = resp.IsSuccessStatusCode,
                        Headers = resp.Headers.ToDictionary(),
                        Reason = resp.ReasonPhrase,
                        StatusCode = resp.StatusCode,
                        ContentObject = JsonConvert.DeserializeObject<SwiftAuthV3Response.TokenContainerObject>(respTxt),
                        ContentStr = respTxt
                    };
                    InitToken(result.Token, result.TokenExpires);
                    return result;
                }

                var length = resp.Content?.Headers?.ContentLength ?? 0;
                var respTxt2 = length > 0 ? await resp.Content.ReadAsStringAsync() : null;
                return new SwiftAuthV3Response()
                {
                    ContentLength = length,
                    IsSuccess = resp.IsSuccessStatusCode,
                    Headers = resp.Headers.ToDictionary(),
                    Reason = resp.ReasonPhrase,
                    StatusCode = resp.StatusCode,
                    ContentStr = respTxt2
                };
            }
            catch (Exception exc)
            {
                return new SwiftAuthV3Response()
                {
                    ContentLength = 0,
                    IsSuccess = false,
                    Headers = new Dictionary<string, string>(),
                    Reason = "Internal error " + exc.Message,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                };
            }
        }

        public void InitToken(string token, DateTime? expiresAt = null)
        {
            Token = token;
            TokenExpiresAt = expiresAt ?? DateTime.MaxValue;
        }

        // To test
        public Task<SwiftAuthV3CatalogResponse> GetServiceCatalog(string authUrl)
        {
            var catalogUrl = $"{authUrl}/auth/catalog";
            return GenericGetRequestAsync<SwiftAuthV3CatalogResponse, SwiftAuthV3CatalogResponse.CatalogObject>(catalogUrl);
        }

        #endregion Identity/Authentication

        #region Storage account

        /// <summary>
        /// GET: Show account details and list containers
        /// </summary>
        /// <param name="objectStoreUrl"></param>
        /// <param name="limit">For an integer value n , limits the number of results to n</param>
        /// <param name="marker">For a string value, x, constrains the list to items whose names are greater than x</param>
        /// <param name="endMarker">For a string value, x, constrains the list to items whose names are less than x</param>
        /// <param name="format">The response format. Valid values are json, xml, or plain.The default is plain.If you append the format = xml or format = json query parameter to the storage account URL, the response shows extended container information serialized in that format. If you append the format = plain query parameter, the response lists the container names separated by newlines</param>
        /// <param name="prefix">Only objects with this prefix will be returned. When combined with a delimiter query, this enables API users to simulate and traverse the objects in a container as if they were in a directory tree</param>
        /// <param name="delimiter">The delimiter is a single character used to split object names to present a pseudo-directory hierarchy of objects.When combined with a prefix query, this enables API users to simulate and traverse the objects in a container as if they were in a directory tree</param>
        /// <returns></returns>
        public Task<SwiftAccountDetailsResponse> AccountListContainersAsync(string objectStoreUrl, int? limit = null, string marker = null, string endMarker = null, string format = null, string prefix = null, string delimiter = null)
        {
            string url = GetStorageUrl(objectStoreUrl, true);

            var prms = new Dictionary<string, string>();
            if (limit.HasValue) prms.Add("limit", limit.Value.ToString());
            if (marker != null) prms.Add("marker", marker);
            if (endMarker != null) prms.Add("end_marker", endMarker);
            if (format != null) prms.Add("format", format);
            if (prefix != null) prms.Add("prefix", prefix);
            if (delimiter != null) prms.Add("delimiter", delimiter);

            UriBuilder ub = new UriBuilder(url);
            foreach (var qp in prms)
                ub.Query += "&" + qp.Key + "=" + qp.Value.UrlEncoded();

            url = ub.Uri.ToString();


            return GenericGetRequestAsync<SwiftAccountDetailsResponse, List<SwiftAccountDetailsResponse.ContainerObject>>(url);
        }

        /// <summary>
        /// HEAD Show account metadata
        /// </summary>
        /// <param name="objectStoreUrl"></param>
        /// <returns></returns>
        public Task<SwiftAccountDetailsResponse> AccountHeadAsync(string objectStoreUrl)
        {
            string url = GetStorageUrl(objectStoreUrl, true);
            return GenericHeadRequestNoContentAsync<SwiftAccountDetailsResponse>(url);
        }

        /// <summary>
        /// POST Creates, updates, or deletes account metadata.
        /// </summary>
        /// <param name="objectStoreUrl"></param>
        /// <param name="metaValues">Will add headers X-Account-Meta-{Key}:{Value}
        /// if Value is null, the meta is deleted, otherwise the meta is created or updated
        /// </param>
        /// <param name="additionalHeaders">Will add headers {Key}:{Value}</param>
        /// <returns></returns>
        public Task<SwiftBaseResponse> AccountPostAsync(string objectStoreUrl, Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null)
        {
            string url = GetStorageUrl(objectStoreUrl, true);

            var headers = new Dictionary<string, string>();
            if (metaValues != null)
                foreach (var kvp in metaValues)
                    headers.Add(SwiftHeaders.AccountMetaPrefix + kvp.Key, kvp.Value);
            if (additionalHeaders != null)
                foreach (var kvp in additionalHeaders)
                    headers.Add(kvp.Key, kvp.Value);

            return GenericPostRequestNoContentAsync<SwiftBaseResponse>(url, headers);
        }

        /// <summary>
        /// POST shortcut to set the temps keys
        /// </summary>
        /// <param name="objectStoreUrl"></param>
        /// <param name="key"></param>
        /// <param name="firstKey"></param>
        /// <returns></returns>
        public Task<SwiftBaseResponse> AccountSetTempUrlKeyAsync(string objectStoreUrl, string key, bool firstKey = true)
        {
            string url = GetStorageUrl(objectStoreUrl, true);

            var headers = new Dictionary<string, string>();
            headers.Add(firstKey ? SwiftHeaders.AccountMetaTempUrlKey : SwiftHeaders.AccountMetaTempUrlKey2, key);

            return AccountPostAsync(url, null, headers);
        }

        #endregion Storage account

        #region Container

        /// <summary>GET Show container details and list objects</summary>
        /// <returns>404 Not Found if the container does not exist</returns>
        /// <returns>204 No Content Success. The response body shows no objects. Either the container has no objects or you are paging through a long list of objects by using the marker, limit, or end_marker query parameter and you have reached the end of the list.</returns>
        /// <returns>200 Success The response body lists the objects.</returns>
        public Task<SwiftContainerInfoResponse> ContainerGetAsync(string objectStoreUrl, string container)
        {
            string url = GetContainerUrl(objectStoreUrl, container, true);

            return GenericGetRequestAsync<SwiftContainerInfoResponse, List<SwiftContainerInfoResponse.ContainerFileObject>>(url);
        }


        public Task<SwiftBaseResponse> ContainerPutAsync(string objectStoreUrl, string container, SwiftContainerPutParameters prms)
        {
            string url = GetContainerUrl(objectStoreUrl, container, true);

            var additionalHeaders = prms.GetHeaders();

            return GenericPutRequestAsync<SwiftBaseResponse>(url, additionalHeaders);
        }

        /// <summary>PUT Create container</summary>
        /// <param name="container">The unique (within an account) name for the container. The container name must be from 1 to 256 characters long and can start with any character and contain any pattern. Character set must be UTF-8. The container name cannot contain a slash (/) character</param>
        /// <returns>201, 202 success</returns>
        /// <returns>400, 404, 507 error</returns>
        public Task<SwiftBaseResponse> ContainerPutAsync(string objectStoreUrl, string container, Dictionary<string, string> metadata = null, Dictionary<string, string> additionalHeaders = null)
        {
            string url = GetContainerUrl(objectStoreUrl, container, true);

            additionalHeaders = additionalHeaders ?? new Dictionary<string, string>();
            if (metadata != null)
                foreach (var kvp in metadata)
                {
                    additionalHeaders.Add(SwiftHeaders.ContainerMetaPrefix + kvp.Key.UrlEncoded(), kvp.Value.UrlEncoded());
                }

            return GenericPutRequestAsync<SwiftBaseResponse>(url, additionalHeaders);
        }

        /// <summary>POST Create, update, or delete container metadata</summary>
        public Task<SwiftBaseResponse> ContainerPostAsync(string objectStoreUrl, string container, Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null)
        {
            string url = GetContainerUrl(objectStoreUrl, container, true);

            var headers = new Dictionary<string, string>();
            if (metaValues != null)
                foreach (var kvp in metaValues)
                    headers.Add(SwiftHeaders.ContainerMetaPrefix + kvp.Key, kvp.Value);
            if (additionalHeaders != null)
                foreach (var kvp in additionalHeaders)
                    headers.Add(kvp.Key, kvp.Value);

            return GenericPostRequestNoContentAsync<SwiftBaseResponse>(url, headers);
        }

        /// <summary>HEAD Show container metadata</summary>
        /// <returns>404 Not Found if the container does not exist</returns>
        /// <returns>204 No Content if the container exists</returns>
        public Task<SwiftContainerInfoResponse> ContainerHeadAsync(string objectStoreUrl, string container)
        {
            string url = GetContainerUrl(objectStoreUrl, container, true);

            return GenericHeadRequestNoContentAsync<SwiftContainerInfoResponse>(url);
        }

        /// <summary>DELETE Delete container</summary>
        /// <returns>404 Not Found if the container does not exist</returns>
        /// <returns>204 No Content if the container has been deleted</returns>
        /// <returns>409 Conflict if the container exists but is not empty</returns>
        public Task<SwiftBaseResponse> ContainerDeleteAsync(string objectStoreUrl, string container)
        {
            string url = GetContainerUrl(objectStoreUrl, container, true);

            return GenericDeleteRequestNoContentAsync<SwiftBaseResponse>(url);
        }

        #endregion Container info



        //GET /v1/{account}/{container}/{object}
        //Get object content and metadata

        //COPY /v1/{account}/{container}/{object}
        //Copy object

        //HEAD /v1/{account}/{container}/{object}
        //Show object metadata

        //POST /v1/{account}/{container}/{object}
        //Create or update object metadata

        // TODO: Put, Head, Get, Delete

        /// <summary>Show container details and list objects</summary>
        /// <returns>404 Not Found if the container does not exist</returns>
        /// <returns>416 Range not satisfiable</returns>
        /// <returns>200 Success The response body lists the objects.</returns>
        /// <remarks>Ranges will fail if there are more than any of these: Fifty ranges. Three overlapping ranges. Eight non-increasing ranges.</remarks>
        public Task<SwiftObjectGetResponse> ObjectGetAsync(string objectStoreUrl, string container, string objectName)
        {
            string url = GetObjectUrl(objectStoreUrl, container, objectName, false);

            return GenericGetRequestNoContentAsync<SwiftObjectGetResponse>(url);
        }


        /// <summary>
        /// Generates a temporary URL for an object
        /// 
        /// It requires to set the X-Account-Meta-Temp-URL-Key header to the storage account
        /// $ swift post -m "Temp-URL-Key:b3968d0207b54ece87cccc06515a89d4"
        /// </summary>
        /// <param name="objectStoreUrl"></param>
        /// <param name="container"></param>
        /// <param name="objectName"></param>
        /// <param name="expiresIn"></param>
        /// <param name="objectStoreKey"></param>
        /// <param name="ipRange">1.2.3.4 or 1.2.3.0/24 </param>
        /// <returns></returns>
        public string ObjectGetTmpUrlAsync(string objectStoreUrl, string container, string objectName, TimeSpan expiresIn, string objectStoreKey, string fileName = null, string ipRange = null, bool? noDownloadButInline = null)
        {
            // https://docs.openstack.org/kilo/config-reference/content/object-storage-tempurl.html

            string url = GetObjectUrl(objectStoreUrl, container, objectName, false);

            // GET HEAD PUT POST DELETE
            var method = "GET";
            var expires = ((int)(DateTime.UtcNow.Add(expiresIn) - new DateTime(1970, 1, 1)).TotalSeconds).ToString();// DateTime.UtcNow.Add(expiresIn).ToString("yyyy-MM-ddTHH:mm:ssK");
            var hmacBody = $"{method}\n{expires}\n{new Uri(url).PathAndQuery}";
            if (!ipRange.IsNullOrEmpty()) hmacBody = $"ip={ipRange}\n" + hmacBody;
            var sig = hmacBody.GenerateHMACSHA1SignatureHexDigest(objectStoreKey);

            var queryParams = new Dictionary<string, string>();
            queryParams.Add("temp_url_sig", sig);
            queryParams.Add("temp_url_expires", expires);
            if (!ipRange.IsNullOrEmpty())
                queryParams.Add("temp_url_ip_range", ipRange);
            if (!fileName.IsNullOrEmpty())
                queryParams.Add("filename", fileName);




            UriBuilder ub = new UriBuilder(url);
            foreach (var qp in queryParams)
                ub.Query += "&" + qp.Key + "=" + qp.Value.UrlEncoded();

            if (noDownloadButInline.HasValue && noDownloadButInline.Value)
                ub.Query += "&inline";

            return ub.Uri.ToString();

        }


        /// <summary>Create or replace object</summary>
        /// <returns></returns>
        /// PUT /v1/{account}/{container}/{object}
        public Task<SwiftBaseResponse> ObjectPutAsync(string objectStoreUrl, string container, string objectName, Stream data, string contentType = "application/octet-stream")
        {
            var url = GetObjectUrl(objectStoreUrl, container, objectName, true);
            var contentHeaders = new Dictionary<string, string>()
            {
                {SwiftHeaders.ContentType,contentType }
            };
            return GenericPutRequestAsync<SwiftBaseResponse>(url, data, additionalContentHeaders: contentHeaders);
        }

        /// <summary>Create or replace object</summary>
        /// PUT /v1/{account}/{container}/{object}
        public Task<SwiftBaseResponse> ObjectPutAsync(string objectStoreUrl, string container, string objectName, byte[] data, string contentType = "application/octet-stream")
        {
            var url = GetObjectUrl(objectStoreUrl, container, objectName, true);

            return GenericPutRequestAsync<SwiftBaseResponse>(url, data);
        }

        /// <summary>POST Create, update, or delete object metadata</summary>
        public Task<SwiftBaseResponse> ObjectPostAsync(string objectStoreUrl, string container, string objectName, Dictionary<string, string> metaValues = null, Dictionary<string, string> additionalHeaders = null)
        {
            string url = GetObjectUrl(objectStoreUrl, container, objectName, true);

            var headers = new Dictionary<string, string>();
            if (metaValues != null)
                foreach (var kvp in metaValues)
                    headers.Add(SwiftHeaders.ObjectMetaPrefix + kvp.Key, kvp.Value);
            if (additionalHeaders != null)
                foreach (var kvp in additionalHeaders)
                    headers.Add(kvp.Key, kvp.Value);

            return GenericPostRequestNoContentAsync<SwiftBaseResponse>(url, headers);
        }

        /// <summary>HEAD Show object metadata</summary>
        /// <returns>204 No Content if the object does not exist</returns>
        /// <returns>200 No Content if the object exists</returns>
        public Task<SwiftObjectGetResponse> ObjectHeadAsync(string objectStoreUrl, string container, string objectName)
        {
            string url = GetObjectUrl(objectStoreUrl, container, objectName, true);

            return GenericHeadRequestNoContentAsync<SwiftObjectGetResponse>(url);
        }

        /// <summary>Delete object</summary>
        /// DELETE /v1/{account}/{container}/{object}
        public Task<SwiftBaseResponse> ObjectDeleteAsync(string objectStoreUrl, string container, string objectName)
        {
            var url = GetObjectUrl(objectStoreUrl, container, objectName, true);

            return GenericDeleteRequestNoContentAsync<SwiftBaseResponse>(url);
        }


        private string GetStorageUrl(string objectStoreUrl, bool appendJson)
        {
            UriBuilder ub = new UriBuilder(objectStoreUrl);
            if (appendJson) ub.Query += "format=json";
            return ub.Uri.ToString();
        }

        private string GetContainerUrl(string objectStoreUrl, string container, bool appendJson)
        {
            UriBuilder ub = new UriBuilder(objectStoreUrl);
            if (ub.Path.EndsWith("/")) ub.Path += container.UrlEncoded();
            else ub.Path += "/" + container.UrlEncoded();
            if (appendJson) ub.Query += "format=json";
            return ub.Uri.ToString();
        }

        private string GetObjectUrl(string objectStoreUrl, string container, string obj, bool appendJson)
        {
            container = container.TrimStart('/');
            obj = obj.TrimStart('/');
            var baseUri = new Uri(objectStoreUrl);
            baseUri = new Uri(baseUri, container);
            baseUri = new Uri(baseUri, obj);
            UriBuilder ub = new UriBuilder(baseUri);
            //if (ub.Path.EndsWith("/")) ub.Path += container.UrlEncoded();
            //else ub.Path += "/" + container.UrlEncoded();
            //ub.Path += "/" + obj.UrlEncoded();
            if (appendJson) ub.Query += "format=json";
            
            return ub.Uri.ToString();
        }


        async Task<T> GenericDeleteRequestNoContentAsync<T>(string url, bool includeToken = true) where T : SwiftBaseResponse
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Delete, url);
            req.FillTokenHeader(Token);
            try
            {
                var resp = await _Client.SendAsync(req);
                T result = (T)Activator.CreateInstance(typeof(T), resp);
                return result;
            }
            catch (Exception exc)
            {
                T result = (T)Activator.CreateInstance(typeof(T));
                result.ContentLength = 0;
                result.IsSuccess = false;
                result.Headers = new Dictionary<string, string>();
                result.Reason = "Network error " + exc.Message;
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return result;
            }
        }

        async Task<T> GenericGetRequestAsync<T, U>(string url, bool includeToken = true) where T : SwiftBaseResponse<U>
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
            req.FillTokenHeader(Token);
            try
            {
                var resp = await _Client.SendAsync(req);
                T result = (T)Activator.CreateInstance(typeof(T), resp);
                await result.PrefillContentAsync(resp);
                return result;
            }
            catch (Exception exc)
            {
                T result = (T)Activator.CreateInstance(typeof(T));
                result.ContentLength = 0;
                result.IsSuccess = false;
                result.Headers = new Dictionary<string, string>();
                result.Reason = "Network error " + exc.Message;
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return result;
            }
        }
        async Task<T> GenericGetRequestNoContentAsync<T>(string url, bool includeToken = true) where T : SwiftBaseResponse
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
            req.FillTokenHeader(Token);
            try
            {
                var resp = await _Client.SendAsync(req);
                T result = (T)Activator.CreateInstance(typeof(T), resp);
                return result;
            }
            catch (Exception exc)
            {
                T result = (T)Activator.CreateInstance(typeof(T));
                result.ContentLength = 0;
                result.IsSuccess = false;
                result.Headers = new Dictionary<string, string>();
                result.Reason = "Network error " + exc.Message;
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return result;
            }
        }

        async Task<T> GenericHeadRequestNoContentAsync<T>(string url, bool includeToken = true) where T : SwiftBaseResponse
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Head, url);
            req.FillTokenHeader(Token);
            try
            {
                var resp = await _Client.SendAsync(req);
                T result = (T)Activator.CreateInstance(typeof(T), resp);
                return result;
            }
            catch (Exception exc)
            {
                T result = (T)Activator.CreateInstance(typeof(T));
                result.ContentLength = 0;
                result.IsSuccess = false;
                result.Headers = new Dictionary<string, string>();
                result.Reason = "Network error " + exc.Message;
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="additionalHeaders">Will be urlencoded</param>
        /// <param name="includeToken"></param>
        /// <returns></returns>
        async Task<T> GenericPostRequestNoContentAsync<T>(string url, Dictionary<string, string> additionalHeaders, bool includeToken = true) where T : SwiftBaseResponse
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
            req.FillTokenHeader(Token);
            foreach (var kvp in additionalHeaders)
                req.Headers.Add(kvp.Key, kvp.Value.UrlEncoded());
            try
            {
                var resp = await _Client.SendAsync(req);
                T result = (T)Activator.CreateInstance(typeof(T), resp);
                return result;
            }
            catch (Exception exc)
            {
                T result = (T)Activator.CreateInstance(typeof(T));
                result.ContentLength = 0;
                result.IsSuccess = false;
                result.Headers = new Dictionary<string, string>();
                result.Reason = "Network error " + exc.Message;
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return result;
            }
        }


        async Task<T> GenericPutRequestAsync<T>(string url, Dictionary<string, string> additionalHeaders = null, bool includeToken = true) where T : SwiftBaseResponse
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, url);
            req.FillTokenHeader(Token);
            try
            {
                if (additionalHeaders != null)
                    foreach (var h in additionalHeaders)
                        req.Headers.Add(h.Key, h.Value);

                var resp = await _Client.SendAsync(req);
                //// container not found
                //if (resp.StatusCode == HttpStatusCode.NotFound)
                //    return await EnsurePutContainer(containerId, () => PutObject(containerId, objectName, data, headers, queryParams)).ConfigureAwait(false);

                T result = (T)Activator.CreateInstance(typeof(T), resp);
                return result;
            }
            catch (Exception exc)
            {
                T result = (T)Activator.CreateInstance(typeof(T));
                result.ContentLength = 0;
                result.IsSuccess = false;
                result.Headers = new Dictionary<string, string>();
                result.Reason = "Network error " + exc.Message;
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return result;
            }
        }


        async Task<T> GenericPutRequestAsync<T>(string url, Stream content, Dictionary<string, string> additionalHeaders = null, Dictionary<string, string> additionalContentHeaders = null, bool includeToken = true) where T : SwiftBaseResponse
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, url);
            req.FillTokenHeader(Token);
            try
            {

                if (additionalHeaders != null)
                    foreach (var h in additionalHeaders)
                        req.Headers.Add(h.Key, h.Value);

                req.Content = new StreamContent(content);

                if (additionalContentHeaders != null)
                    foreach (var h in additionalContentHeaders)
                        req.Content.Headers.Add(h.Key, h.Value);

                var resp = await _Client.SendAsync(req);
                //// container not found
                //if (resp.StatusCode == HttpStatusCode.NotFound)
                //    return await EnsurePutContainer(containerId, () => PutObject(containerId, objectName, data, headers, queryParams)).ConfigureAwait(false);

                T result = (T)Activator.CreateInstance(typeof(T), resp);
                return result;
            }
            catch (Exception exc)
            {
                T result = (T)Activator.CreateInstance(typeof(T));
                result.ContentLength = 0;
                result.IsSuccess = false;
                result.Headers = new Dictionary<string, string>();
                result.Reason = "Network error " + exc.Message;
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return result;
            }
        }

        async Task<T> GenericPutRequestAsync<T>(string url, byte[] content, Dictionary<string, string> additionalHeaders = null, Dictionary<string, string> additionalContentHeaders = null, bool includeToken = true) where T : SwiftBaseResponse
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, url);
            req.FillTokenHeader(Token);
            try
            {
                if (additionalHeaders != null)
                    foreach (var h in additionalHeaders)
                        req.Headers.Add(h.Key, h.Value);

                req.Content = new ByteArrayContent(content);

                if (additionalContentHeaders != null)
                    foreach (var h in additionalContentHeaders)
                        req.Content.Headers.Add(h.Key, h.Value);

                var resp = await _Client.SendAsync(req);
                //// container not found
                //if (resp.StatusCode == HttpStatusCode.NotFound)
                //    return await EnsurePutContainer(containerId, () => PutObject(containerId, objectName, data, headers, queryParams)).ConfigureAwait(false);

                T result = (T)Activator.CreateInstance(typeof(T), resp);
                return result;
            }
            catch (Exception exc)
            {
                T result = (T)Activator.CreateInstance(typeof(T));
                result.ContentLength = 0;
                result.IsSuccess = false;
                result.Headers = new Dictionary<string, string>();
                result.Reason = "Network error " + exc.Message;
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return result;
            }
        }
    }
}
