// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>Where and when what a document covers was handed over.</summary>
    /// <remarks>
    ///     The vendor requires this whole object for exports and for supplies made through
    ///     e-commerce, and requires the delivery address to be complete when it is present.
    /// </remarks>
    public sealed record MarminAeDelivery
    {
        /// <summary>Where the goods or services went.</summary>
        [JsonPropertyName("delivery_location")]
        public MarminAeDeliveryLocation? DeliveryLocation { get; init; }

        /// <summary>The day they got there.</summary>
        [JsonPropertyName("actual_delivery_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? ActualDeliveryDate { get; init; }

        /// <summary>The name of the party receiving delivery.</summary>
        [JsonPropertyName("party_name")]
        public string? PartyName { get; init; }

        /// <summary>The identifier of the party receiving delivery.</summary>
        [JsonPropertyName("party_id")]
        public string? PartyId { get; init; }

        /// <summary>The delivery terms, as an Incoterm.</summary>
        [JsonPropertyName("terms")]
        public string? Terms { get; init; }
    }

    /// <summary>A place goods or services were delivered to.</summary>
    public sealed record MarminAeDeliveryLocation
    {
        /// <summary>The address. Required by the vendor when the location is present.</summary>
        [JsonPropertyName("address")]
        public MarminAeAddress? Address { get; init; }

        /// <summary>An identifier for the location.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }
    }

    /// <summary>How a document is to be paid.</summary>
    /// <remarks>
    ///     The vendor requires at least one instruction on every document except a deemed supply,
    ///     and obliges a different detail object for each family of payment method: a card account
    ///     for card payments, a financial account for credit transfers, and a mandate for direct
    ///     debits.
    /// </remarks>
    public sealed record MarminAePaymentMeans
    {
        /// <summary>The payment method. Required by the vendor when an instruction is sent.</summary>
        [JsonPropertyName("payment_means_code")]
        public string? PaymentMeansCode { get; init; }

        /// <summary>The card the payment is taken from.</summary>
        [JsonPropertyName("card_account")]
        public MarminAeCardAccount? CardAccount { get; init; }

        /// <summary>The account the payment is credited to.</summary>
        [JsonPropertyName("payee_financial_account")]
        public MarminAePayeeFinancialAccount? PayeeFinancialAccount { get; init; }

        /// <summary>The mandate the payment is collected under.</summary>
        [JsonPropertyName("payment_mandate")]
        public MarminAePaymentMandate? PaymentMandate { get; init; }

        /// <summary>An identifier for the instruction.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The references payment is to be reconciled against.</summary>
        [JsonPropertyName("payment_id")]
        public IReadOnlyList<string>? PaymentId { get; init; }
    }

    /// <summary>A payment card.</summary>
    public sealed record MarminAeCardAccount
    {
        /// <summary>The card's primary account number.</summary>
        [JsonPropertyName("primary_account_number_id")]
        public string? PrimaryAccountNumberId { get; init; }

        /// <summary>The card network.</summary>
        [JsonPropertyName("network_id")]
        public string? NetworkId { get; init; }

        /// <summary>The name on the card.</summary>
        [JsonPropertyName("holder_name")]
        public string? HolderName { get; init; }
    }

    /// <summary>A bank account a payment is credited to.</summary>
    public sealed record MarminAePayeeFinancialAccount
    {
        /// <summary>The account identifier.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The account name.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>The branch holding the account.</summary>
        [JsonPropertyName("financial_institution_branch_id")]
        public string? FinancialInstitutionBranchId { get; init; }

        /// <summary>The branch address.</summary>
        [JsonPropertyName("address")]
        public MarminAeAddress? Address { get; init; }
    }

    /// <summary>The authority a direct debit is collected under.</summary>
    public sealed record MarminAePaymentMandate
    {
        /// <summary>The mandate reference.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The account the debit is taken from.</summary>
        [JsonPropertyName("payer_financial_account_id")]
        public string? PayerFinancialAccountId { get; init; }
    }

    /// <summary>Who is to be paid, when that is not the supplier.</summary>
    public sealed record MarminAePayeeParty
    {
        /// <summary>How the payee is identified.</summary>
        [JsonPropertyName("party_identification")]
        public MarminAePartyReference? PartyIdentification { get; init; }

        /// <summary>The payee's name.</summary>
        [JsonPropertyName("party_name")]
        public string? PartyName { get; init; }

        /// <summary>The account the payee is paid into.</summary>
        [JsonPropertyName("financial_account")]
        public MarminAePayeeFinancialAccount? FinancialAccount { get; init; }
    }

    /// <summary>Who represents the supplier for tax purposes.</summary>
    public sealed record MarminAeTaxRepresentativeParty
    {
        /// <summary>How the representative is identified.</summary>
        [JsonPropertyName("party_identification")]
        public MarminAePartyReference? PartyIdentification { get; init; }

        /// <summary>The representative's name.</summary>
        [JsonPropertyName("party_name")]
        public string? PartyName { get; init; }

        /// <summary>The representative's address.</summary>
        [JsonPropertyName("postal_address")]
        public MarminAeAddress? PostalAddress { get; init; }
    }
}
