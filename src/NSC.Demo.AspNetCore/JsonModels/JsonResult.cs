
using System.Collections.Generic;

namespace NetSwiftClient.Demo.AspNetCore
{
    public class JsonResult : JsonResult<object>//:IActionResult
    {
        public JsonResult() { }
        public JsonResult(bool success) : base(success) { }
    }
    public class JsonResult<T>//:IActionResult
    {
        public JsonResult() { }
        public JsonResult(bool success) { Success = true; }
        public bool Success { get; set; }
        public T Result { get; set; }
        public string Exception { get; set; }
        public string ErrorCode { get; set; }
        public string Error { get; set; }
        public List<string> ErrorCodes { get; set; }
        public List<string> Errors { get; set; }
        public string ErrorTarget { get; set; }
        public int StatusCode { get; set; } = 0;
    }
    public class StandardResult
    {
        public string RedirectLink { get; set; }
        public object ErrorDetails { get; set; }
    }
}
