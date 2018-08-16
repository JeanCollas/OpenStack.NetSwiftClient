using System;
using System.Collections.Generic;
using System.Text;

namespace NetSwiftClient.Models
{
    public class SwiftAuthV3CatalogResponse : SwiftBaseResponse<SwiftAuthV3CatalogResponse.CatalogObject>
    {
        public class CatalogObject
        {
//            public List<LinkItem> Links { get; set; }
            public List<SwiftAuthV3Response.CatalogItem> Catalog { get; set; }
        }
    }
}
