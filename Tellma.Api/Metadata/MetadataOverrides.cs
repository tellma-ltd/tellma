using System;

namespace Tellma.Api.Metadata
{
    public class MetadataOverrides : ITenantLanguages // One MetadataOverrides is cached per tenantId
    {
        private readonly IPropertyMetadataOverridesProvider _provider;

        public MetadataOverrides(int? tenantId, ITenantLanguages languages, IPropertyMetadataOverridesProvider provider)
        {
            TenantId = tenantId;
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            PrimaryLanguageId = languages?.PrimaryLanguageId;
            PrimaryLanguageSymbol = languages?.PrimaryLanguageSymbol;
            SecondaryLanguageId = languages?.SecondaryLanguageId;
            SecondaryLanguageSymbol = languages?.SecondaryLanguageSymbol;
            TernaryLanguageId = languages?.TernaryLanguageId;
            TernaryLanguageSymbol = languages?.TernaryLanguageSymbol;
        }

        public int? TenantId { get; }

        public string PrimaryLanguageId { get; }
        public string PrimaryLanguageSymbol { get; }
        public string SecondaryLanguageId { get; }
        public string SecondaryLanguageSymbol { get; }
        public string TernaryLanguageId { get; }
        public string TernaryLanguageSymbol { get; }

        public PropertyMetadataOverrides PropertyOverrides(Type type, int? definitionId, string propName, Func<string> display) => 
            _provider.PropertyOverrides(type, definitionId, propName, display);
    }

    public interface ITenantLanguages
    {
        public string PrimaryLanguageId { get; }
        public string PrimaryLanguageSymbol { get; }
        public string SecondaryLanguageId { get; }
        public string SecondaryLanguageSymbol { get; }
        public string TernaryLanguageId { get; }
        public string TernaryLanguageSymbol { get; }
    }

    public interface IPropertyMetadataOverridesProvider
    {
        public PropertyMetadataOverrides PropertyOverrides(Type type, int? definitionId, string propName, Func<string> display);
    }
}
