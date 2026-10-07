// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>
    ///     The source-generated serializer for every payload this client reads or writes: no
    ///     reflection at runtime, and a stable property order the payload snapshots are written
    ///     against.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Omitting nulls is load-bearing rather than cosmetic. It is what keeps an unset
    ///         optional field off the wire entirely, which matters most on a credit note, where the
    ///         vendor's field list and its own examples disagree about three fields: a caller who
    ///         follows the list sends exactly the list.
    ///     </para>
    ///     <para>
    ///         There is no naming policy, and every member spells its wire name out. The vendor is
    ///         not consistent — the transmission snapshot is camel case where everything else is
    ///         snake case, and it spells one field in a way no camel-case policy produces — so a
    ///         policy would be right in most places and silently wrong in the rest.
    ///     </para>
    ///     <para>
    ///         Reading numbers from strings, and matching names case-insensitively, are read-side
    ///         tolerances only: they cost nothing on the way out and are the difference between a
    ///         readable document and a failed one on the way in.
    ///     </para>
    /// </remarks>
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(MarminAeSalesInvoiceRequest))]
    [JsonSerializable(typeof(MarminAeSalesCreditNoteRequest))]
    [JsonSerializable(typeof(MarminAeDocument))]
    [JsonSerializable(typeof(MarminAePage<MarminAeDocument>))]
    [JsonSerializable(typeof(MarminAePage<string>))]
    [JsonSerializable(typeof(MarminAePeppolStatusSnapshot))]
    [JsonSerializable(typeof(List<MarminAePeppolStatusLogEntry>))]
    [JsonSerializable(typeof(MarminAeBusinessProfile))]
    internal sealed partial class MarminAeJsonContext : JsonSerializerContext;
}
