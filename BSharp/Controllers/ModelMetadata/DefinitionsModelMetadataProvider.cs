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

            ///// Below are types which have dynamic properties
            //if (key.ModelType.IsSameOrSubClassOf<ResourceForSave>())
            //{
            //    // Get the route data from http context
            //    // Loop over the properties and special treatment to the dynamic ones
            //    foreach (var propDetails in propsDetails)
            //    {
            //        switch (propDetails.Key.Name)
            //        {
            //            case nameof(ResourceForSave.PreferredSupplierId):
            //                propDetails.DisplayMetadata = new DisplayMetadata
            //                {
            //                    DisplayName = () =>
            //                    {
            //                        var tenantId = _tenantIdAccessor.GetTenantId();
            //                        var definitions = _definitionsCache.GetDefinitionsIfCached(tenantId)?.Resources ?? 
            //                            throw new InvalidOperationException($"The definitions for tenantId {tenantId} were missing from the cache");

            //                        var routeData = _httpContextAccessor.HttpContext.GetRouteData();
            //                        var resourceType = routeData.Values["resourceType"]?.ToString();
            //                        var definition = definitions[resourceType];

            //                        var label = definition.ToString(); // TODO
            //                        return label;
            //                    }
            //                };
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}

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
                            string result;

                            switch (lang)
                            {
                                case Language.Primary:
                                    result = _localizer[name] + PrimaryPostfix(info);
                                    break;

                                case Language.Secondary:
                                    // An empty name indicates a hidden column
                                    result = string.IsNullOrWhiteSpace(info.SecondaryLanguageId) ?
                                    "" : _localizer[name] + SecondaryPostfix(info);
                                    break;

                                case Language.Ternary:
                                    // An empty name indicates a hidden column
                                    result = string.IsNullOrWhiteSpace(info.TernaryLanguageId) ?
                                    "" : _localizer[name] + TernaryPostfix(info);
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
}
