using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore
{
    public class UploadService
    {
        private readonly SwiftClientService _SwiftService;
        private readonly CookieTokenService _TokenService;
        private readonly FileExtensionContentTypeProvider _FileExtensionContentTypeProvider;

        public UploadService(
            SwiftClientService swiftService,
            CookieTokenService tokenService,
            FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _SwiftService = swiftService;
            _TokenService = tokenService;
            _FileExtensionContentTypeProvider = fileExtensionContentTypeProvider;
        }

        public UploadOptions DefaultOptions { get; } = new UploadOptions();
        public UploadOptions NewOptions => new UploadOptions();
        public class UploadOptions
        {
            public int SizeLimitKb = 8000;
            public int FileCountLimitPerDay = 100;
            public List<string> AllowedExtensions { get; } = new List<string>() { "*" };
            public List<string> AllowedContentTypes { get; } = new List<string>() { "*" };
            public bool IsExtensionAllowed(string ext) { return AllowedExtensions.Contains("*") || AllowedExtensions.Contains(ext); }
            public bool IsContentTypeAllowed(string ctype) { return AllowedContentTypes.Contains("*") || AllowedContentTypes.Contains(ctype); }
        }




        public async Task<List<(UploadStatus status, UploadedFile file)>> AddFiles(List<IFormFile> files, string accountUrl, string container, string prefix = null, UploadOptions options = null)
        {
            List<(UploadStatus status, UploadedFile file)> list = new List<(UploadStatus status, UploadedFile file)>();
            foreach (var f in files)
                list.Add(await AddFile(f, accountUrl, container, prefix, options: options));
            return list;
        }

        /// <summary>
        ///  Considers that the user is logged in
        /// </summary>
        public async Task<(UploadStatus status, UploadedFile file)>
            AddFile(IFormFile file, string accountUrl, string container, string prefix = null, string fileName = null, string fileExtension = null, string contentType = null, UploadOptions options = null)
        {
            options = options ?? DefaultOptions;

            if (file == null || file.Length == 0)
                return (UploadStatus.NoFileProvided, null);

            if (file.Length / 1024 > options.SizeLimitKb)
                return (UploadStatus.FileTooBig, null);

            _SwiftService.InitToken(_TokenService.Token.Token);

            fileName = fileName ?? file.FileName;
            var ext = System.IO.Path.GetExtension(fileName);
            fileExtension = fileExtension ?? ext;

            if (_FileExtensionContentTypeProvider.TryGetContentType(fileName, out string contentType2))
                contentType = contentType ?? contentType2;
            else contentType = contentType ?? "unknown";


            UploadedFile uf = new UploadedFile()
            {
                ContentType = contentType,
                Extension = fileExtension,
                FileName = fileName,
                UploadId = Guid.NewGuid(),
                Size = file.Length,
                //Data =,
            };

            byte[] data = null;
            using (var fs1 = file.OpenReadStream())
            using (var ms1 = new MemoryStream())
            {
                fs1.CopyTo(ms1);
                data = ms1.ToArray();
                var resp = await _SwiftService.ObjectPutAsync(accountUrl, container, $"{prefix}{uf.FileName}", data, contentType);
                //var resp=await _SwiftService.ObjectPutAsync(accountUrl, container, $"{prefix}{uf.FileName}", fs1, contentType);
            }

            return (UploadStatus.Ok, uf);
        }

        public enum UploadStatus
        {
            NoFileProvided = 0,
            Ok = 1,
            TooManyUploads = 2,
            InternalError = 3,
            ForbiddenExtension = 4,
            ForbiddenContentType = 5,
            FileTooBig = 6,
            UnsupportedFormat = 7,
            SaveFailed = 8,
        }

    }
}
