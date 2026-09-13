// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Security.Cryptography;

namespace Tellma.Connector.MarminAe
{
    /// <summary>Checks that a webhook delivery really came from the vendor.</summary>
    /// <remarks>
    ///     <para>
    ///         The scheme is an HMAC-SHA256 of the raw request body, keyed with the endpoint's
    ///         signing secret and Base64-encoded into a header. <strong>Raw</strong> is the word
    ///         that matters: the body has to be verified byte for byte as it arrived, before any
    ///         parsing, because re-serializing a parsed document changes insignificant whitespace
    ///         and with it the signature.
    ///     </para>
    ///     <para>
    ///         More than one secret is accepted, which is the rotation affordance: regenerating the
    ///         secret in the vendor's portal replaces it at a keystroke, and accepting the old
    ///         alongside the new across a deploy is what keeps rotation from dropping deliveries.
    ///     </para>
    ///     <para>
    ///         There is deliberately no freshness check on the delivery. The vendor redelivers
    ///         failed deliveries carrying their original timestamps, a legitimate staleness no
    ///         tolerance window can tell apart from a replay; replay defence is deduplication on
    ///         the delivery id, at the tier that handles the event.
    ///     </para>
    /// </remarks>
    public static class MarminAeWebhookVerifier
    {
        /// <summary>The header a delivery is signed in.</summary>
        public const string SignatureHeaderName = "x-marmin-signature";

        /// <summary>Checks a delivery against every secret the endpoint accepts.</summary>
        /// <param name="body">The request body, byte for byte as it arrived.</param>
        /// <param name="signature">The value of the signature header.</param>
        /// <param name="secrets">The signing secrets currently accepted.</param>
        /// <returns>True when the signature matches the body under any of them.</returns>
        public static bool Verify(ReadOnlySpan<byte> body, string? signature, IReadOnlyList<string> secrets)
        {
            ArgumentNullException.ThrowIfNull(secrets);

            if (string.IsNullOrEmpty(signature) || secrets.Count == 0)
            {
                return false;
            }

            Span<byte> received = stackalloc byte[MarminAeSignature.MacByteCount];
            if (!TryDecode(signature, received, out int decoded)
                || decoded != MarminAeSignature.MacByteCount)
            {
                // Anything that is not exactly one Base64-encoded HMAC-SHA256 — a hex digest, a
                // truncated paste, an empty header — cannot match, and rejecting it here keeps the
                // comparison below over two fixed-length values.
                return false;
            }

            Span<byte> computed = stackalloc byte[MarminAeSignature.MacByteCount];
            bool accepted = false;

            for (int index = 0; index < secrets.Count; index++)
            {
                string secret = secrets[index];
                if (string.IsNullOrEmpty(secret))
                {
                    continue;
                }

                MarminAeSignature.ComputeInto(secret, body, computed);

                // Accumulated rather than returned early, so how long this takes does not depend on
                // which secret matched or on whether one did. The comparison itself is the
                // framework's fixed-time one; only the length of the received value, which is
                // public, can be inferred from anything here.
                accepted |= CryptographicOperations.FixedTimeEquals(computed, received);
            }

            return accepted;
        }

        /// <summary>Decodes the signature header, padded or not.</summary>
        /// <remarks>
        ///     Base64 without its trailing padding is still Base64, and a proxy or a portal that
        ///     strips it would otherwise produce a rejection indistinguishable from a wrong secret.
        /// </remarks>
        private static bool TryDecode(string signature, Span<byte> destination, out int decoded)
        {
            if (Convert.TryFromBase64String(signature, destination, out decoded))
            {
                return true;
            }

            int padding = signature.Length % 4;

            return padding is not (0 or 1)
                && Convert.TryFromBase64String(
                    signature + new string('=', 4 - padding), destination, out decoded);
        }
    }
}
