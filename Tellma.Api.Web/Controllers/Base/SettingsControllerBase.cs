using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public abstract class SettingsControllerBase<TSettingsForSave, TSettings> : ControllerBase
        where TSettings : Entity
        where TSettingsForSave : Entity
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<SettingsControllerBase<TSettingsForSave, TSettings>> _logger;

        public SettingsControllerBase(IServiceProvider sp)
        {
            _sp = sp;
            _logger = _sp.GetRequiredService<ILogger<SettingsControllerBase<TSettingsForSave, TSettings>>>();
        }

        [HttpGet]
        public virtual async Task<ActionResult<GetEntityResponse<TSettings>>> GetSettings([FromQuery] SelectExpandArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
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
            },
            _logger);
        }

        [HttpPost]
        public async Task<ActionResult<SaveSettingsResponse<TSettings>>> Save([FromBody] TSettingsForSave settingsForSave, [FromQuery] SaveArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
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
            },
            _logger);
        }

        protected abstract ApplicationSettingsServiceBase<TSettingsForSave, TSettings> GetSettingsService();
    }
}
