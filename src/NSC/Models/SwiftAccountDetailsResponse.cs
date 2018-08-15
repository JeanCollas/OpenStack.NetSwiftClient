using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NetSwiftClient.Models
{
    public class SwiftAccountDetailsResponse : SwiftBaseResponse<List<SwiftAccountDetailsResponse.ContainerObject>>
    {
        public SwiftAccountDetailsResponse() { }
        public SwiftAccountDetailsResponse(HttpResponseMessage resp) : base(resp) { }

        [HasHeader(SwiftHeaders.AccountObjectCount)]
        public long ObjectCount { get; set; }

        [HasHeader(SwiftHeaders.AccountBytesUsed)]
        public long BytesUsed { get; set; }

        [HasHeader(SwiftHeaders.AcceptRanges)]
        public string AcceptRanges { get; set; }

        [HasHeader(SwiftHeaders.AccountMetaTempUrlKey)]
        public string TempKey { get; set; }

        [HasHeader(SwiftHeaders.AccountMetaTempUrlKey2)]
        public string TempKey2 { get; set; }


        public class ContainerObject
        {
            public int Count { get; set; }
            public long Bytes { get; set; }
            public string Name { get; set; }
            public DateTime Last_modified { get; set; }
        }
    }
}

// https://developer.openstack.org/api-ref/object-store/
