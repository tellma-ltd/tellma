// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

namespace Tellma.Connector.MarminAe
{
    /// <summary>A file the vendor served, still streaming.</summary>
    /// <remarks>
    ///     <para>
    ///         The bytes are not buffered: a legal artifact runs to megabytes, and the caller is
    ///         usually about to write it somewhere anyway. That means the underlying response is
    ///         still open, so <strong>the caller must dispose this</strong> — disposing it disposes
    ///         the response and with it the stream.
    ///     </para>
    ///     <para>
    ///         The client's own timeout covers reaching the response headers, not reading the body.
    ///         Read time belongs to whoever is reading.
    ///     </para>
    /// </remarks>
    public sealed class MarminAeDownload : IDisposable
    {
        private readonly HttpResponseMessage _response;
        private bool _disposed;

        /// <summary>Takes ownership of a response whose body the caller will read.</summary>
        /// <param name="response">The response, which this disposes.</param>
        /// <param name="content">Its body.</param>
        /// <param name="contentType">The media type the vendor served it as.</param>
        /// <param name="fileName">The name the vendor suggested, if any.</param>
        /// <param name="contentLength">How long the vendor said it is, if it said.</param>
        /// <param name="rateLimit">What the response said about the remaining quota.</param>
        internal MarminAeDownload(
            HttpResponseMessage response,
            Stream content,
            string? contentType,
            string? fileName,
            long? contentLength,
            MarminAeRateLimit rateLimit)
        {
            _response = response;
            Content = content;
            ContentType = contentType;
            FileName = fileName;
            ContentLength = contentLength;
            RateLimit = rateLimit;
        }

        /// <summary>The file's bytes.</summary>
        public Stream Content { get; }

        /// <summary>The media type the vendor served it as.</summary>
        public string? ContentType { get; }

        /// <summary>
        ///     The name the vendor suggested for it, from the dedicated header if there was one and
        ///     from the disposition header otherwise.
        /// </summary>
        public string? FileName { get; }

        /// <summary>How long the file is, when the vendor said.</summary>
        public long? ContentLength { get; }

        /// <summary>What the vendor said about the remaining quota.</summary>
        public MarminAeRateLimit RateLimit { get; }

        /// <summary>Closes the stream and the response behind it.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _response.Dispose();
        }
    }
}
