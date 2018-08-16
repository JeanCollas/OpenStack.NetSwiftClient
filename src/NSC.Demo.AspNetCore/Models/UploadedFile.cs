using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore
{
    public class UploadedFile
    {
        public Guid UploadId { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; }
    }
}
