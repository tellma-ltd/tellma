using BSharp.Controllers.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IStringLocalizer<DynamicModelMetadataProvider> _localizer;

        public DynamicModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IHttpContextAccessor contextAccessor, IStringLocalizer<DynamicModelMetadataProvider> localizer) : base(detailsProvider)
        {
            _contextAccessor = contextAccessor;
            _localizer = localizer;
        }

        public DynamicModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor, 
            IHttpContextAccessor contextAccessor, IStringLocalizer<DynamicModelMetadataProvider> localizer) : base(detailsProvider, optionsAccessor)
        {
            _contextAccessor = contextAccessor;
            _localizer = localizer;
        }

        protected override DefaultMetadataDetails[] CreatePropertyDetails(ModelMetadataIdentity key)
        {
            // Call the base implementation
            var propsDetails = base.CreatePropertyDetails(key);

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
                                string displayName = $"Agent_{agentType}_BirthDateTime"; // TODO Localize
                                return _localizer[displayName];
                            }
                        };
                    }
                }
            }

            return propsDetails;
        }

        public bool IsSameOrSubclass<TBase>(Type potentialDescendant)
        {
            var potentialBase = typeof(TBase);
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }
    }
}
