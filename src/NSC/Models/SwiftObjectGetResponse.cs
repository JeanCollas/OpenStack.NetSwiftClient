using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace NetSwiftClient.Models
{
    public class SwiftObjectGetResponse : SwiftBaseResponse, ISwiftResponseWithStream
    {
        public SwiftObjectGetResponse() { }
        public SwiftObjectGetResponse(HttpResponseMessage resp) : base(resp) { }

        public Stream ObjectStreamContent { get; set; }

        [HasHeader(SwiftHeaders.ObjectMetaName)]
        public string ObjectName { get; set; }

        [HasHeader(SwiftHeaders.AcceptRanges)]
        public string AcceptRanges { get; set; }

        [HasHeader(SwiftHeaders.ContentDisposition)]
        public string ContentDisposition { get; set; }

        [HasHeader(SwiftHeaders.DeleteAt)]
        public int? DeleteAtEpochTimeStamp { get; set; }
        public DateTime? DeleteAt => (!DeleteAtEpochTimeStamp.HasValue ? (DateTime?)null : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds(DeleteAtEpochTimeStamp.Value));

        [HasHeader(SwiftHeaders.ObjectManifest)]
        public string ObjectManifest { get; set; }

        [HasHeader(SwiftHeaders.LastModified)]
        public string LastModified { get; set; }

        [HasHeader(SwiftHeaders.ETag)]
        public string ETag { get; set; }

        [HasHeader(SwiftHeaders.XTimestamp)]
        public decimal? CreationTimeEpoch { get; set; }
        public DateTime? CreationTime => (!CreationTimeEpoch.HasValue ? (DateTime?)null : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds((int)CreationTimeEpoch.Value));

        [HasHeader(SwiftHeaders.ObjectStaticLargeObject)]
        public bool LargeObject { get; set; }

        [HasHeader(SwiftHeaders.SymlinkTarget)]
        public string SymlinkTarget { get; set; }

        [HasHeader(SwiftHeaders.SymlinkTargetAccount)]
        public string SymlinkTargetAccount { get; set; }


        #region Dispose
        bool _Disposed = false;
         
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
                return;

            if (disposing)
            {
                if (ObjectStreamContent != null) ObjectStreamContent.Dispose();
            }

            _Disposed = true;
        }
        #endregion
    }
}

// https://developer.openstack.org/api-ref/object-store/
