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

                    DisplayMetadata displayMetadata;

                    switch (propDetails.Key.Name)
                    {
                        case nameof(Resource.Identifier):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.IdentifierVisibility, e => e.IdentifierLabel, e => e.IdentifierLabel2, e => e.IdentifierLabel3, defaultName);
                            break;

                        // All dynamically labelled properties
                        case nameof(Resource.Currency):
                        case nameof(Resource.CurrencyId):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.CurrencyVisibility, e => e.CurrencyLabel, e => e.CurrencyLabel2, e => e.CurrencyLabel3, defaultName);
                            break;

                        case nameof(Resource.MonetaryValue):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.MonetaryValueVisibility, e => e.MonetaryValueLabel, e => e.MonetaryValueLabel2, e => e.MonetaryValueLabel3, defaultName);
                            break;

                        case nameof(Resource.CountUnit):
                        case nameof(Resource.CountUnitId):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.CountUnitVisibility, e => e.CountUnitLabel, e => e.CountUnitLabel2, e => e.CountUnitLabel3, defaultName);
                            break;

                        case nameof(Resource.Count):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.CountVisibility, e => e.CountLabel, e => e.CountLabel2, e => e.CountLabel3, defaultName);
                            break;

                        case nameof(Resource.MassUnit):
                        case nameof(Resource.MassUnitId):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.MassUnitVisibility, e => e.MassUnitLabel, e => e.MassUnitLabel2, e => e.MassUnitLabel3, defaultName);
                            break;

                        case nameof(Resource.Mass):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.MassVisibility, e => e.MassLabel, e => e.MassLabel2, e => e.MassLabel3, defaultName);
                            break;

                        case nameof(Resource.VolumeUnit):
                        case nameof(Resource.VolumeUnitId):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.VolumeUnitVisibility, e => e.VolumeUnitLabel, e => e.VolumeUnitLabel2, e => e.VolumeUnitLabel3, defaultName);
                            break;

                        case nameof(Resource.Volume):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.VolumeVisibility, e => e.VolumeLabel, e => e.VolumeLabel2, e => e.VolumeLabel3, defaultName);
                            break;

                        case nameof(Resource.TimeUnit):
                        case nameof(Resource.TimeUnitId):
                            displayMetadata = LocalizeResourceProperty
                                (e => e.TimeUnitVisibility, e => e.TimeUnitLabel, e => e.TimeUnitLabel2, e => e.TimeUnitLabel3, defaultName);
                            break;

                        case nameof(Resource.Time):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.TimeVisibility, e => e.TimeLabel, e => e.TimeLabel2, e => e.TimeLabel3, defaultName);
                            break;

                        case nameof(Resource.AvailableSince):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.AvailableSinceVisibility, e => e.AvailableSinceLabel, e => e.AvailableSinceLabel2, e => e.AvailableSinceLabel3, defaultName);
                            break;

                        case nameof(Resource.AvailableTill):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.AvailableTillVisibility, e => e.AvailableTillLabel, e => e.AvailableTillLabel2, e => e.AvailableTillLabel3, defaultName);
                            break;

                        case nameof(Resource.Decimal1):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.Decimal1Visibility, e => e.Decimal1Label, e => e.Decimal1Label2, e => e.Decimal1Label3, defaultName);
                            break;

                        case nameof(Resource.Decimal2):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.Decimal2Visibility, e => e.Decimal2Label, e => e.Decimal2Label2, e => e.Decimal2Label3, defaultName);
                            break;

                        case nameof(Resource.Int1):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.Int1Visibility, e => e.Int1Label, e => e.Int1Label2, e => e.Int1Label3, defaultName);
                            break;

                        case nameof(Resource.Int2):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.Int2Visibility, e => e.Int2Label, e => e.Int2Label2, e => e.Int2Label3, defaultName);
                            break;

                        case nameof(Resource.Lookup1):
                        case nameof(Resource.Lookup1Id):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.Lookup1Visibility, e => e.Lookup1Label, e => e.Lookup1Label2, e => e.Lookup1Label3, defaultName);
                            break;

                        case nameof(Resource.Lookup2):
                        case nameof(Resource.Lookup2Id):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.Lookup2Visibility, e => e.Lookup2Label, e => e.Lookup2Label2, e => e.Lookup2Label3, defaultName);
                            break;

                        case nameof(Resource.Lookup3):
                        case nameof(Resource.Lookup3Id):
                            displayMetadata = LocalizeResourceProperty(e => e.Lookup3Visibility, e => e.Lookup3Label, e => e.Lookup3Label2, e => e.Lookup3Label3, defaultName);
                            break;

                        case nameof(Resource.Lookup4):
                        case nameof(Resource.Lookup4Id):
                            displayMetadata = LocalizeResourceProperty(e => e.Lookup4Visibility, e => e.Lookup4Label, e => e.Lookup4Label2, e => e.Lookup4Label3, defaultName);
                            break;

                        //case nameof(Resource.Lookup5):
                        //case nameof(Resource.Lookup5Id):
                        //    displayMetadata = LocalizeResourceProperty(e => e.Lookup5Visibility, e => e.Lookup5Label, e => e.Lookup5Label2, e => e.Lookup5Label3, defaultName);
                        //    break;

                        case nameof(Resource.Text1):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.Text1Visibility, e => e.Text1Label, e => e.Text1Label2, e => e.Text1Label3, defaultName);
                            break;

                        case nameof(Resource.Text2):
                            displayMetadata = LocalizeResourceProperty(
                                e => e.Text2Visibility, e => e.Text2Label, e => e.Text2Label2, e => e.Text2Label3, defaultName);
                            break;

                        default:
                            displayMetadata = null;
                            break;
                    }

                    propDetails.DisplayMetadata = displayMetadata;
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
                    var displayMetadata = propDetails.Key.Name switch
                    {
                        // All dynamically labelled properties
                        nameof(Agent.TaxIdentificationNumber) => LocalizeAgentSpecificProperty(e => e.TaxIdentificationNumberVisibility, defaultName),
                        nameof(Agent.StartDate) => LocalizeAgentProperty(e => e.StartDateVisibility, e => e.StartDateLabel, e => e.StartDateLabel2, e => e.StartDateLabel3, defaultName),
                        nameof(Agent.JobId) => LocalizeAgentSpecificProperty(e => e.JobVisibility, defaultName),//  case nameof(Agent.Job): TODO
                        nameof(Agent.BasicSalary) => LocalizeAgentSpecificProperty(e => e.BasicSalaryVisibility, defaultName),
                        nameof(Agent.TransportationAllowance) => LocalizeAgentSpecificProperty(e => e.TransportationAllowanceVisibility, defaultName),
                        nameof(Agent.OvertimeRate) => LocalizeAgentSpecificProperty(e => e.OvertimeRateVisibility, defaultName),
                        nameof(Agent.BankAccountNumber) => LocalizeAgentSpecificProperty(e => e.BankAccountNumberVisibility, defaultName),
                        _ => null,
                    };
                    propDetails.DisplayMetadata = displayMetadata;
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
