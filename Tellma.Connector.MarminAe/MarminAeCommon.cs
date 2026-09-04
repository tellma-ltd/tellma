// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>A postal or delivery address.</summary>
    /// <remarks>
    ///     <para>
    ///         Shared between requests and responses: the vendor echoes the same field list back.
    ///     </para>
    ///     <para>
    ///         The vendor requires <see cref="StreetName" />, <see cref="CityName" />,
    ///         <see cref="CountrySubentity" />, <see cref="Country" /> and
    ///         <see cref="CountryCode" /> on every address it is sent, and rejects an incomplete one
    ///         with a field-level error. None of them is declared required here, because the same
    ///         type reads responses, and a type that refuses to deserialize a field the vendor
    ///         happened to omit would turn one missing value into a failed read of the whole
    ///         document.
    ///     </para>
    /// </remarks>
    public sealed record MarminAeAddress
    {
        /// <summary>The street name or first address line. Required by the vendor.</summary>
        [JsonPropertyName("street_name")]
        public string? StreetName { get; init; }

        /// <summary>The second address line.</summary>
        [JsonPropertyName("additional_street_name")]
        public string? AdditionalStreetName { get; init; }

        /// <summary>The city or town. Required by the vendor.</summary>
        [JsonPropertyName("city_name")]
        public string? CityName { get; init; }

        /// <summary>The postal or ZIP code.</summary>
        [JsonPropertyName("postal_zone")]
        public string? PostalZone { get; init; }

        /// <summary>
        ///     The country subdivision. Required by the vendor, and for a UAE address it must be an
        ///     emirate code rather than a name.
        /// </summary>
        [JsonPropertyName("country_subentity")]
        public string? CountrySubentity { get; init; }

        /// <summary>A further unstructured address line.</summary>
        [JsonPropertyName("address_line")]
        public string? AddressLine { get; init; }

        /// <summary>The country name. Required by the vendor.</summary>
        [JsonPropertyName("country")]
        public string? Country { get; init; }

        /// <summary>The ISO 3166-1 alpha-2 country code. Required by the vendor.</summary>
        [JsonPropertyName("country_code")]
        public string? CountryCode { get; init; }
    }

    /// <summary>A party's VAT registration.</summary>
    /// <remarks>
    ///     The vendor treats the pair as all-or-nothing: supplying either member obliges the other.
    /// </remarks>
    public sealed record MarminAePartyTaxScheme
    {
        /// <summary>The UAE VAT registration number.</summary>
        [JsonPropertyName("company_id")]
        public string? CompanyId { get; init; }

        /// <summary>The tax scheme, which for the UAE is VAT.</summary>
        [JsonPropertyName("tax_scheme")]
        public string? TaxScheme { get; init; }
    }

    /// <summary>The VAT treatment of a line, a charge, or an allowance.</summary>
    /// <remarks>
    ///     <see cref="Id" /> is required by the vendor wherever this object appears, and an exempt
    ///     category additionally obliges both exemption fields. Choosing among the categories is
    ///     tax knowledge this client deliberately does not carry.
    /// </remarks>
    public sealed record MarminAeTaxCategory
    {
        /// <summary>The tax category code. Required by the vendor.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The exemption reason code, required when the category is exempt.</summary>
        [JsonPropertyName("tax_exemption_reason_code")]
        public string? TaxExemptionReasonCode { get; init; }

        /// <summary>The exemption reason text, required when the category is exempt.</summary>
        [JsonPropertyName("tax_exemption_reason")]
        public string? TaxExemptionReason { get; init; }

        /// <summary>The VAT rate as a percentage.</summary>
        [JsonPropertyName("percent")]
        public decimal? Percent { get; init; }

        /// <summary>The tax scheme, which for the UAE is VAT.</summary>
        [JsonPropertyName("tax_scheme")]
        public string? TaxScheme { get; init; }
    }

    /// <summary>An amount added to a document or to one of its lines.</summary>
    /// <remarks>
    ///     The vendor obliges at least one of <see cref="Reason" /> and
    ///     <see cref="ReasonCode" />, a <see cref="TaxCategory" /> at document level, and the pair
    ///     <see cref="MultiplierFactorNumeric" /> and <see cref="BaseAmount" /> together when
    ///     either is present.
    /// </remarks>
    public sealed record MarminAeCharge
    {
        /// <summary>The free-text reason for the charge.</summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; init; }

        /// <summary>The coded reason for the charge.</summary>
        [JsonPropertyName("reason_code")]
        public string? ReasonCode { get; init; }

        /// <summary>The VAT treatment of the charge.</summary>
        [JsonPropertyName("tax_category")]
        public MarminAeTaxCategory? TaxCategory { get; init; }

        /// <summary>The charge amount.</summary>
        [JsonPropertyName("amount")]
        public decimal? Amount { get; init; }

        /// <summary>The percentage the charge is computed at.</summary>
        [JsonPropertyName("multiplier_factor_numeric")]
        public decimal? MultiplierFactorNumeric { get; init; }

        /// <summary>The amount the percentage applies to.</summary>
        [JsonPropertyName("base_amount")]
        public decimal? BaseAmount { get; init; }
    }

    /// <summary>An amount deducted from a document or from one of its lines.</summary>
    /// <remarks>The vendor's rules mirror <see cref="MarminAeCharge" /> exactly.</remarks>
    public sealed record MarminAeAllowance
    {
        /// <summary>The free-text reason for the allowance.</summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; init; }

        /// <summary>The coded reason for the allowance.</summary>
        [JsonPropertyName("reason_code")]
        public string? ReasonCode { get; init; }

        /// <summary>The VAT treatment of the allowance.</summary>
        [JsonPropertyName("tax_category")]
        public MarminAeTaxCategory? TaxCategory { get; init; }

        /// <summary>The allowance amount.</summary>
        [JsonPropertyName("amount")]
        public decimal? Amount { get; init; }

        /// <summary>The percentage the allowance is computed at.</summary>
        [JsonPropertyName("multiplier_factor_numeric")]
        public decimal? MultiplierFactorNumeric { get; init; }

        /// <summary>The amount the percentage applies to.</summary>
        [JsonPropertyName("base_amount")]
        public decimal? BaseAmount { get; init; }
    }

    /// <summary>A discount applied to a line's unit price rather than to the line.</summary>
    public sealed record MarminAePriceAllowance
    {
        /// <summary>The discount taken off the unit price.</summary>
        [JsonPropertyName("amount")]
        public decimal? Amount { get; init; }

        /// <summary>The price the discount applies to.</summary>
        [JsonPropertyName("base_amount")]
        public decimal? BaseAmount { get; init; }
    }

    /// <summary>The rate converting a document's currency into the currency tax is reported in.</summary>
    /// <remarks>
    ///     Required whenever the document currency is not the local one; all three members are
    ///     required by the vendor when the object is present.
    /// </remarks>
    public sealed record MarminAeTaxExchangeRate
    {
        /// <summary>The document's own currency.</summary>
        [JsonPropertyName("source_currency_code")]
        public string? SourceCurrencyCode { get; init; }

        /// <summary>The currency tax is reported in.</summary>
        [JsonPropertyName("target_currency_code")]
        public string? TargetCurrencyCode { get; init; }

        /// <summary>The rate applied.</summary>
        [JsonPropertyName("calculation_rate")]
        public decimal? CalculationRate { get; init; }
    }

    /// <summary>The period a document, or one of its lines, covers.</summary>
    /// <remarks>
    ///     The vendor obliges at least one of the two dates when the object is present, and a line
    ///     period must fall inside the document period when both are sent.
    /// </remarks>
    public sealed record MarminAeInvoicePeriod
    {
        /// <summary>The first day of the period.</summary>
        [JsonPropertyName("start_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? StartDate { get; init; }

        /// <summary>The last day of the period.</summary>
        [JsonPropertyName("end_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? EndDate { get; init; }

        /// <summary>How often the period recurs, as a code from the vendor's frequency list.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; init; }
    }

    /// <summary>The terms payment is due under.</summary>
    public sealed record MarminAePaymentTerms
    {
        /// <summary>The terms in prose.</summary>
        [JsonPropertyName("note")]
        public string? Note { get; init; }

        /// <summary>The day payment falls due.</summary>
        [JsonPropertyName("due_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? DueDate { get; init; }

        /// <summary>The day an instalment falls due.</summary>
        [JsonPropertyName("installment_due_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? InstallmentDueDate { get; init; }

        /// <summary>The amount the terms cover.</summary>
        [JsonPropertyName("amount")]
        public decimal? Amount { get; init; }

        /// <summary>The payment instruction these terms attach to.</summary>
        [JsonPropertyName("payment_means_id")]
        public string? PaymentMeansId { get; init; }
    }
}
