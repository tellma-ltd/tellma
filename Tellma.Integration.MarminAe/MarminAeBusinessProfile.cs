// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>One of an organization's business profiles.</summary>
    /// <remarks>
    ///     A profile is the entity documents are issued under, and its identifier is what every
    ///     submission route names. Reading one is how a caller confirms at startup that the profile
    ///     it is configured with exists and has finished onboarding.
    /// </remarks>
    public sealed record MarminAeBusinessProfile
    {
        /// <summary>The vendor's internal identifier for the profile record.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The organization the profile belongs to.</summary>
        [JsonPropertyName("org_id")]
        public string? OrgId { get; init; }

        /// <summary>The identifier every submission route names.</summary>
        [JsonPropertyName("profile_id")]
        public string? ProfileId { get; init; }

        /// <summary>How far the profile has got through onboarding.</summary>
        /// <remarks>
        ///     A profile that has not finished onboarding cannot issue documents. The vendor does
        ///     not publish this vocabulary exhaustively, so it stays a string.
        /// </remarks>
        [JsonPropertyName("status")]
        public string? Status { get; init; }

        /// <summary>The profile's display name.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>The legal name of the entity.</summary>
        [JsonPropertyName("party_name")]
        public string? PartyName { get; init; }

        /// <summary>The legal name in the local language.</summary>
        [JsonPropertyName("party_name_in_local_language")]
        public string? PartyNameInLocalLanguage { get; init; }

        /// <summary>Where the profile's logo is hosted.</summary>
        /// <remarks>A string rather than a URI: the vendor sends an empty one when there is none.</remarks>
        [JsonPropertyName("logo_url")]
        public string? LogoUrl { get; init; }

        /// <summary>The profile's contact email.</summary>
        [JsonPropertyName("email")]
        public string? Email { get; init; }

        /// <summary>The profile's telephone number.</summary>
        [JsonPropertyName("telephone")]
        public string? Telephone { get; init; }

        /// <summary>Which kind of registration the entity is identified by.</summary>
        [JsonPropertyName("scheme_agency_id")]
        public string? SchemeAgencyId { get; init; }

        /// <summary>
        ///     The trade licence or registration number. Distinct from the VAT number on
        ///     <see cref="PartyTaxScheme" />.
        /// </summary>
        [JsonPropertyName("company_id")]
        public string? CompanyId { get; init; }

        /// <summary>The authority behind a trade-licence registration.</summary>
        [JsonPropertyName("authority_name")]
        public string? AuthorityName { get; init; }

        /// <summary>The entity's VAT registration.</summary>
        [JsonPropertyName("party_tax_scheme")]
        public MarminAePartyTaxScheme? PartyTaxScheme { get; init; }

        /// <summary>The entity's taxpayer identification number.</summary>
        [JsonPropertyName("tin")]
        public string? Tin { get; init; }

        /// <summary>The profile's network address.</summary>
        [JsonPropertyName("endpoint_id")]
        public string? EndpointId { get; init; }

        /// <summary>The scheme the network address belongs to.</summary>
        [JsonPropertyName("endpoint_scheme_id")]
        public string? EndpointSchemeId { get; init; }

        /// <summary>The profile's postal address.</summary>
        [JsonPropertyName("postal_address")]
        public MarminAeBusinessProfileAddress? PostalAddress { get; init; }

        /// <summary>Who created the profile record.</summary>
        [JsonPropertyName("created_by")]
        public string? CreatedBy { get; init; }

        /// <summary>Who last changed it.</summary>
        [JsonPropertyName("updated_by")]
        public string? UpdatedBy { get; init; }

        /// <summary>When it was created, in Unix seconds.</summary>
        [JsonPropertyName("created_at")]
        public long? CreatedAtSeconds { get; init; }

        /// <summary>When it was created.</summary>
        [JsonIgnore]
        public DateTimeOffset? CreatedAt => CreatedAtSeconds is long value
            ? DateTimeOffset.FromUnixTimeSeconds(value)
            : null;

        /// <summary>When it last changed, in Unix seconds.</summary>
        [JsonPropertyName("updated_at")]
        public long? UpdatedAtSeconds { get; init; }

        /// <summary>When it last changed.</summary>
        [JsonIgnore]
        public DateTimeOffset? UpdatedAt => UpdatedAtSeconds is long value
            ? DateTimeOffset.FromUnixTimeSeconds(value)
            : null;
    }

    /// <summary>A business profile's postal address.</summary>
    /// <remarks>
    ///     The document address with an identifier on it: the vendor persists a profile's address
    ///     as a record of its own, and returns that record's id alongside the address.
    /// </remarks>
    public sealed record MarminAeBusinessProfileAddress
    {
        /// <summary>The identifier of the persisted address record.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The street name or first address line.</summary>
        [JsonPropertyName("street_name")]
        public string? StreetName { get; init; }

        /// <summary>The second address line.</summary>
        [JsonPropertyName("additional_street_name")]
        public string? AdditionalStreetName { get; init; }

        /// <summary>The city or town.</summary>
        [JsonPropertyName("city_name")]
        public string? CityName { get; init; }

        /// <summary>The postal or ZIP code.</summary>
        [JsonPropertyName("postal_zone")]
        public string? PostalZone { get; init; }

        /// <summary>The emirate or region code.</summary>
        [JsonPropertyName("country_subentity")]
        public string? CountrySubentity { get; init; }

        /// <summary>A further unstructured address line.</summary>
        [JsonPropertyName("address_line")]
        public string? AddressLine { get; init; }

        /// <summary>The country name.</summary>
        [JsonPropertyName("country")]
        public string? Country { get; init; }

        /// <summary>The ISO 3166-1 alpha-2 country code.</summary>
        [JsonPropertyName("country_code")]
        public string? CountryCode { get; init; }
    }
}
