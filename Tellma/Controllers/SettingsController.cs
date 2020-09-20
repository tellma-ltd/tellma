using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/settings")]
    [AuthorizeJwtBearer]
    [ApplicationController(allowUnobtrusive: true)]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsService _service;
        private readonly ILogger<SettingsController> _logger;
        private readonly ISettingsCache _settingsCache;

        public SettingsController(SettingsService service,
            ILogger<SettingsController> logger,
            ISettingsCache settingsCache)
        {
            _service = service;
            _logger = logger;
            _settingsCache = settingsCache;
        }

        [HttpGet]
        public async Task<ActionResult<GetEntityResponse<Settings>>> Get([FromQuery] SelectExpandArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var settings = await _service.Get(args, cancellation);

                var singleton = new Settings[] { settings };
                var relatedEntities = ControllerUtilities.FlattenAndTrim(singleton, cancellation: default);

                var result = new GetEntityResponse<Settings>
                {
                    Result = settings,
                    RelatedEntities = relatedEntities
                };

                return Ok(result);
            },
            _logger);
        }

        [HttpPost]
        public async Task<ActionResult<SaveSettingsResponse>> Save([FromBody] SettingsForSave settingsForSave, [FromQuery] SaveArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (settings, settingsForClient) = await _service.Save(settingsForSave, args);

                var singleton = new Settings[] { settings };
                var relatedEntities = ControllerUtilities.FlattenAndTrim(singleton, cancellation: default);

                var result = new SaveSettingsResponse
                {
                    Result = settings,
                    RelatedEntities = relatedEntities,
                    SettingsForClient = settingsForClient
                };

                return Ok(result);
            },
            _logger);

            //// Authorized access (Criteria are not supported here)
            //var updatePermissions = await _repo.UserPermissions(Constants.Update, "settings", cancellation: default);
            //if (!updatePermissions.Any())
            //{
            //    return StatusCode(403);
            //}

            //try
            //{
            //    // Trim all string fields just in case
            //    settingsForSave.TrimStringProperties();

            //    // Validate
            //    ValidateAndPreprocessSettings(settingsForSave);

            //    if (!ModelState.IsValid)
            //    {
            //        return UnprocessableEntity(ModelState);
            //    }

            //    // Persist
            //    await _repo.Settings__Save(settingsForSave);

            //    // Update the settings cache
            //    var tenantId = _tenantIdAccessor.GetTenantId();
            //    var settingsForClient = await LoadSettingsForClient(_repo, cancellation: default);
            //    _settingsCache.SetSettings(tenantId, settingsForClient);

            //    // If requested, return the updated entity
            //    if (args.ReturnEntities ?? false)
            //    {
            //        // If requested, return the same response you would get from a GET
            //        var res = await GetImpl(new GetByIdArguments { Expand = args.Expand }, cancellation: default);
            //        var result = new SaveSettingsResponse
            //        {
            //            Entities = res.Entities,
            //            Result = res.Result,
            //            SettingsForClient = settingsForClient
            //        };

            //        return result;
            //    }
            //    else
            //    {
            //        return Ok();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
            //    return BadRequest(ex.Message);
            //}
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
                _logger.LogError(ex, $"Error caught in {nameof(SettingsController)}.{nameof(SettingsForClient)}: {ex.Message}");
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
    }

    public class SettingsService : ServiceBase
    {
        private readonly ApplicationRepository _repo;
        private readonly IStringLocalizer _localizer;
        private readonly ISettingsCache _settingsCache;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly MetadataProvider _metadataPrvider;

        public SettingsService(ApplicationRepository repo,
            IStringLocalizer<Strings> localizer,
            ISettingsCache settingsCache,
            ITenantIdAccessor tenantIdAccessor,
            MetadataProvider metadataPrvider)
        {
            _repo = repo;
            _localizer = localizer;
            _settingsCache = settingsCache;
            _tenantIdAccessor = tenantIdAccessor;
            _metadataPrvider = metadataPrvider;
        }

        public async Task<Settings> Get(SelectExpandArguments args, CancellationToken cancellation)
        {
            // Authorized access (Criteria are not supported here)

            var readPermissions = await _repo.PermissionsFromCache("settings", Constants.Read, cancellation);
            if (!readPermissions.Any())
            {
                throw new ForbiddenException();
            }

            var settings = await _repo.Settings
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

        public async Task<(Settings, Versioned<SettingsForClient>)> Save(SettingsForSave settingsForSave, SaveArguments args)
        {
            // Authorized access (Criteria are not supported here)
            var updatePermissions = await _repo.PermissionsFromCache("settings", Constants.Update, cancellation: default);
            if (!updatePermissions.Any())
            {
                throw new ForbiddenException();
            }
            // Trim all string fields just in case
            settingsForSave.TrimStringProperties();

            // Validate
            ValidateAndPreprocessSettings(settingsForSave);
            ModelState.ThrowIfInvalid();

            // Persist
            await _repo.Settings__Save(settingsForSave);

            // Update the settings cache
            var tenantId = _tenantIdAccessor.GetTenantId();
            var settingsForClient = await LoadSettingsForClient(_repo, cancellation: default);
            _settingsCache.SetSettings(tenantId, settingsForClient);

            // If requested, return the updated entity
            if (args.ReturnEntities ?? false)
            {
                // If requested, return the same response you would get from a GET
                var res = await Get(args, cancellation: default);
                return (res, settingsForClient);
            }
            else
            {
                return default;
            }
        }

        private void ValidateAndPreprocessSettings(SettingsForSave entity)
        {
            // Basic Validation
            var meta = _metadataPrvider.GetMetadata(_tenantIdAccessor.GetTenantId(), typeof(SettingsForSave));
            ValidateEntity(entity, meta);

            // Sophisticated validation
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

            // Make sure the color is a valid HTML color
            // Credit: https://bit.ly/2ToV6x4
            if (!string.IsNullOrWhiteSpace(entity.BrandColor) && !Regex.IsMatch(entity.BrandColor, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
            {
                ModelState.AddModelError(nameof(entity.BrandColor),
                    _localizer["Error_TheField0MustBeAValidColorFormat", _localizer["Settings_BrandColor"]]);
            }
        }

        public static async Task<Versioned<SettingsForClient>> LoadSettingsForClient(ApplicationRepository repo, CancellationToken cancellation)
        {
            var (isMultiSegment, settings) = await repo.Settings__Load(cancellation);
            if (settings == null)
            {
                // This should never happen
                throw new BadRequestException("Settings have not been initialized");
            }

            // Prepare the settings for client
            SettingsForClient settingsForClient = new SettingsForClient();
            foreach (var forClientProp in typeof(SettingsForClient).GetProperties())
            {
                var settingsProp = typeof(Settings).GetProperty(forClientProp.Name);
                if (settingsProp != null)
                {
                    var value = settingsProp.GetValue(settings);
                    forClientProp.SetValue(settingsForClient, value);
                }
            }

            // Is Multi Center/Segment
            settingsForClient.IsMultiSegment = isMultiSegment;

            // Functional currency
            settingsForClient.FunctionalCurrencyDecimals = settings.FunctionalCurrency.E ?? 0;
            settingsForClient.FunctionalCurrencyName = settings.FunctionalCurrency.Name;
            settingsForClient.FunctionalCurrencyName2 = settings.FunctionalCurrency.Name2;
            settingsForClient.FunctionalCurrencyName3 = settings.FunctionalCurrency.Name3;
            settingsForClient.FunctionalCurrencyDescription = settings.FunctionalCurrency.Description;
            settingsForClient.FunctionalCurrencyDescription2 = settings.FunctionalCurrency.Description2;
            settingsForClient.FunctionalCurrencyDescription3 = settings.FunctionalCurrency.Description3;

            // Language
            settingsForClient.PrimaryLanguageName = GetCultureDisplayName(settingsForClient.PrimaryLanguageId);
            settingsForClient.SecondaryLanguageName = GetCultureDisplayName(settingsForClient.SecondaryLanguageId);
            settingsForClient.TernaryLanguageName = GetCultureDisplayName(settingsForClient.TernaryLanguageId);

            // Tag the settings for client with their current version
            var result = new Versioned<SettingsForClient>
            (
                version: settings.SettingsVersion.ToString(),
                data: settingsForClient
            );

            return result;
        }

        private static string GetCultureDisplayName(string cultureName)
        {
            if (cultureName is null)
            {
                return null;
            }

            return System.Globalization.CultureInfo.GetCultureInfo(cultureName)?.NativeName;
        }
    }
}
