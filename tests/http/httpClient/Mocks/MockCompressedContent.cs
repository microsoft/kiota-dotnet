using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
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
            await SerializeToStreamAsync(stream, CancellationToken.None).ConfigureAwait(false);
        }

#if NET5_0_OR_GREATER
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            await SerializeToStreamAsync(stream, cancellationToken).ConfigureAwait(false);
        }
#endif

        private async Task SerializeToStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            Stream compressedStream = new GZipStream(stream, CompressionMode.Compress, true);
#if NET5_0_OR_GREATER
            await _originalContent.CopyToAsync(compressedStream, cancellationToken).ConfigureAwait(false);
#else
            await _originalContent.CopyToAsync(compressedStream).ConfigureAwait(false);
#endif
            compressedStream.Dispose();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
