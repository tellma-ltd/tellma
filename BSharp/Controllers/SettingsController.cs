using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.GlobalSettings;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/settings")]
    [ApiController]
    [AuthorizeAccess]
    [LoadTenantInfo]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class SettingsController : ControllerBase
    {
        // Private fields

        private readonly ApplicationContext _db;
        private readonly ILogger<SettingsController> _logger;
        private readonly IGlobalSettingsCache _globalSettingsCache;
        private readonly IStringLocalizer<SettingsController> _localizer;
        private readonly IMapper _mapper;
        private readonly ITenantUserInfoAccessor _tenantInfo;


        // Constructor

        public SettingsController(ApplicationContext db, ILogger<SettingsController> logger, IGlobalSettingsCache globalSettingsCache,
            IStringLocalizer<SettingsController> localizer, IMapper mapper, ITenantUserInfoAccessor tenantInfo)
        {
            _db = db;
            _logger = logger;
            _globalSettingsCache = globalSettingsCache;
            _localizer = localizer;
            _mapper = mapper;
            _tenantInfo = tenantInfo;
        }


        // API

        [HttpGet]
        public async Task<ActionResult<GetByIdResponse<Settings>>> Get([FromQuery] GetByIdArguments args)
        {
            // Authorized access (Criteria are not supported here)
            var readPermissions = await ControllerUtilities.GetPermissions(_db.AbstractPermissions, PermissionLevel.Read, "settings");
            if (!readPermissions.Any())
            {
                return StatusCode(403);
            }

            try
            {
                return await GetImpl(args);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<SaveSettingsResponse>> Save([FromBody] SettingsForSave settingsForSave, [FromQuery] SaveArguments args)
        {
            // Authorized access (Criteria are not supported here)
            var updatePermissions = await ControllerUtilities.GetPermissions(_db.AbstractPermissions, PermissionLevel.Update, "settings");
            if (!updatePermissions.Any())
            {
                return StatusCode(403);
            }

            try
            {
                // Trim all string fields just in case
                settingsForSave.TrimStringProperties();

                // Validate
                ValidateAndPreprocessSettings(settingsForSave);

                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(ModelState);
                }

                // Persist
                M.Settings mSettings = await _db.Settings.FirstOrDefaultAsync();
                if (mSettings == null)
                {
                    // This should never happen
                    return BadRequest("Settings have not been initialized");
                }

                _mapper.Map(settingsForSave, mSettings);

                mSettings.ModifiedAt = DateTimeOffset.Now;
                mSettings.ModifiedById = _tenantInfo.GetCurrentInfo().UserId.Value;
                mSettings.SettingsVersion = Guid.NewGuid(); // prompts clients to refresh

                await _db.SaveChangesAsync();

                // If requested, return the updated entity
                if (args.ReturnEntities ?? false)
                {
                    // If requested, return the same response you would get from a GET
                    var res = await GetImpl(new GetByIdArguments { Expand = args.Expand });
                    var result = new SaveSettingsResponse
                    {
                        CollectionName = res.CollectionName,
                        Entity = res.Entity,
                        RelatedEntities = res.RelatedEntities,
                        SettingsForClient = await GetForClientImpl()
                    };

                    return result;
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("client")]
        public async Task<ActionResult<DataWithVersion<SettingsForClient>>> GetForClient()
        {
            try
            {
                var result = await GetForClientImpl();
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            // If all you want is to check whether the cached versions of settings and permissions 
            // are fresh you can use this API that only does that through the [LoadTenantInfo] filter

            return Ok();
        }


        // Helper methods

        private async Task<GetByIdResponse<Settings>> GetImpl(GetByIdArguments args)
        {
            M.Settings mSettings = await _db.Settings.FirstOrDefaultAsync();
            if (mSettings == null)
            {
                // This should never happen
                throw new BadRequestException("Settings have not been initialized");
            }

            var settings = _mapper.Map<Settings>(mSettings);
            var result = new GetByIdResponse<Settings>
            {
                CollectionName = "Settings",
                Entity = settings,
            };

            if (!string.IsNullOrWhiteSpace(args.Expand))
            {
                Expand(args.Expand, settings, result);
            }

            return result;
        }

        private async Task<DataWithVersion<SettingsForClient>> GetForClientImpl()
        {
            M.Settings mSettings = await _db.Settings.FirstOrDefaultAsync();
            if (mSettings == null)
            {
                // This should never happen
                throw new BadRequestException("Settings have not been initialized");
            }

            // Prepare the settings for client
            var settings = _mapper.Map<SettingsForClient>(mSettings);
            var activeCulures = _globalSettingsCache.GetGlobalSettings().Data.ActiveCultures;

            settings.PrimaryLanguageName = GetCultureDisplayName(settings.PrimaryLanguageId);
            settings.SecondaryLanguageName = GetCultureDisplayName(settings.SecondaryLanguageId);

            // Tag the settings for client with their current version
            var result = new DataWithVersion<SettingsForClient>
            {
                Version = mSettings.SettingsVersion.ToString(),
                Data = settings
            };

            return result;
        }

        private string GetCultureDisplayName(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return null;
            }

            var activeCulures = _globalSettingsCache.GetGlobalSettings().Data.ActiveCultures;
            activeCulures.TryGetValue(cultureName, out Culture culture);

            if (culture != null)
            {
                return culture.Name;
            }
            else
            {
                try
                {
                    var c = new CultureInfo(cultureName);
                    return c.NativeName;
                }
                catch
                {
                    return null;
                }
            }
        }

        private void Expand(string expand, Settings settings, GetByIdResponse<Settings> result)
        {

        }

        private void ValidateAndPreprocessSettings(SettingsForSave entity)
        {
            var activeCulures = _globalSettingsCache.GetGlobalSettings().Data.ActiveCultures;
            {
                if (!activeCulures.ContainsKey(entity.PrimaryLanguageId))
                {
                    ModelState.AddModelError(nameof(entity.PrimaryLanguageId),
                        _localizer["Error_InvalidLanguageId0", entity.PrimaryLanguageId]);
                }
            }

            if (!string.IsNullOrWhiteSpace(entity.SecondaryLanguageId))
            {
                if (!activeCulures.ContainsKey(entity.SecondaryLanguageId))
                {
                    ModelState.AddModelError(nameof(entity.PrimaryLanguageId),
                        _localizer["Error_InvalidLanguageId0", entity.SecondaryLanguageId]);
                }
            }

            if (string.IsNullOrWhiteSpace(entity.SecondaryLanguageId))
            {
                entity.SecondaryLanguageSymbol = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(entity.PrimaryLanguageSymbol))
                {
                    ModelState.AddModelError(nameof(entity.PrimaryLanguageSymbol),
                        _localizer[nameof(RequiredAttribute), _localizer["Settings_PrimaryLanguageSymbol"]]);
                }

                if (string.IsNullOrWhiteSpace(entity.SecondaryLanguageSymbol))
                {
                    ModelState.AddModelError(nameof(entity.SecondaryLanguageSymbol),
                        _localizer[nameof(RequiredAttribute), _localizer["Settings_SecondaryLanguageSymbol"]]);
                }

                if (entity.SecondaryLanguageId == entity.PrimaryLanguageId)
                {

                    ModelState.AddModelError(nameof(entity.SecondaryLanguageId),
                        _localizer["Error_SecondaryLanguageCannotBeTheSameAsPrimaryLanguage"]);
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
    }
}
