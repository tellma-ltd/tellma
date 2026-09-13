// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

namespace Tellma.Connector.MarminAe
{
    /// <summary>Everything that could be read out of a refusal.</summary>
    /// <remarks>
    ///     The vendor publishes no schema for its error bodies, so the parser recognises the shapes
    ///     it has seen and keeps the body verbatim either way. A body nothing could be read out of
    ///     is exactly the one somebody will need to look at.
    /// </remarks>
    /// <param name="Message">The refusal in one sentence, when the body carried one.</param>
    /// <param name="Errors">What the vendor objected to, field by field, when it said.</param>
    /// <param name="RawBody">The body as it arrived, truncated if it was long.</param>
    /// <param name="IsRawBodyTruncated">Whether <paramref name="RawBody" /> was cut short.</param>
    public sealed record MarminAeErrorDetail(
        string? Message,
        IReadOnlyList<MarminAeError> Errors,
        string? RawBody,
        bool IsRawBodyTruncated)
    {
        /// <summary>The refusal as a single line, for a log or an exception message.</summary>
        /// <returns>
        ///     The field-level objections when there were any, the message when there was one, and
        ///     the raw body when there was neither.
        /// </returns>
        public string? Describe()
        {
            if (Errors.Count == 0)
            {
                return string.IsNullOrWhiteSpace(Message) ? RawBody : Message;
            }

            string described = string.Join(
                "; ",
                Errors.Select(static error => string.IsNullOrEmpty(error.Field)
                    ? error.Message
                    : string.IsNullOrEmpty(error.Message)
                        ? error.Field
                        : $"{error.Field}: {error.Message}"));

            // A refusal can carry both a summary and the fields it objected to, and the summary is
            // the half a person reads first. Keeping only the fields threw it away.
            return string.IsNullOrWhiteSpace(Message) ? described : $"{Message}: {described}";
        }
    }

    /// <summary>One thing the vendor objected to.</summary>
    /// <param name="Message">What was wrong.</param>
    /// <param name="Field">Where, as a path into the submitted payload, when the vendor said.</param>
    /// <param name="Code">The vendor's own code for the objection, when it gave one.</param>
    public sealed record MarminAeError(string? Message, string? Field = null, string? Code = null);
}
