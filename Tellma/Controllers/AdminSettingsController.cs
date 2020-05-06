using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.ImportExport;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Controllers.Utilities;

namespace Tellma.Controllers
{
    [Route("api/admin-settings")]
    [AuthorizeAccess]
    [AdminController]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminSettingsController : ControllerBase
    {
        private readonly AdminSettingsService _service;

        // Private fields

        private readonly ILogger<SettingsController> _logger;

        public AdminSettingsController(AdminSettingsService service, ILogger<SettingsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // API

        #region TODO

        //[HttpGet]
        //public async Task<ActionResult<GetEntityResponse<Settings>>> Get([FromQuery] GetByIdArguments args)
        //{
        //    // Authorized access (Criteria are not supported here)
        //    var readPermissions = await _repo.UserPermissions(Constants.Read, "settings");
        //    if (!readPermissions.Any())
        //    {
        //        return StatusCode(403);
        //    }
        //    try
        //    {
        //        return await GetImpl(args);
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        return Ok();
        //    }
        //    catch (BadRequestException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult<SaveSettingsResponse>> Save([FromBody] SettingsForSave settingsForSave, [FromQuery] SaveArguments args)
        //{
        //    // Authorized access (Criteria are not supported here)
        //    var updatePermissions = await _repo.UserPermissions(Constants.Update, "settings");
        //    if (!updatePermissions.Any())
        //    {
        //        return StatusCode(403);
        //    }

        //    try
        //    {
        //        // Trim all string fields just in case
        //        settingsForSave.TrimStringProperties();

        //        // Validate
        //        ValidateAndPreprocessSettings(settingsForSave);

        //        if (!ModelState.IsValid)
        //        {
        //            return UnprocessableEntity(ModelState);
        //        }

        //        // Persist
        //        await _repo.Settings__Save(settingsForSave);

        //        // Update the settings cache
        //        var tenantId = _tenantIdAccessor.GetTenantId();
        //        var settingsForClient = await LoadSettingsForClient(_repo);
        //        _settingsCache.SetSettings(tenantId, settingsForClient);

        //        // If requested, return the updated entity
        //        if (args.ReturnEntities ?? false)
        //        {
        //            // If requested, return the same response you would get from a GET
        //            var res = await GetImpl(new GetByIdArguments { Expand = args.Expand });
        //            var result = new SaveSettingsResponse
        //            {
        //                Entities = res.Entities,
        //                Result = res.Result,
        //                SettingsForClient = settingsForClient
        //            };

        //            return result;
        //        }
        //        else
        //        {
        //            return Ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
        //        return BadRequest(ex.Message);
        //    }
        //}

        #endregion

        [HttpGet("client")]
        public async Task<ActionResult<DataWithVersion<AdminSettingsForClient>>> SettingsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Simply retrieves the cached settings, which were refreshed by AdminApiAttribute
                var result = await _service.SettingsForClient(cancellation);
                return Ok(result);
            }, 
            _logger);
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            // If all you want is to check whether the cached versions of settings and permissions 
            // are fresh you can use this API that only does that through the [AdminApi] filter

            return Ok();
        }

        // Helper methods

        //private async Task<GetEntityResponse<Settings>> GetImpl(GetByIdArguments args)
        //{
        //    var settings = await _repo.Settings
        //        .Select(args.Select)
        //        .Expand(args.Expand)
        //        .OrderBy("PrimaryLanguageId")
        //        .FirstOrDefaultAsync();

        //    if (settings == null)
        //    {
        //        // Programmer mistake
        //        throw new BadRequestException("Settings have not been initialized");
        //    }

        //    var result = new GetEntityResponse<Settings>
        //    {
        //        Result = settings,
        //    };

        //    return result;
        //}

        //private void ValidateAndPreprocessSettings(SettingsForSave entity)
        //{
        //    if (!string.IsNullOrWhiteSpace(entity.SecondaryLanguageId) || !string.IsNullOrWhiteSpace(entity.TernaryLanguageId))
        //    {
        //        if (string.IsNullOrWhiteSpace(entity.PrimaryLanguageSymbol))
        //        {
        //            ModelState.AddModelError(nameof(entity.PrimaryLanguageSymbol),
        //                _localizer[Services.Utilities.Constants.Error_RequiredField0, _localizer["Settings_PrimaryLanguageSymbol"]]);
        //        }
        //    }

        //    if (string.IsNullOrWhiteSpace(entity.SecondaryLanguageId))
        //    {
        //        entity.SecondaryLanguageSymbol = null;
        //    }
        //    else
        //    {
        //        if (string.IsNullOrWhiteSpace(entity.SecondaryLanguageSymbol))
        //        {
        //            ModelState.AddModelError(nameof(entity.SecondaryLanguageSymbol),
        //                _localizer[Services.Utilities.Constants.Error_RequiredField0, _localizer["Settings_SecondaryLanguageSymbol"]]);
        //        }

        //        if (entity.SecondaryLanguageId == entity.PrimaryLanguageId)
        //        {
        //            ModelState.AddModelError(nameof(entity.SecondaryLanguageId),
        //                _localizer["Error_SecondaryLanguageCannotBeTheSameAsPrimaryLanguage"]);
        //        }
        //    }

        //    if (string.IsNullOrWhiteSpace(entity.TernaryLanguageId))
        //    {
        //        entity.TernaryLanguageSymbol = null;
        //    }
        //    else
        //    {
        //        if (string.IsNullOrWhiteSpace(entity.TernaryLanguageSymbol))
        //        {
        //            ModelState.AddModelError(nameof(entity.TernaryLanguageSymbol),
        //                _localizer[Services.Utilities.Constants.Error_RequiredField0, _localizer["Settings_TernaryLanguageSymbol"]]);
        //        }

        //        if (entity.TernaryLanguageId == entity.PrimaryLanguageId)
        //        {
        //            ModelState.AddModelError(nameof(entity.TernaryLanguageId),
        //                _localizer["Error_TernaryLanguageCannotBeTheSameAsPrimaryLanguage"]);
        //        }

        //        if (entity.TernaryLanguageId == entity.SecondaryLanguageId)
        //        {
        //            ModelState.AddModelError(nameof(entity.TernaryLanguageId),
        //                _localizer["Error_TernaryLanguageCannotBeTheSameAsSecondaryLanguage"]);
        //        }
        //    }

        //    // Make sure the color is a valid HTML color
        //    // Credit: https://bit.ly/2ToV6x4
        //    if (!string.IsNullOrWhiteSpace(entity.BrandColor) && !Regex.IsMatch(entity.BrandColor, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
        //    {
        //        ModelState.AddModelError(nameof(entity.BrandColor),
        //            _localizer["Error_TheField0MustBeAValidColorFormat", _localizer["Settings_BrandColor"]]);
        //    }
        //}

        public static async Task<DataWithVersion<AdminSettingsForClient>> LoadSettingsForClient(AdminRepository repo, CancellationToken cancellation)
        {
            var settings = await repo.Settings__Load(cancellation);
            if (settings == null)
            {
                // This should never happen
                throw new BadRequestException("AdminSettings have not been initialized");
            }

            // Prepare the settings for client
            AdminSettingsForClient settingsForClient = new AdminSettingsForClient();
            foreach (var forClientProp in typeof(AdminSettingsForClient).GetProperties())
            {
                var settingsProp = typeof(AdminSettings).GetProperty(forClientProp.Name);
                if (settingsProp != null)
                {
                    var value = settingsProp.GetValue(settings);
                    forClientProp.SetValue(settingsForClient, value);
                }
            }

            // Tag the settings for client with their current version
            var result = new DataWithVersion<AdminSettingsForClient>
            {
                Version = settings.SettingsVersion.ToString(),
                Data = settingsForClient
            };

            return result;
        }
    }

    public class AdminSettingsService : ServiceBase
    {
        // Private fields

        private readonly AdminRepository _repo;
        private readonly IStringLocalizer _localizer;

        public AdminSettingsService(AdminRepository repo,
            IStringLocalizer<Strings> localizer)
        {
            _repo = repo;
            _localizer = localizer;
        }

        public async Task<DataWithVersion<AdminSettingsForClient>> SettingsForClient(CancellationToken cancellation)
        {
            // Simply retrieves the cached settings, which were refreshed by AdminApiAttribute
            var adminSettings = await _repo.Settings__Load(cancellation);
            if (adminSettings == null)
            {
                throw new BadRequestException("Admin Settings were not initialized");
            }

            var adminSettingsForClient = new AdminSettingsForClient
            {
                CreatedAt = adminSettings.CreatedAt
            };

            var result = new DataWithVersion<AdminSettingsForClient>
            {
                Data = adminSettingsForClient,
                Version = adminSettings.SettingsVersion.ToString()
            };

            return result;
        }
    }
}
