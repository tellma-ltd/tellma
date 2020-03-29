using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Tellma.Controllers
{
    /// <summary>
    /// This provider dynamically sets the display names of entity properties based on
    /// the current culture and the definitions that are loaded from the database
    /// </summary>
    public class DefinitionsModelMetadataProvider : DefaultModelMetadataProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly ITenantInfoAccessor _tenantInfoAccessor;
        private readonly IStringLocalizer _localizer;

        public DefinitionsModelMetadataProvider(
            ICompositeMetadataDetailsProvider detailsProvider,
            ITenantIdAccessor tenantIdAccessor,
            IHttpContextAccessor httpContextAccessor,
            IDefinitionsCache definitionsCache,
            ITenantInfoAccessor tenantInfoAccessor,
            IStringLocalizer<Strings> localizer) : base(detailsProvider)
        {
            _tenantIdAccessor = tenantIdAccessor;
            _httpContextAccessor = httpContextAccessor;
            _definitionsCache = definitionsCache;
            _tenantInfoAccessor = tenantInfoAccessor;
            _localizer = localizer;
        }

        public DefinitionsModelMetadataProvider(
            ICompositeMetadataDetailsProvider detailsProvider,
            ITenantIdAccessor tenantIdAccessor,
            IHttpContextAccessor httpContextAccessor,
            IDefinitionsCache definitionsCache,
            ITenantInfoAccessor tenantInfoAccessor,
            IStringLocalizer<Strings> localizer,
            IOptions<MvcOptions> optionsAccessor) : base(detailsProvider, optionsAccessor)
        {
            _tenantIdAccessor = tenantIdAccessor;
            _httpContextAccessor = httpContextAccessor;
            _definitionsCache = definitionsCache;
            _tenantInfoAccessor = tenantInfoAccessor;
            _localizer = localizer;
        }

        protected override DefaultMetadataDetails[] CreatePropertyDetails(ModelMetadataIdentity key)
        {
            // Call the base implementation
            var propsDetails = base.CreatePropertyDetails(key);

            // Customize the label of Resource properties
            if (key.ModelType.IsSameOrSubClassOf<ResourceForSave>())
            {
                // Get the route data from http context
                // Loop over the properties and special treatment to the dynamic ones
                foreach (var propDetails in propsDetails)
                {
                    var defaultName = propDetails.ModelAttributes.PropertyAttributes
                        .OfType<DisplayAttribute>().FirstOrDefault()?.Name ?? propDetails.Key.Name;

                    propDetails.DisplayMetadata = propDetails.Key.Name switch
                    {
                        nameof(Resource.Identifier) => LocalizeResourceProperty(e => e.IdentifierVisibility, e => e.IdentifierLabel, e => e.IdentifierLabel2, e => e.IdentifierLabel3, defaultName),
                        // All dynamically labelled properties
                        nameof(Resource.Currency) => LocalizeResourceProperty(e => e.CurrencyVisibility, e => e.CurrencyLabel, e => e.CurrencyLabel2, e => e.CurrencyLabel3, defaultName),
                        nameof(Resource.CurrencyId) => LocalizeResourceProperty(e => e.CurrencyVisibility, e => e.CurrencyLabel, e => e.CurrencyLabel2, e => e.CurrencyLabel3, defaultName),
                        nameof(Resource.MonetaryValue) => LocalizeResourceProperty(e => e.MonetaryValueVisibility, e => e.MonetaryValueLabel, e => e.MonetaryValueLabel2, e => e.MonetaryValueLabel3, defaultName),
                        nameof(Resource.AvailableSince) => LocalizeResourceProperty(e => e.AvailableSinceVisibility, e => e.AvailableSinceLabel, e => e.AvailableSinceLabel2, e => e.AvailableSinceLabel3, defaultName),
                        nameof(Resource.AvailableTill) => LocalizeResourceProperty(e => e.AvailableTillVisibility, e => e.AvailableTillLabel, e => e.AvailableTillLabel2, e => e.AvailableTillLabel3, defaultName),
                        nameof(Resource.Decimal1) => LocalizeResourceProperty(e => e.Decimal1Visibility, e => e.Decimal1Label, e => e.Decimal1Label2, e => e.Decimal1Label3, defaultName),
                        nameof(Resource.Decimal2) => LocalizeResourceProperty(e => e.Decimal2Visibility, e => e.Decimal2Label, e => e.Decimal2Label2, e => e.Decimal2Label3, defaultName),
                        nameof(Resource.Int1) => LocalizeResourceProperty(e => e.Int1Visibility, e => e.Int1Label, e => e.Int1Label2, e => e.Int1Label3, defaultName),
                        nameof(Resource.Int2) => LocalizeResourceProperty(e => e.Int2Visibility, e => e.Int2Label, e => e.Int2Label2, e => e.Int2Label3, defaultName),
                        nameof(Resource.Lookup1) => LocalizeResourceProperty(e => e.Lookup1Visibility, e => e.Lookup1Label, e => e.Lookup1Label2, e => e.Lookup1Label3, defaultName),
                        nameof(Resource.Lookup1Id) => LocalizeResourceProperty(e => e.Lookup1Visibility, e => e.Lookup1Label, e => e.Lookup1Label2, e => e.Lookup1Label3, defaultName),
                        nameof(Resource.Lookup2) => LocalizeResourceProperty(e => e.Lookup2Visibility, e => e.Lookup2Label, e => e.Lookup2Label2, e => e.Lookup2Label3, defaultName),
                        nameof(Resource.Lookup2Id) => LocalizeResourceProperty(e => e.Lookup2Visibility, e => e.Lookup2Label, e => e.Lookup2Label2, e => e.Lookup2Label3, defaultName),
                        nameof(Resource.Lookup3) => LocalizeResourceProperty(e => e.Lookup3Visibility, e => e.Lookup3Label, e => e.Lookup3Label2, e => e.Lookup3Label3, defaultName),
                        nameof(Resource.Lookup3Id) => LocalizeResourceProperty(e => e.Lookup3Visibility, e => e.Lookup3Label, e => e.Lookup3Label2, e => e.Lookup3Label3, defaultName),
                        nameof(Resource.Lookup4) => LocalizeResourceProperty(e => e.Lookup4Visibility, e => e.Lookup4Label, e => e.Lookup4Label2, e => e.Lookup4Label3, defaultName),
                        nameof(Resource.Lookup4Id) => LocalizeResourceProperty(e => e.Lookup4Visibility, e => e.Lookup4Label, e => e.Lookup4Label2, e => e.Lookup4Label3, defaultName),
                        //nameof(Resource.Lookup5) => LocalizeResourceProperty(e => e.Lookup5Visibility, e => e.Lookup5Label, e => e.Lookup5Label2, e => e.Lookup5Label3, defaultName),
                        //nameof(Resource.Lookup5Id) => LocalizeResourceProperty(e => e.Lookup5Visibility, e => e.Lookup5Label, e => e.Lookup5Label2, e => e.Lookup5Label3, defaultName),
                        nameof(Resource.Text1) => LocalizeResourceProperty(e => e.Text1Visibility, e => e.Text1Label, e => e.Text1Label2, e => e.Text1Label3, defaultName),
                        nameof(Resource.Text2) => LocalizeResourceProperty(e => e.Text2Visibility, e => e.Text2Label, e => e.Text2Label2, e => e.Text2Label3, defaultName),
                        _ => null,
                    };
                }
            }

            // Customize the label of Agent properties
            if (key.ModelType.IsSameOrSubClassOf<AgentForSave>())
            {
                // Get the route data from http context
                // Loop over the properties and special treatment to the dynamic ones
                foreach (var propDetails in propsDetails)
                {
                    var defaultName = propDetails.ModelAttributes.PropertyAttributes
                        .OfType<DisplayAttribute>().FirstOrDefault()?.Name ?? propDetails.Key.Name;
                    propDetails.DisplayMetadata = propDetails.Key.Name switch
                    {
                        // All dynamically labelled properties
                        nameof(Agent.TaxIdentificationNumber) => LocalizeAgentSpecificProperty(e => e.TaxIdentificationNumberVisibility, defaultName),
                        nameof(Agent.StartDate) => LocalizeAgentProperty(e => e.StartDateVisibility, e => e.StartDateLabel, e => e.StartDateLabel2, e => e.StartDateLabel3, defaultName),
                        nameof(Agent.JobId) => LocalizeAgentSpecificProperty(e => e.JobVisibility, defaultName),//  case nameof(Agent.Job): TODO
                        nameof(Agent.Rates) => LocalizeAgentSpecificProperty(e => e.RatesVisibility, defaultName),
                        nameof(Agent.BankAccountNumber) => LocalizeAgentSpecificProperty(e => e.BankAccountNumberVisibility, defaultName),
                        _ => null,
                    };
                }
            }

            if (key.ModelType.IsSameOrSubClassOf<DocumentForSave>())
            {
                // Get the route data from http context
                // Loop over the properties and special treatment to the dynamic ones
                foreach (var propDetails in propsDetails)
                {
                    var defaultName = propDetails.ModelAttributes.PropertyAttributes
                        .OfType<DisplayAttribute>().FirstOrDefault()?.Name ?? propDetails.Key.Name;

                    propDetails.DisplayMetadata = propDetails.Key.Name switch
                    {
                        // All dynamically labelled properties
                        nameof(Document.Memo) => LocalizeDocumentProperty(e => e.MemoVisibility, e => e.MemoLabel, e => e.MemoLabel2, e => e.MemoLabel3, defaultName),
                        nameof(Document.DebitAgentId) => LocalizeDocumentSpecificProperty(e => e.DebitAgentVisibility, e => e.DebitAgentLabel, e => e.DebitAgentLabel2, e => e.DebitAgentLabel3, defaultName),
                        nameof(Document.DebitAgent) => LocalizeDocumentSpecificProperty(e => e.DebitAgentVisibility, e => e.DebitAgentLabel, e => e.DebitAgentLabel2, e => e.DebitAgentLabel3, defaultName),
                        nameof(Document.CreditAgentId) => LocalizeDocumentSpecificProperty(e => e.CreditAgentVisibility, e => e.CreditAgentLabel, e => e.CreditAgentLabel2, e => e.CreditAgentLabel3, defaultName),
                        nameof(Document.CreditAgent) => LocalizeDocumentSpecificProperty(e => e.CreditAgentVisibility, e => e.CreditAgentLabel, e => e.CreditAgentLabel2, e => e.CreditAgentLabel3, defaultName),
                        nameof(Document.NotedAgentId) => LocalizeDocumentSpecificProperty(e => e.NotedAgentVisibility, e => e.NotedAgentLabel, e => e.NotedAgentLabel2, e => e.NotedAgentLabel3, defaultName),
                        nameof(Document.NotedAgent) => LocalizeDocumentSpecificProperty(e => e.NotedAgentVisibility, e => e.NotedAgentLabel, e => e.NotedAgentLabel2, e => e.NotedAgentLabel3, defaultName),
                        nameof(Document.InvestmentCenter) => LocalizeDocumentSpecificProperty(e => e.InvestmentCenterVisibility, e => e.InvestmentCenterLabel, e => e.InvestmentCenterLabel2, e => e.InvestmentCenterLabel3, defaultName),
                        nameof(Document.InvestmentCenterId) => LocalizeDocumentSpecificProperty(e => e.InvestmentCenterVisibility, e => e.InvestmentCenterLabel, e => e.InvestmentCenterLabel2, e => e.InvestmentCenterLabel3, defaultName),
                        nameof(Document.Time1) => LocalizeDocumentSpecificProperty(e => e.Time1Visibility, e => e.Time1Label, e => e.Time1Label2, e => e.Time1Label3, defaultName),
                        nameof(Document.Time2) => LocalizeDocumentSpecificProperty(e => e.Time2Visibility, e => e.Time2Label, e => e.Time2Label2, e => e.Time2Label3, defaultName),
                        nameof(Document.Quantity) => LocalizeDocumentSpecificProperty(e => e.QuantityVisibility, e => e.QuantityLabel, e => e.QuantityLabel2, e => e.QuantityLabel3, defaultName),
                        nameof(Document.UnitId) => LocalizeDocumentSpecificProperty(e => e.UnitVisibility, e => e.UnitLabel, e => e.UnitLabel2, e => e.UnitLabel3, defaultName),
                        nameof(Document.Unit) => LocalizeDocumentSpecificProperty(e => e.UnitVisibility, e => e.UnitLabel, e => e.UnitLabel2, e => e.UnitLabel3, defaultName),
                        nameof(Document.CurrencyId) => LocalizeDocumentSpecificProperty(e => e.CurrencyVisibility, e => e.CurrencyLabel, e => e.CurrencyLabel2, e => e.CurrencyLabel3, defaultName),
                        nameof(Document.Currency) => LocalizeDocumentSpecificProperty(e => e.CurrencyVisibility, e => e.CurrencyLabel, e => e.CurrencyLabel2, e => e.CurrencyLabel3, defaultName),
                        _ => null,
                    };
                }
            }

            // In general: append the language name to the labels of multilingual
            foreach (var propDetails in propsDetails)
            {
                var att = propDetails.ModelAttributes.PropertyAttributes
                    .OfType<MultilingualDisplayAttribute>().FirstOrDefault();

                if (att != null)
                {
                    var name = att.Name ?? "";
                    var lang = att.Language;

                    propDetails.DisplayMetadata = new DisplayMetadata
                    {
                        DisplayName = () =>
                        {
                            var info = _tenantInfoAccessor.GetCurrentInfo();
                            if (info == null)
                            {
                                // Developer mistake
                                throw new InvalidOperationException("TenantInfo is not set");
                            }

                            var result = lang switch
                            {
                                Language.Primary => _localizer[name] + PrimaryPostfix(info),
                                Language.Secondary => string.IsNullOrWhiteSpace(info.SecondaryLanguageId) ? Constants.HIDDEN_FIELD : _localizer[name] + SecondaryPostfix(info),
                                Language.Ternary => string.IsNullOrWhiteSpace(info.TernaryLanguageId) ? Constants.HIDDEN_FIELD : _localizer[name] + TernaryPostfix(info),
                                _ => _localizer[name],
                            };
                            ;

                            return result;
                        }
                    };
                }
            }

            return propsDetails;
        }

        DisplayMetadata LocalizeResourceProperty(
            Func<ResourceDefinitionForClient, string> visibilityFunc,
            Func<ResourceDefinitionForClient, string> s1Func,
            Func<ResourceDefinitionForClient, string> s2Func,
            Func<ResourceDefinitionForClient, string> s3Func,
            string defaultDisplayName)
        {
            return LocalizeProperty(
                (tenantId, definitionId) => _definitionsCache.GetDefinitionsIfCached(tenantId)?.Data?.Resources?.GetValueOrDefault(definitionId),
                    visibilityFunc, s1Func, s2Func, s3Func, defaultDisplayName);
        }

        DisplayMetadata LocalizeAgentProperty(
            Func<AgentDefinitionForClient, string> visibilityFunc,
            Func<AgentDefinitionForClient, string> s1Func,
            Func<AgentDefinitionForClient, string> s2Func,
            Func<AgentDefinitionForClient, string> s3Func,
            string defaultDisplayName)
        {
            return LocalizeProperty(
                (tenantId, definitionId) => _definitionsCache.GetDefinitionsIfCached(tenantId)?.Data?.Agents?.GetValueOrDefault(definitionId),
                    visibilityFunc, s1Func, s2Func, s3Func, defaultDisplayName);
        }

        DisplayMetadata LocalizeAgentSpecificProperty(
            Func<AgentDefinitionForClient, string> visibilityFunc,
            string defaultDisplayName)
        {
            return LocalizeProperty(
                (tenantId, definitionId) => _definitionsCache.GetDefinitionsIfCached(tenantId)?.Data?.Agents?.GetValueOrDefault(definitionId),
                    visibilityFunc, e => null, e => null, e => null, defaultDisplayName);
        }

        DisplayMetadata LocalizeDocumentProperty(
            Func<DocumentDefinitionForClient, string> visibilityFunc,
            Func<DocumentDefinitionForClient, string> s1Func,
            Func<DocumentDefinitionForClient, string> s2Func,
            Func<DocumentDefinitionForClient, string> s3Func,
            string defaultDisplayName)
        {
            return LocalizeProperty(
                (tenantId, definitionId) => _definitionsCache.GetDefinitionsIfCached(tenantId)?.Data?.Documents?.GetValueOrDefault(definitionId),
                    visibilityFunc, s1Func, s2Func, s3Func, defaultDisplayName);
        }

        DisplayMetadata LocalizeDocumentSpecificProperty(
            Func<DocumentDefinitionForClient, bool> isVisibleFunc,
            Func<DocumentDefinitionForClient, string> s1Func,
            Func<DocumentDefinitionForClient, string> s2Func,
            Func<DocumentDefinitionForClient, string> s3Func,
            string defaultDisplayName)
        {
            // Changes the boolean visibility to string visibility
            string visibilityFunc(DocumentDefinitionForClient def) => isVisibleFunc(def) ? Visibility.Optional : null;

            return LocalizeProperty(
                (tenantId, definitionId) => _definitionsCache.GetDefinitionsIfCached(tenantId)?.Data?.Documents?.GetValueOrDefault(definitionId),
                    visibilityFunc, s1Func, s2Func, s3Func, defaultDisplayName);
        }

        DisplayMetadata LocalizeProperty<TDefinitionForClient>(
            Func<int, string, TDefinitionForClient> definitionFunc,
            Func<TDefinitionForClient, string> visibilityFunc,
            Func<TDefinitionForClient, string> s1Func,
            Func<TDefinitionForClient, string> s2Func,
            Func<TDefinitionForClient, string> s3Func,
            string defaultDisplayName)
        {
            return new DisplayMetadata
            {
                // Return a dynamic display name from the definitions, and fall back to
                // the default if non are available. Be as forgiving as possible
                DisplayName = () =>
                {
                    string result = _localizer[defaultDisplayName];
                    var routeData = _httpContextAccessor.HttpContext.GetRouteData();
                    var definitionId = routeData.Values["definitionId"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(definitionId))
                    {
                        var tenantId = _tenantIdAccessor.GetTenantId();
                        var definition = definitionFunc(tenantId, definitionId);

                        if (definition != null)
                        {
                            if (visibilityFunc(definition) == null)
                            {
                                result = Constants.HIDDEN_FIELD;
                            }
                            else
                            {
                                result = _tenantInfoAccessor.GetCurrentInfo().Localize(
                                    s1Func(definition),
                                    s2Func(definition),
                                    s3Func(definition)) ?? result;
                            }
                        }
                    }

                    return result;
                }
            };
        }

        private string PrimaryPostfix(TenantInfo info)
        {
            if (info != null && (info.SecondaryLanguageId != null || info.TernaryLanguageId != null))
            {
                return $" ({info.PrimaryLanguageSymbol})";
            }

            return "";
        }

        private string SecondaryPostfix(TenantInfo info)
        {
            if (info != null && info.SecondaryLanguageId != null)
            {
                return $" ({info.SecondaryLanguageSymbol})";
            }

            return "";
        }

        private string TernaryPostfix(TenantInfo info)
        {
            if (info != null && info.TernaryLanguageId != null)
            {
                return $" ({info.TernaryLanguageSymbol})";
            }

            return "";
        }
    }

    public static class TenantInfoAccessorExtensions
    {
        public static string Localize(this TenantInfo tenantInfo, string s, string s2, string s3)
        {
            var cultureName = CultureInfo.CurrentUICulture.Name;

            var currentLangIndex = cultureName == tenantInfo.TernaryLanguageId ? 3 : cultureName == tenantInfo.SecondaryLanguageId ? 2 : 1;
            return currentLangIndex == 3 ? (s3 ?? s) : currentLangIndex == 2 ? (s2 ?? s) : s;
        }
    }
}
