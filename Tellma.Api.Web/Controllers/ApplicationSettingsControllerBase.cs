using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Common;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using OData-like parameters.
    /// </summary>
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class ApplicationSettingsControllerBase<TSettingsForSave, TSettings> : ControllerBase
        where TSettings : Entity
        where TSettingsForSave : Entity
    {
        public ApplicationSettingsControllerBase(IServiceProvider _)
        {
        }

        [HttpGet]
        public virtual async Task<ActionResult<GetEntityResponse<TSettings>>> GetSettings([FromQuery] SelectExpandArguments args, CancellationToken cancellation)
        {
            var _service = GetSettingsService();
            var settings = await _service.GetSettings(args, cancellation);

            var singleton = new TSettings[] { settings };
            var relatedEntities = ControllerUtilities.FlattenAndTrim(singleton, cancellation: default);

            var result = new GetEntityResponse<TSettings>
            {
                Result = settings,
                RelatedEntities = relatedEntities
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<SaveSettingsResponse<TSettings>>> Save([FromBody] TSettingsForSave settingsForSave, [FromQuery] SaveArguments args)
        {
            var _service = GetSettingsService();
            var (settings, settingsForClient) = await _service.SaveSettings(settingsForSave, args);

            var singleton = new TSettings[] { settings };
            var relatedEntities = ControllerUtilities.FlattenAndTrim(singleton, cancellation: default);

            var result = new SaveSettingsResponse<TSettings>
            {
                Result = settings,
                RelatedEntities = relatedEntities,
                SettingsForClient = settingsForClient
            };

            return Ok(result);
        }

        protected abstract ApplicationSettingsServiceBase<TSettingsForSave, TSettings> GetSettingsService();
    }
}
