// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>A party on a document as the vendor holds it.</summary>
    /// <remarks>
    ///     A superset of what a submission can express: the vendor adds the profile the party
    ///     resolved to and a handful of registration details it fills in itself.
    /// </remarks>
    public sealed record MarminAeParty
    {
        /// <summary>The party's display name.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>The party's legal name.</summary>
        [JsonPropertyName("party_name")]
        public string? PartyName { get; init; }

        /// <summary>The business profile the party resolved to, when it is one of ours.</summary>
        [JsonPropertyName("profile_id")]
        public string? ProfileId { get; init; }

        /// <summary>The party's postal address.</summary>
        [JsonPropertyName("postal_address")]
        public MarminAeAddress? PostalAddress { get; init; }

        /// <summary>The party's VAT registration.</summary>
        [JsonPropertyName("party_tax_scheme")]
        public MarminAePartyTaxScheme? PartyTaxScheme { get; init; }

        /// <summary>The party's taxpayer identification number.</summary>
        [JsonPropertyName("tin")]
        public string? Tin { get; init; }

        /// <summary>The party's network address.</summary>
        [JsonPropertyName("endpoint_id")]
        public string? EndpointId { get; init; }

        /// <summary>The scheme the network address belongs to.</summary>
        [JsonPropertyName("endpoint_scheme_id")]
        public string? EndpointSchemeId { get; init; }

        /// <summary>Which kind of registration the party is identified by.</summary>
        [JsonPropertyName("scheme_agency_id")]
        public string? SchemeAgencyId { get; init; }

        /// <summary>The party's registration number.</summary>
        [JsonPropertyName("company_id")]
        public string? CompanyId { get; init; }

        /// <summary>The scheme the registration number belongs to.</summary>
        [JsonPropertyName("company_scheme_id")]
        public string? CompanySchemeId { get; init; }

        /// <summary>The authority behind a trade-licence registration.</summary>
        [JsonPropertyName("authority_name")]
        public string? AuthorityName { get; init; }

        /// <summary>The country behind a passport registration.</summary>
        [JsonPropertyName("passport_issuing_country_code")]
        public string? PassportIssuingCountryCode { get; init; }

        /// <summary>The party's legal form.</summary>
        [JsonPropertyName("company_legal_form")]
        public string? CompanyLegalForm { get; init; }

        /// <summary>The party's email address.</summary>
        [JsonPropertyName("email")]
        public string? Email { get; init; }

        /// <summary>The party's telephone number.</summary>
        [JsonPropertyName("telephone")]
        public string? Telephone { get; init; }

        /// <summary>Where the party's logo is hosted.</summary>
        /// <remarks>
        ///     A string rather than a URI: the vendor sends an empty one for a party with no logo,
        ///     and an empty string is not a URI.
        /// </remarks>
        [JsonPropertyName("logo_url")]
        public string? LogoUrl { get; init; }
    }

    /// <summary>A file held against a document.</summary>
    /// <remarks>
    ///     Metadata only. The bytes are fetched separately, which is why nothing here corresponds
    ///     to the Base64 content a submission carries.
    /// </remarks>
    public sealed record MarminAeAttachment
    {
        /// <summary>The identifier the download route takes.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The stored file name.</summary>
        [JsonPropertyName("file_name")]
        public string? FileName { get; init; }

        /// <summary>The stored media type.</summary>
        [JsonPropertyName("file_type")]
        public string? FileType { get; init; }
    }

    /// <summary>The VAT owed at one category and rate.</summary>
    /// <remarks>
    ///     The category and the rate each arrive under one of two names depending on the endpoint,
    ///     so both are read and the computed members report whichever arrived.
    /// </remarks>
    public sealed record MarminAeTaxBreakdownItem
    {
        /// <summary>The tax category, under one of the two names the vendor uses.</summary>
        [JsonPropertyName("tax_category_code")]
        public string? TaxCategoryCode { get; init; }

        /// <summary>The tax category, under the other.</summary>
        [JsonPropertyName("tax_category_id")]
        public string? TaxCategoryId { get; init; }

        /// <summary>The tax category, from whichever field carried it.</summary>
        [JsonIgnore]
        public string? TaxCategory => TaxCategoryCode ?? TaxCategoryId;

        /// <summary>The rate, under one of the two names the vendor uses.</summary>
        [JsonPropertyName("tax_percentage")]
        public decimal? TaxPercentage { get; init; }

        /// <summary>The rate, under the other.</summary>
        [JsonPropertyName("percent")]
        public decimal? Percent { get; init; }

        /// <summary>The rate, from whichever field carried it.</summary>
        [JsonIgnore]
        public decimal? Rate => TaxPercentage ?? Percent;

        /// <summary>How much of the document falls in this category.</summary>
        [JsonPropertyName("taxable_amount")]
        public decimal? TaxableAmount { get; init; }

        /// <summary>The VAT owed on it.</summary>
        [JsonPropertyName("tax_amount")]
        public decimal? TaxAmount { get; init; }
    }

    /// <summary>What the vendor knows about a document beyond its content.</summary>
    public sealed record MarminAeDocumentMetaInfo
    {
        /// <summary>How far the document has got through validation and transmission.</summary>
        [JsonPropertyName("peppol_status")]
        public MarminAeDocumentPeppolStatus? PeppolStatus { get; init; }
    }

    /// <summary>A document's transmission outcome, as summarised on the document itself.</summary>
    /// <remarks>
    ///     <para>
    ///         Distinct from the standalone status snapshot, which is a different shape read from a
    ///         different route: this one summarises, that one details the two legs.
    ///     </para>
    ///     <para>
    ///         Acceptance of a submission is not the end of the story. Validation and transmission
    ///         run afterwards and can fail a document that was accepted, so
    ///         <see cref="OverallStatus" /> — not the status code of the submission — is what says
    ///         whether a document is good.
    ///     </para>
    /// </remarks>
    public sealed record MarminAeDocumentPeppolStatus
    {
        /// <summary>How the delivery leg to the buyer went.</summary>
        [JsonPropertyName("participant_status")]
        public string? ParticipantStatus { get; init; }

        /// <summary>How the reporting leg to the authority went.</summary>
        [JsonPropertyName("fta_status")]
        public string? FtaStatus { get; init; }

        /// <summary>The outcome to branch on.</summary>
        [JsonPropertyName("overall_status")]
        public string? OverallStatus { get; init; }

        /// <summary>What validation objected to, when it objected.</summary>
        /// <remarks>
        ///     Left as raw JSON: the vendor publishes no shape for these entries, and guessing one
        ///     would silently drop whatever the guess did not anticipate — which on a validation
        ///     failure is exactly the part someone needs.
        /// </remarks>
        [JsonPropertyName("validation_results")]
        public IReadOnlyList<JsonElement>? ValidationResults { get; init; }
    }
}
