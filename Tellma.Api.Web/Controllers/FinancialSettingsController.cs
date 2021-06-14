using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/financial-settings")]
    public class FinancialSettingsController : SettingsControllerBase<FinancialSettingsForSave, FinancialSettings>
    {
        private readonly FinancialSettingsService _service;
        private readonly ILogger<FinancialSettingsController> _logger;
        private readonly ISettingsCache _settingsCache;

        public FinancialSettingsController(IServiceProvider sp, FinancialSettingsService service, ILogger<FinancialSettingsController> logger, ISettingsCache settingsCache) : base(sp)
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
                _logger.LogError(ex, $"Error caught in {nameof(FinancialSettingsController)}.{nameof(SettingsForClient)}: {ex.Message}");
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

        protected override SettingsServiceBase<FinancialSettingsForSave, FinancialSettings> GetSettingsService()
        {
            return _service;
        }
    }

    public class FinancialSettingsService : SettingsServiceBase<FinancialSettingsForSave, FinancialSettings>
    {
        private readonly IStringLocalizer _localizer;
        private readonly MetadataProvider _metadataPrvider;

        public FinancialSettingsService(IServiceProvider sp,
            IStringLocalizer<Strings> localizer,
            MetadataProvider metadataPrvider): base(sp)
        {
            _localizer = localizer;
            _metadataPrvider = metadataPrvider;
        }

        protected override async Task<FinancialSettings> GetExecute(SelectExpandArguments args, CancellationToken cancellation)
        {
            var settings = await _repo.FinancialSettings
                .Select(args.Select)
                .Expand(args.Expand)
                .OrderBy("FinancialModifiedById")
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
            return await _repo.PermissionsFromCache("financial-settings", action, cancellation);
        }

        protected override async Task SaveValidate(FinancialSettingsForSave entity)
        {
            // Attribute Validation
            var meta = _metadataPrvider.GetMetadata(_tenantIdAccessor.GetTenantId(), typeof(FinancialSettingsForSave));
            ValidateEntity(entity, meta);

            // Make sure the archive date is not in the future
            if (entity.ArchiveDate != null && entity.ArchiveDate.Value > DateTime.Today.AddDays(1))
            {
                ModelState.AddModelError(nameof(entity.ArchiveDate),
                    _localizer["Error_DateCannotBeInTheFuture"]);
            }

            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.FinancialSettings_Validate__Save(entity, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);

            return;
        }

        protected override async Task SaveExecute(FinancialSettingsForSave settingsForSave, SelectExpandArguments args)
        {
            // Persist
            await _repo.FinancialSettings__Save(settingsForSave);
        }
    }
}
