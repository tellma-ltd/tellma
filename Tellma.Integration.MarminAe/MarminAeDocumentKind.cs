// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

namespace Tellma.Connector.MarminAe
{
    /// <summary>
    ///     One of the four document families the vendor exposes a uniform read surface over.
    /// </summary>
    /// <remarks>
    ///     The families differ in what may be written — only the two sales families accept
    ///     submissions from this client — but not in how they are read, which is why every read
    ///     operation takes this rather than being repeated four times.
    /// </remarks>
    public enum MarminAeDocumentKind
    {
        /// <summary>A sales invoice, issued by the configured business profile to a customer.</summary>
        SalesInvoice = 0,

        /// <summary>A sales credit note, adjusting a sales invoice already issued.</summary>
        SalesCreditNote = 1,

        /// <summary>A purchase invoice, self-billed or received over the network.</summary>
        PurchaseInvoice = 2,

        /// <summary>A purchase credit note, adjusting a purchase invoice.</summary>
        PurchaseCreditNote = 3,
    }
}
