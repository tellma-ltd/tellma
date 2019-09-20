using BSharp.Controllers.Dto;
using BSharp.Data;
using BSharp.Entities;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
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

namespace BSharp.Controllers
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
                        // All dynamically labelled properties
                        case nameof(Resource.MassUnit):
                        case nameof(Resource.MassUnitId):
                            displayMetadata = LocalizeResourceProperty(e => e.MassUnit_Label, e => e.MassUnit_Label2, e => e.MassUnit_Label3, defaultName);
                            break;

                        case nameof(Resource.VolumeUnit):
                        case nameof(Resource.VolumeUnitId):
                            displayMetadata = LocalizeResourceProperty(e => e.VolumeUnit_Label, e => e.VolumeUnit_Label2, e => e.VolumeUnit_Label3, defaultName);
                            break;

                        case nameof(Resource.AreaUnit):
                        case nameof(Resource.AreaUnitId):
                            displayMetadata = LocalizeResourceProperty(e => e.AreaUnit_Label, e => e.AreaUnit_Label2, e => e.AreaUnit_Label3, defaultName);
                            break;

                        case nameof(Resource.LengthUnit):
                        case nameof(Resource.LengthUnitId):
                            displayMetadata = LocalizeResourceProperty(e => e.LengthUnit_Label, e => e.LengthUnit_Label2, e => e.LengthUnit_Label3, defaultName);
                            break;

                        case nameof(Resource.TimeUnit):
                        case nameof(Resource.TimeUnitId):
                            displayMetadata = LocalizeResourceProperty(e => e.TimeUnit_Label, e => e.TimeUnit_Label2, e => e.TimeUnit_Label3, defaultName);
                            break;

                        case nameof(Resource.CountUnit):
                        case nameof(Resource.CountUnitId):
                            displayMetadata = LocalizeResourceProperty(e => e.CountUnit_Label, e => e.CountUnit_Label2, e => e.CountUnit_Label3, defaultName);
                            break;

                        case nameof(Resource.Memo):
                            displayMetadata = LocalizeResourceProperty(e => e.Memo_Label, e => e.Memo_Label2, e => e.Memo_Label3, defaultName);
                            break;

                        case nameof(Resource.CustomsReference):
                            displayMetadata = LocalizeResourceProperty(e => e.CustomsReference_Label, e => e.CustomsReference_Label2, e => e.CustomsReference_Label3, defaultName);
                            break;

                        case nameof(Resource.ResourceLookup1):
                        case nameof(Resource.ResourceLookup1Id):
                            displayMetadata = LocalizeResourceProperty(e => e.ResourceLookup1_Label, e => e.ResourceLookup1_Label2, e => e.ResourceLookup1_Label3, defaultName);
                            break;

                        case nameof(Resource.ResourceLookup2):
                        case nameof(Resource.ResourceLookup2Id):
                            displayMetadata = LocalizeResourceProperty(e => e.ResourceLookup2_Label, e => e.ResourceLookup2_Label2, e => e.ResourceLookup2_Label3, defaultName);
                            break;

                        case nameof(Resource.ResourceLookup3):
                        case nameof(Resource.ResourceLookup3Id):
                            displayMetadata = LocalizeResourceProperty(e => e.ResourceLookup3_Label, e => e.ResourceLookup3_Label2, e => e.ResourceLookup3_Label3, defaultName);
                            break;

                        case nameof(Resource.ResourceLookup4):
                        case nameof(Resource.ResourceLookup4Id):
                            displayMetadata = LocalizeResourceProperty(e => e.ResourceLookup4_Label, e => e.ResourceLookup4_Label2, e => e.ResourceLookup4_Label3, defaultName);
                            break;
                        default:
                            displayMetadata = null;
                            break;
                    }

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

                            string result;

                            switch (lang)
                            {
                                case Language.Primary:
                                    result = _localizer[name] + PrimaryPostfix(info);
                                    break;

                                case Language.Secondary:
                                    result = string.IsNullOrWhiteSpace(info.SecondaryLanguageId) ?
                                    Constants.HIDDEN_FIELD : _localizer[name] + SecondaryPostfix(info);
                                    break;

                                case Language.Ternary:
                                    result = string.IsNullOrWhiteSpace(info.TernaryLanguageId) ?
                                    Constants.HIDDEN_FIELD : _localizer[name] + TernaryPostfix(info);
                                    break;

                                default:
                                    result = _localizer[name];
                                    break;
                            };

                            return result;
                        }
                    };
                }
            }

            return propsDetails;
        }

        DisplayMetadata LocalizeResourceProperty(
            Func<ResourceDefinitionForClient, string> s1Func,
            Func<ResourceDefinitionForClient, string> s2Func,
            Func<ResourceDefinitionForClient, string> s3Func,
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
                        var definition = _definitionsCache.GetDefinitionsIfCached(tenantId)?.Data?.Resources?.GetValueOrDefault(definitionId);

                        if(definition != null)
                        {
                            result = _tenantInfoAccessor.Localize(
                                s1Func(definition),
                                s2Func(definition),
                                s3Func(definition)) ?? result;
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

    public static class MetadataProviderExtensions
    {
        public static string Localize(this ITenantInfoAccessor tenantInfoAccessor, string s, string s2, string s3)
        {
            var cultureName = CultureInfo.CurrentUICulture.Name;
            var tenantInfo = tenantInfoAccessor.GetCurrentInfo();

            var currentLangIndex = cultureName == tenantInfo.TernaryLanguageId ? 3 : cultureName == tenantInfo.SecondaryLanguageId ? 2 : 1;
            return currentLangIndex == 3 ? (s3 ?? s) : currentLangIndex == 2 ? (s2 ?? s) : s;
        }
    }
}
