// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace Tellma.Connector.MarminAe
{
    /// <summary>
    ///     The one HMAC-SHA256 recipe this connector uses, in both directions.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Outbound, the token request proves possession of the client secret by sending the
    ///         MAC of the client id keyed with the secret. Inbound, a webhook delivery is
    ///         authenticated by the MAC of the raw request body keyed with the endpoint signing
    ///         secret. Same primitive, same encoding, different key and message.
    ///     </para>
    ///     <para>
    ///         Everything here is UTF-8 in and Base64 out. Both halves are load-bearing: a hex
    ///         encoding, or an ASCII round trip that mangles a non-ASCII secret, produces a
    ///         signature that looks entirely plausible and that the vendor rejects.
    ///     </para>
    /// </remarks>
    internal static class MarminAeSignature
    {
        /// <summary>The length of an HMAC-SHA256 result.</summary>
        internal const int MacByteCount = 32;

        // Long enough for any credential an operator will paste, short enough to keep off the heap.
        private const int StackKeyByteLimit = 256;

        /// <summary>Computes the Base64 MAC of a text message.</summary>
        /// <param name="secret">The key.</param>
        /// <param name="message">The message.</param>
        /// <returns>The Base64-encoded MAC.</returns>
        internal static string Compute(string secret, string message)
        {
            Span<byte> mac = stackalloc byte[MacByteCount];
            int messageByteCount = Encoding.UTF8.GetByteCount(message);
            byte[]? rentedMessage = null;

            try
            {
                Span<byte> messageBuffer = messageByteCount <= StackKeyByteLimit
                    ? stackalloc byte[StackKeyByteLimit]
                    : (rentedMessage = ArrayPool<byte>.Shared.Rent(messageByteCount));

                int written = Encoding.UTF8.GetBytes(message, messageBuffer);
                ComputeInto(secret, messageBuffer[..written], mac);

                return Convert.ToBase64String(mac);
            }
            finally
            {
                if (rentedMessage is not null)
                {
                    ArrayPool<byte>.Shared.Return(rentedMessage, clearArray: true);
                }
            }
        }

        /// <summary>Computes the MAC of a byte message into a caller-owned buffer.</summary>
        /// <param name="secret">The key.</param>
        /// <param name="message">The message, byte for byte as it must be signed.</param>
        /// <param name="destination">A buffer of at least <see cref="MacByteCount" /> bytes.</param>
        internal static void ComputeInto(string secret, ReadOnlySpan<byte> message, Span<byte> destination)
        {
            int keyByteCount = Encoding.UTF8.GetByteCount(secret);
            byte[]? rentedKey = null;

            try
            {
                Span<byte> keyBuffer = keyByteCount <= StackKeyByteLimit
                    ? stackalloc byte[StackKeyByteLimit]
                    : (rentedKey = ArrayPool<byte>.Shared.Rent(keyByteCount));

                int written = Encoding.UTF8.GetBytes(secret, keyBuffer);
                try
                {
                    HMACSHA256.HashData(keyBuffer[..written], message, destination);
                }
                finally
                {
                    // The stack buffer held the secret in the clear; the pooled one is handed on to
                    // unrelated code next, beyond whatever that code happens to write into it.
                    CryptographicOperations.ZeroMemory(keyBuffer[..written]);
                }
            }
            finally
            {
                if (rentedKey is not null)
                {
                    ArrayPool<byte>.Shared.Return(rentedKey, clearArray: true);
                }
            }
        }
    }
}
