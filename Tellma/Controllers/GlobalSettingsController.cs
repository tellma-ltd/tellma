using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;
using Tellma.Controllers.Dto;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/global-settings")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class GlobalSettingsController : ControllerBase
    {
        private readonly GlobalSettingsProvider _globalSettingsProvider;
        private readonly ILogger<GlobalSettingsController> _logger;

        public GlobalSettingsController(GlobalSettingsProvider globalSettingsProvider, ILogger<GlobalSettingsController> logger)
        {
            _globalSettingsProvider = globalSettingsProvider;
            _logger = logger;
        }

        [HttpGet("client")]
        public ActionResult<Versioned<GlobalSettingsForClient>> GlobalSettingsForClient()
        {
            try
            {
                var result = _globalSettingsProvider.GetForClient();
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
            // If all you want is to check whether the cached versions of global settings
            // are fresh you can use this API that only does that through the registered filter

            return Ok();
        }
    }

    /// <summary>
    /// Singleton service to store and provide the global settings for client object as well as its SHA1 hash
    /// </summary>
    public class GlobalSettingsProvider
    {
        // If these settings change, the app restarts
        private readonly Versioned<GlobalSettingsForClient> versionedSettings;

        public GlobalSettingsProvider(IOptions<GlobalOptions> options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Compute the global settings object
            var opt = options.Value;
            var settings = new GlobalSettingsForClient
            {
                EmailEnabled = opt.EmailEnabled,
                SmsEnabled = opt.SmsEnabled
            };

            // Compute the version as SHA1 of the JSON representation of the global settings
            var settingsText = JsonConvert.SerializeObject(settings);
            var version = Sha1Hash(settingsText);

            // Construct the for client object
            versionedSettings = new Versioned<GlobalSettingsForClient>(settings, version);
        }

        /// <summary>
        /// Returns the latest version of the global settings for client
        /// </summary>
        /// <returns></returns>
        public Versioned<GlobalSettingsForClient> GetForClient()
        {
            return versionedSettings;
        }

        /// <summary>
        /// Returns whether or not the provided version string is the latest version of the global settings for client
        /// </summary>
        public bool IsFresh(string version)
        {
            return versionedSettings.Version == version;
        }

        /// <summary>
        /// Helper method that computes the SHA1 hash of any string
        /// </summary>
        private string Sha1Hash(string text)
        {
            using SHA1Managed sha1 = new SHA1Managed();

            // Compute hash bytes
            var bytes = Encoding.UTF8.GetBytes(text);
            var hashBytes = sha1.ComputeHash(bytes);

            // Turn hash bytes into string
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            var hashText = sb.ToString();

            return hashText;
        }
    }
}
