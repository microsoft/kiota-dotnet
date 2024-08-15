using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks
{
    public class MockCompressedContent : HttpContent
    {
        private readonly HttpContent _originalContent;

        public MockCompressedContent(HttpContent httpContent)
        {
            _originalContent = httpContent;
            foreach(var header in _originalContent.Headers)
                Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            Stream compressedStream = new GZipStream(stream, CompressionMode.Compress, true);
            await _originalContent.CopyToAsync(compressedStream);
            compressedStream.Dispose();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
