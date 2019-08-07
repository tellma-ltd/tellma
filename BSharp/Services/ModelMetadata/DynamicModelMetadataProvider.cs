using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
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

namespace BSharp.Services.ModelMetadata
{
    /// <summary>
    /// The purpose for this is to support dynamic display names for model properties,
    /// e.g. display names that depend on the route parameters, or the database configuration 
    /// which is a common use case in this application
    /// </summary>
    public class DynamicModelMetadataProvider : DefaultModelMetadataProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITenantUserInfoAccessor _tenantInfo;
        private readonly IStringLocalizer<DynamicModelMetadataProvider> _localizer;

        public DynamicModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IHttpContextAccessor contextAccessor,
            ITenantUserInfoAccessor tenantInfo, IStringLocalizer<DynamicModelMetadataProvider> localizer) : base(detailsProvider)
        {
            _contextAccessor = contextAccessor;
            _tenantInfo = tenantInfo;
            _localizer = localizer;
        }

        public DynamicModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor,
            IHttpContextAccessor contextAccessor, ITenantUserInfoAccessor tenantInfo, IStringLocalizer<DynamicModelMetadataProvider> localizer) : base(detailsProvider, optionsAccessor)
        {
            _contextAccessor = contextAccessor;
            _tenantInfo = tenantInfo;
            _localizer = localizer;
        }

        protected override DefaultMetadataDetails[] CreatePropertyDetails(ModelMetadataIdentity key)
        {
            // Call the base implementation
            var propsDetails = base.CreatePropertyDetails(key);

            foreach (var propDetails in propsDetails)
            {
                var att = propDetails.ModelAttributes.PropertyAttributes
                    .OfType<MultilingualDisplayAttribute>().FirstOrDefault();

                if (att != null)
                {
                    propDetails.DisplayMetadata = new DisplayMetadata
                    {
                        DisplayName = () =>
                        {
                            var name = att.Name ?? "";
                            var lang = att.Language;
                            var info = _tenantInfo.GetCurrentInfo();
                            string result;

                            switch (lang)
                            {
                                case Language.Primary:
                                    result = _localizer[name] + PrimaryPostfix(info);
                                    break;

                                case Language.Secondary:
                                    // A null name indicates a hidden column in Excel templates
                                    result = string.IsNullOrWhiteSpace(info.SecondaryLanguageId) ?
                                    Constants.Hidden : _localizer[name] + SecondaryPostfix(info);
                                    break;

                                case Language.Ternary:
                                    // A null name indicates a hidden column in Excel templates
                                    result = string.IsNullOrWhiteSpace(info.TernaryLanguageId) ?
                                    Constants.Hidden : _localizer[name] + TernaryPostfix(info);
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

            ///// Below are types which have dynamic properties
            if (IsSameOrSubclass<AgentForSave>(key.ModelType))
            {
                // Get the route data from http context
                // Loop over the properties and special treatment to the dynamic ones
                foreach (var propDetails in propsDetails)
                {
                    string propertyName = propDetails.Key.Name;
                    if (propertyName == nameof(AgentForSave.BirthDateTime))
                    {
                        propDetails.DisplayMetadata = new DisplayMetadata
                        {
                            DisplayName = () =>
                            {
                                var routeData = _contextAccessor.HttpContext.GetRouteData();
                                string agentType = routeData.Values["agentType"]?.ToString();
                                string displayName = $"Agent_{agentType}_BirthDateTime";
                                return _localizer[displayName];
                            }
                        };
                    }
                }
            }

            return propsDetails;
        }

        private string PrimaryPostfix(TenantUserInfo info)
        {
            if (info != null && info.SecondaryLanguageId != null && info.TernaryLanguageId != null)
            {
                return $" ({info.PrimaryLanguageSymbol})";
            }

            return "";
        }


        private string SecondaryPostfix(TenantUserInfo info)
        {
            if (info != null && info.SecondaryLanguageId != null)
            {
                return $" ({info.SecondaryLanguageSymbol})";
            }

            return "";
        }
        private string TernaryPostfix(TenantUserInfo info)
        {
            if (info != null && info.TernaryLanguageId != null)
            {
                return $" ({info.TernaryLanguageSymbol})";
            }

            return "";
        }

        public bool IsSameOrSubclass<TBase>(Type potentialDescendant)
        {
            var potentialBase = typeof(TBase);
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }

        // TODO: Delete
        public bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }
    }
}
