using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore.Controllers
{
    public class UploadController:BaseController
    {
        private readonly SwiftClientService _SwiftClientService;
        private readonly CookieTokenService _TokenService;
        private readonly UploadService _UploadService;

        public UploadController(
                        SwiftClientService swiftClientService,
                        CookieTokenService tokenService,
                        UploadService uploadService,
                        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _SwiftClientService = swiftClientService;
            _TokenService = tokenService;
            _UploadService = uploadService;
        }

        public class FileUploadResult : StandardResult
        {
            //public Guid FileId { get; set; }
            public string FileName { get; set; }
            public int SizeKB { get; set; }
        }

        public static class ErrorCodes
        {
            public const string InternalError = SiteErrorCodes.InternalError;
            public const string NotAuthorized = SiteErrorCodes.NotAuthorized;

            public const string FileTooBig = SiteErrorCodes.FileTooBig;
            public const string ForbiddenContentType = SiteErrorCodes.ForbiddenContentType;
            public const string ForbiddenExtension = SiteErrorCodes.ForbiddenExtension;
            public const string EmptyContent = SiteErrorCodes.EmptyContent;
            public const string UnsupportedFormat = SiteErrorCodes.UnsupportedFormat;
            public const string SaveFailed = SiteErrorCodes.SaveFailed;

            public static (int StatusCode, string ErrorCode) FromUploadStatus(UploadService.UploadStatus status)
            {
                switch (status)
                {
                    case UploadService.UploadStatus.FileTooBig: return (StatusCodes.Status413PayloadTooLarge, ErrorCodes.FileTooBig);
                    case UploadService.UploadStatus.ForbiddenContentType: return (StatusCodes.Status415UnsupportedMediaType, ErrorCodes.ForbiddenContentType);
                    case UploadService.UploadStatus.ForbiddenExtension: return (StatusCodes.Status415UnsupportedMediaType, ErrorCodes.ForbiddenExtension);
                    case UploadService.UploadStatus.InternalError: return (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError);
                    case UploadService.UploadStatus.NoFileProvided: return (StatusCodes.Status400BadRequest, ErrorCodes.EmptyContent);
                    case UploadService.UploadStatus.UnsupportedFormat: return (StatusCodes.Status400BadRequest, ErrorCodes.UnsupportedFormat);
                    case UploadService.UploadStatus.SaveFailed: return (StatusCodes.Status500InternalServerError, ErrorCodes.SaveFailed);
                    default: return (StatusCodes.Status400BadRequest, ErrorCodes.InternalError);
                }
            }

        }


        [HttpPost("/UploadFile", Name = Routes.POST_UploadFile_Route)]
        public async Task<IActionResult> UploadFile( IFormFile file, string accountUrl, string container, string prefix = null)
        {
            var response = new JsonResult<FileUploadResult>() { Success = false, Result = new FileUploadResult() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized, () => Url.RouteUrl(Routes.GET_Home_Route))) 
                return Json(response);

            var (status, newFile) = await _UploadService.AddFile(file, accountUrl, container, prefix );

            if (status != UploadService.UploadStatus.Ok)
            {
                response.Success = false;
                (response.StatusCode, response.ErrorCode) = ErrorCodes.FromUploadStatus(status);
                return JsonResult(response);
            }

            response.Success = true;
            response.StatusCode = StatusCodes.Status200OK;
            //response.Result.FileId = newFile.Id;
            response.Result.FileName = newFile.FileName;
            response.Result.SizeKB = (int)(newFile.Size / 1024);
            return JsonResult(response);
        }

        [HttpPost("/UploadFiles", Name = Routes.POST_UploadFiles_Route)]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, string accountUrl , string container , string prefix=null)
        {
            var response = new JsonResult<List<FileUploadResult>>() { Success = false, Result = new List<FileUploadResult>() };

            if (!GenericCheck(() => _TokenService.HasToken, response, StatusCodes.Status401Unauthorized, SiteErrorCodes.NotAuthorized, () => Url.RouteUrl(Routes.GET_Home_Route)))
                return Json(response);

            if ((files?.Count ?? 0) == 0)
            {
                response.Success = false; response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = ErrorCodes.FromUploadStatus(UploadService.UploadStatus.NoFileProvided).ErrorCode;
                return JsonResult(response);
            }

            //var file = files.FirstOrDefault();

            var res = await _UploadService.AddFiles(files, accountUrl, container, prefix );

            if (res.All(s => s.status != UploadService.UploadStatus.Ok))
            {
                response.Success = false;
                var statuss = from r in res where r.status != UploadService.UploadStatus.Ok select ErrorCodes.FromUploadStatus(r.status);
                response.StatusCode = statuss.FirstOrDefault().StatusCode;
                response.ErrorCodes = statuss.Select(s => s.ErrorCode).ToList();
                return JsonResult(response);
            }

            response.Success = true;
            response.StatusCode = StatusCodes.Status200OK;
            foreach (var f in res.Where(r => r.file != null).Select(r => r.file))
            {
                response.Result.Add(new FileUploadResult()
                {
                    //FileId = f.Id,
                    FileName = f.FileName,
                    SizeKB = (int)(f.Size / 1024)
                });
            }
            return JsonResult(response);
        }


    }
}
