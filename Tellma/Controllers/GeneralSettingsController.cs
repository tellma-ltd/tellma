using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/general-settings")]
    public class GeneralSettingsController : SettingsControllerBase<GeneralSettingsForSave, GeneralSettings>
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

        protected override SettingsServiceBase<GeneralSettingsForSave, GeneralSettings> GetSettingsService()
        {
            return _service;
        }
    }

    public class GeneralSettingsService : SettingsServiceBase<GeneralSettingsForSave, GeneralSettings>
    {
        private readonly IStringLocalizer _localizer;
        private readonly MetadataProvider _metadataPrvider;

        public GeneralSettingsService(IServiceProvider sp,
            IStringLocalizer<Strings> localizer,
            MetadataProvider metadataPrvider): base(sp)
        {
            _localizer = localizer;
            _metadataPrvider = metadataPrvider;
        }

        protected override async Task<GeneralSettings> GetExecute(SelectExpandArguments args, CancellationToken cancellation)
        {
            var settings = await _repo.GeneralSettings
                .Select(args.Select)
                .Expand(args.Expand)
                .OrderBy("PrimaryLanguageId")
                .FirstOrDefaultAsync(cancellation);

            if (settings == null)
            {
                // Programmer mistake
                throw new InvalidOperationException("Bug: Settings have not been initialized");
            }

            return settings;
        }

        protected override ApplicationRepository GetRepository()
        {
            return _repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.PermissionsFromCache("general-settings", action, cancellation);
        }

        protected override async Task SaveValidate(GeneralSettingsForSave entity)
        {
            // Attribute Validation
            var meta = _metadataPrvider.GetMetadata(_tenantIdAccessor.GetTenantId(), typeof(GeneralSettingsForSave));
            ValidateEntity(entity, meta);

            // C# validation
            if (!string.IsNullOrWhiteSpace(entity.SecondaryLanguageId) || !string.IsNullOrWhiteSpace(entity.TernaryLanguageId))
            {
                if (string.IsNullOrWhiteSpace(entity.PrimaryLanguageSymbol))
                {
                    ModelState.AddModelError(nameof(entity.PrimaryLanguageSymbol),
                        _localizer[Constants.Error_Field0IsRequired, _localizer["Settings_PrimaryLanguageSymbol"]]);
                }
            }

            if (string.IsNullOrWhiteSpace(entity.SecondaryLanguageId))
            {
                entity.SecondaryLanguageSymbol = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(entity.SecondaryLanguageSymbol))
                {
                    ModelState.AddModelError(nameof(entity.SecondaryLanguageSymbol),
                        _localizer[Constants.Error_Field0IsRequired, _localizer["Settings_SecondaryLanguageSymbol"]]);
                }

                if (entity.SecondaryLanguageId == entity.PrimaryLanguageId)
                {
                    ModelState.AddModelError(nameof(entity.SecondaryLanguageId),
                        _localizer["Error_SecondaryLanguageCannotBeTheSameAsPrimaryLanguage"]);
                }
            }

            if (string.IsNullOrWhiteSpace(entity.TernaryLanguageId))
            {
                entity.TernaryLanguageSymbol = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(entity.TernaryLanguageSymbol))
                {
                    ModelState.AddModelError(nameof(entity.TernaryLanguageSymbol),
                        _localizer[Constants.Error_Field0IsRequired, _localizer["Settings_TernaryLanguageSymbol"]]);
                }

                if (entity.TernaryLanguageId == entity.PrimaryLanguageId)
                {
                    ModelState.AddModelError(nameof(entity.TernaryLanguageId),
                        _localizer["Error_TernaryLanguageCannotBeTheSameAsPrimaryLanguage"]);
                }

                if (entity.TernaryLanguageId == entity.SecondaryLanguageId)
                {
                    ModelState.AddModelError(nameof(entity.TernaryLanguageId),
                        _localizer["Error_TernaryLanguageCannotBeTheSameAsSecondaryLanguage"]);
                }
            }

            if (!string.IsNullOrWhiteSpace(entity.SecondaryCalendar))
            {
                if (entity.PrimaryCalendar == entity.SecondaryCalendar)
                {
                    ModelState.AddModelError(nameof(entity.SecondaryCalendar),
                        _localizer["Error_SecondaryCalendarCannotBeTheSameAsPrimaryCalendar"]);
                }
            }

            // Make sure the color is a valid HTML color
            // Credit: https://bit.ly/2ToV6x4
            if (!string.IsNullOrWhiteSpace(entity.BrandColor) && !Regex.IsMatch(entity.BrandColor, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
            {
                ModelState.AddModelError(nameof(entity.BrandColor),
                    _localizer["Error_TheField0MustBeAValidColorFormat", _localizer["Settings_BrandColor"]]);
            }

            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.GeneralSettings_Validate__Save(entity, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);

            return;
        }

        protected override async Task SaveExecute(GeneralSettingsForSave settingsForSave, SelectExpandArguments args)
        {
            // Persist
            await _repo.GeneralSettings__Save(settingsForSave);
        }
    }
}
