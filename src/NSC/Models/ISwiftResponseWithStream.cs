using System;
using System.IO;

namespace NetSwiftClient.Models
{
    internal interface ISwiftResponseWithStream: IDisposable
    {
        Stream ObjectStreamContent { get; set; }
    }
}