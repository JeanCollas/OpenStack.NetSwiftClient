using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NetSwiftClient.Models
{
    public class SwiftContainerInfoResponse : SwiftBaseResponse<List<SwiftContainerInfoResponse.ContainerFileObject>>
    {
        public SwiftContainerInfoResponse() { }
        public SwiftContainerInfoResponse(HttpResponseMessage resp) : base(resp) { }
        //public string ContentStr { get; set; }
        //public List<ContainerObject> ContentObject { get; set; }

        [HasHeader(SwiftHeaders.ContainerMetaName)]
        public string ContainerName { get; set; }

        [HasHeader(SwiftHeaders.ContainerObjectCount)]
        public long ObjectCount { get; set; }

        [HasHeader(SwiftHeaders.ContainerBytesUsed)]
        public long BytesUsed { get; set; }

        [HasHeader(SwiftHeaders.ContainerAcceptRanges)]
        public string AcceptRanges { get; set; }
        
        public class ContainerFileObject
        {
            public string Hash { get; set; }
            public DateTime Last_modified { get; set; }
            public long Bytes { get; set; }
            public long KBytes => Bytes / 1024;
            public long MBytes => Bytes / 1024 / 1024;
            public string Name { get; set; }
            public string Content_type { get; set; }
        }
    }
}

// https://developer.openstack.org/api-ref/object-store/
