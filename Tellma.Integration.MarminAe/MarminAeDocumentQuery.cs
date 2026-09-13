// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

namespace Tellma.Connector.MarminAe
{
    /// <summary>The filters and paging a document listing accepts.</summary>
    /// <remarks>
    ///     Every member is optional; an instance with nothing set produces a request with no query
    ///     string at all, which the vendor answers with its own default page.
    /// </remarks>
    public sealed record MarminAeDocumentQuery
    {
        /// <summary>Matches the document number as displayed on the document.</summary>
        public string? DocumentNumber { get; init; }

        /// <summary>Matches the supplier's name.</summary>
        public string? BilledByName { get; init; }

        /// <summary>Matches the customer's name.</summary>
        public string? BilledToName { get; init; }

        /// <summary>Matches the customer's VAT registration number.</summary>
        public string? BilledToVat { get; init; }

        /// <summary>Matches the supplier's VAT registration number.</summary>
        public string? BilledByVat { get; init; }

        /// <summary>Matches the vendor's own document status.</summary>
        /// <remarks>The vendor does not publish this vocabulary, so it stays a free string.</remarks>
        public string? Status { get; init; }

        /// <summary>Matches the issue date.</summary>
        public DateOnly? DocumentDate { get; init; }

        /// <summary>The zero-indexed page to return.</summary>
        public int? Page { get; init; }

        /// <summary>How many documents the page holds.</summary>
        public int? Size { get; init; }
    }
}
