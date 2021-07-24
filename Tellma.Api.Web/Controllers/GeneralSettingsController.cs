using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Model.Application;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/general-settings")]
    public class GeneralSettingsController : ApplicationSettingsControllerBase<GeneralSettingsForSave, GeneralSettings>
    {
        private readonly GeneralSettingsService _service;
        private readonly ILogger<GeneralSettingsController> _logger;
        private readonly ISettingsCache _settingsCache;

        public GeneralSettingsController(IServiceProvider sp, GeneralSettingsService service, ILogger<GeneralSettingsController> logger, ISettingsCache settingsCache) : base(sp)
        {
            _service = service;
            _logger = logger;
            _settingsCache = settingsCache;
        }

        [HttpGet("client")]
        public ActionResult<Versioned<SettingsForClient>> SettingsForClient()
        {
            try
            {
                // Simply retrieves the cached settings, which were refreshed by ApplicationControllerAttribute
                var result = _settingsCache.GetCurrentSettingsIfCached();
                if (result == null)
                {
                    throw new InvalidOperationException("The settings were missing from the cache");
                }

                return Ok(result);
            }
            catch (TaskCanceledException)
            {
                return Ok();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error caught in {nameof(GeneralSettingsController)}.{nameof(SettingsForClient)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            // If all you want is to check whether the cached versions of settings and permissions 
            // are fresh you can use this API that only does that through the [ApplicationApi] filter

            return Ok();
        }

        protected override ApplicationSettingsServiceBase<GeneralSettingsForSave, GeneralSettings> GetSettingsService()
        {
            return _service;
        }
    }
}
