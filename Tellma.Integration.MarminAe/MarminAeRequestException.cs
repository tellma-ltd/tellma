// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>A request the vendor refused, or one this client refused to send.</summary>
    /// <remarks>
    ///     <para>
    ///         Transmission outcomes are not failures and never arrive this way: a document can be
    ///         accepted here and rejected by validation minutes later, and that shows up as data on
    ///         the document, not as an exception.
    ///     </para>
    ///     <para>
    ///         A throttling refusal is not retried anywhere in this client. It carries the vendor's
    ///         own quota reading on <see cref="RateLimit" /> so the caller can pace itself, because
    ///         how long to wait, and whether to wait at all, is the caller's decision.
    ///     </para>
    /// </remarks>
    public sealed class MarminAeRequestException : Exception
    {
        /// <summary>Creates an exception describing a refusal.</summary>
        /// <param name="statusCode">The status the vendor answered with, or zero when the client
        ///     refused to send the request at all.</param>
        /// <param name="detail">Whatever could be read out of the refusal.</param>
        /// <param name="rateLimit">What the vendor said about the remaining quota.</param>
        public MarminAeRequestException(
            int statusCode, MarminAeErrorDetail? detail, MarminAeRateLimit rateLimit)
            : base(Describe(statusCode, detail))
        {
            StatusCode = statusCode;
            Detail = detail;
            RateLimit = rateLimit;
        }

        /// <summary>Creates an exception with a message.</summary>
        /// <param name="message">The message.</param>
        public MarminAeRequestException(string message)
            : base(message)
        {
            RateLimit = MarminAeRateLimit.None;
        }

        /// <summary>Creates an exception with a message and a cause.</summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The cause.</param>
        public MarminAeRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
            RateLimit = MarminAeRateLimit.None;
        }

        /// <summary>Creates an exception with no detail.</summary>
        public MarminAeRequestException()
        {
            RateLimit = MarminAeRateLimit.None;
        }

        /// <summary>
        ///     The status the vendor answered with, or zero when this client refused to send the
        ///     request at all.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>Whatever could be read out of the refusal.</summary>
        public MarminAeErrorDetail? Detail { get; }

        /// <summary>What the vendor said about the remaining quota.</summary>
        public MarminAeRateLimit RateLimit { get; }

        /// <summary>Whether the refusal was a throttling one.</summary>
        public bool IsRateLimited => StatusCode == 429;

        /// <summary>Turns an unsuccessful response into the exception describing it.</summary>
        /// <param name="response">The response.</param>
        /// <param name="body">Its body, already read.</param>
        /// <param name="rateLimit">What its headers said about the remaining quota.</param>
        /// <exception cref="MarminAeRequestException">The response was not a success.</exception>
        internal static void ThrowIfRefused(
            HttpResponseMessage response, string body, MarminAeRateLimit rateLimit)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new MarminAeRequestException(
                    (int)response.StatusCode, MarminAeErrorParser.Parse(body), rateLimit);
            }
        }

        private static string Describe(int statusCode, MarminAeErrorDetail? detail)
        {
            string? described = detail?.Describe();

            return string.IsNullOrWhiteSpace(described)
                ? string.Create(
                    CultureInfo.InvariantCulture,
                    $"The Marmin UAE API refused the request with status {statusCode}.")
                : string.Create(
                    CultureInfo.InvariantCulture,
                    $"The Marmin UAE API refused the request with status {statusCode}. {described}");
        }
    }
}
