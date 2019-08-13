using AutoMapper;
using BSharp.Controllers.Dto;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.GlobalSettings;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/global-settings")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class GlobalSettingsController : ControllerBase
    {
        // Private fields

        private readonly AdminContext _db;
        private readonly ILogger<GlobalSettingsController> _logger;
        private readonly IStringLocalizer<GlobalSettingsController> _localizer;
        private readonly IGlobalSettingsCache _globalSettingsCache;
        private readonly IMapper _mapper;


        // Constructor

        public GlobalSettingsController(AdminContext db, ILogger<GlobalSettingsController> logger,
            IStringLocalizer<GlobalSettingsController> localizer, IMapper mapper, IGlobalSettingsCache globalSettingsCache)
        {
            _db = db;
            _logger = logger;
            _localizer = localizer;
            _globalSettingsCache = globalSettingsCache;
            _mapper = mapper;
        }


        // API

        [HttpGet]
        public async Task<ActionResult<GetByIdResponse<GlobalSettings>>> Get([FromQuery] GetByIdArguments args)
        {
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
        public async Task<ActionResult<SaveGlobalSettingsResponse>> Save([FromBody] GlobalSettingsForSave settingsForSave, [FromQuery] SaveArguments args)
        {
            // Authorized access (Criteria are not supported here)
            // TODO Authorize
            //var updatePermissions = await ControllerUtilities.GetPermissions(_db.AbstractPermissions, PermissionLevel.Update, "settings");
            //if (!updatePermissions.Any())
            //{
            //    return StatusCode(403);
            //}

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
                M.GlobalSettings mSettings = await _db.GlobalSettings.FirstOrDefaultAsync();
                if (mSettings == null)
                {
                    // This should never happen
                    return BadRequest("Global settings have not been initialized");
                }

                _mapper.Map(settingsForSave, mSettings);
                mSettings.SettingsVersion = Guid.NewGuid(); // prompts clients to refresh

                await _db.SaveChangesAsync();

                // IF requested, return the updated entity
                if (args.ReturnEntities ?? false)
                {
                    // IF requested, return the same response you would get from a GET
                    var res = await GetImpl(new GetByIdArguments { Expand = args.Expand });
                    var result = new SaveGlobalSettingsResponse
                    {
                        CollectionName = res.CollectionName,
                        Result = res.Result,
                        RelatedEntities = res.RelatedEntities,
                        SettingsForClient = GetForClientImpl()
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
        public ActionResult<DataWithVersion<GlobalSettingsForClient>> GetForClient()
        {
            try
            {
                var result = GetForClientImpl();
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
            // If all you want is to check whether the cached versions of global settings
            // are fresh you can use this API that only does that through the registered filter

            return Ok();
        }


        // Helper methods

        private async Task<GetByIdResponse<GlobalSettings>> GetImpl(GetByIdArguments args)
        {
            M.GlobalSettings mSettings = await _db.GlobalSettings.FirstOrDefaultAsync();
            if (mSettings == null)
            {
                // This should never happen
                throw new BadRequestException("Settings have not been initialized");
            }

            var settings = _mapper.Map<GlobalSettings>(mSettings);
            var result = new GetByIdResponse<GlobalSettings>
            {
                CollectionName = "Settings",
                Result = settings,
            };

            if (!string.IsNullOrWhiteSpace(args.Expand))
            {
                Expand(args.Expand, settings, result);
            }

            return result;
        }

        private DataWithVersion<GlobalSettingsForClient> GetForClientImpl()
        {
            return _globalSettingsCache.GetGlobalSettings();
        }

        private void Expand(string expand, GlobalSettings settings, GetByIdResponse<GlobalSettings> result)
        {
            // Add related entities
            if (expand != null)
            {

            }
        }

        private void ValidateAndPreprocessSettings(GlobalSettingsForSave entity)
        {
 
        }
    }
}
