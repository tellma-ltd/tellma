using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.Identity;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/settings")]
    [ApiController]
    [LoadTenantInfo]
    public class SettingsController : ControllerBase
    {
        // Private fields

        private readonly ApplicationContext _db;
        private readonly ILogger<SettingsController> _logger;
        private readonly IStringLocalizer<SettingsController> _localizer;
        private readonly IMapper _mapper;
        private readonly ITenantUserInfoAccessor _tenantInfo;
        private readonly CulturesRepository _culturesRepo;


        // Constructor

        public SettingsController(ApplicationContext db, ILogger<SettingsController> logger,
            IStringLocalizer<SettingsController> localizer, IMapper mapper, ITenantUserInfoAccessor tenantInfo)
        {
            _db = db;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _tenantInfo = tenantInfo;
            _culturesRepo = new CulturesRepository();
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
                M.Settings mSettings = await _db.Settings.FirstOrDefaultAsync();
                if (mSettings == null)
                {
                    // This should never happen
                    return BadRequest("Settings have not been initialized");
                }

                var settings = _mapper.Map<Settings>(mSettings);
                var result = new GetByIdResponse<Settings>
                {
                    CollectionName = "Settings",
                    Entity = settings,
                };

                if(!string.IsNullOrWhiteSpace(args.Expand))
                {
                    Expand(args.Expand, settings, result);
                }

                return result;
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
                M.Settings mSettings = await _db.Settings.FirstOrDefaultAsync();
                if (mSettings == null)
                {
                    // This should never happen
                    return BadRequest("Settings have not been initialized");
                }

                // Prepare the settings for client
                var settings = _mapper.Map<SettingsForClient>(mSettings);
                settings.PrimaryLanguageName = _culturesRepo.GetCulture(settings.PrimaryLanguageId)?.Name;
                settings.SecondaryLanguageName = _culturesRepo.GetCulture(settings.SecondaryLanguageId)?.Name;
                settings.UserId = _tenantInfo.UserId();

                // Tag the settings for client with their current version
                var result = new DataWithVersion<SettingsForClient>
                {
                    Version = mSettings.SettingsVersion.ToString(),
                    Data = settings
                };

                // Return the result
                return Ok(result);
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

        [HttpPost]
        public async Task<ActionResult<GetByIdResponse<Settings>>> Save([FromBody] SettingsForSave settingsForSave, [FromQuery] SaveArguments args)
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
                mSettings.SettingsVersion = Guid.NewGuid(); // promps clients to refresh

                await _db.SaveChangesAsync();

                // If requested, return the updated entity
                if (args.ReturnEntities ?? false)
                {
                    // If requested, return the same response you would get from a GET
                    return await Get(new GetByIdArguments { Expand = args.Expand });
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


        // Helper methods

        private void Expand(string expand, Settings settings, GetByIdResponse<Settings> result)
        {

            // Add related entities
            if (expand != null)
            {
                var cultures = new List<Culture>();
                if (expand.Contains("PrimaryLanguage"))
                {
                    if (!string.IsNullOrWhiteSpace(settings.PrimaryLanguageId))
                    {
                        var cultureDef = _culturesRepo.GetCulture(settings.PrimaryLanguageId);
                        var culture = _mapper.Map<Culture>(cultureDef);
                        cultures.Add(culture);
                    }
                }

                if (expand.Contains("SecondaryLanguage"))
                {
                    if (!string.IsNullOrWhiteSpace(settings.SecondaryLanguageId))
                    {
                        var cultureDef = _culturesRepo.GetCulture(settings.SecondaryLanguageId);
                        var culture = _mapper.Map<Culture>(cultureDef);
                        cultures.Add(culture);
                    }
                }

                if (cultures.Any())
                {
                    result.RelatedEntities = new Dictionary<string, IEnumerable<DtoBase>>
                    {
                        ["Cultures"] = cultures
                    };
                }
            }
        }

        private void ValidateAndPreprocessSettings(SettingsForSave entity)
        {
            {
                var culture = _culturesRepo.GetCulture(entity.PrimaryLanguageId);
                if (culture == null)
                {
                    ModelState.AddModelError(nameof(entity.PrimaryLanguageId),
                        _localizer["Error_InvalidLanguageId0", entity.PrimaryLanguageId]);
                }
            }

            if (!string.IsNullOrWhiteSpace(entity.SecondaryLanguageId))
            {

                var culture = _culturesRepo.GetCulture(entity.SecondaryLanguageId);
                if (culture == null)
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

                if(entity.SecondaryLanguageId == entity.PrimaryLanguageId)
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