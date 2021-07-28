using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Tellma.Api.Dto;
using Tellma.Controllers.Dto;
using Tellma.Utilities.Email;
using Tellma.Utilities.Sms;

namespace Tellma.Controllers
{
    [Route("api/global-settings")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class GlobalSettingsController : ControllerBase
    {
        private readonly GlobalSettingsProvider _globalSettingsProvider;

        public GlobalSettingsController(GlobalSettingsProvider globalSettingsProvider)
        {
            _globalSettingsProvider = globalSettingsProvider;
        }

        [HttpGet("client")]
        public ActionResult<Versioned<GlobalSettingsForClient>> GlobalSettingsForClient()
        {
            var result = _globalSettingsProvider.GetForClient();
            return Ok(result);
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
    /// Singleton service to store and provide the global settings for client object as well as its version computed using SHA1.
    /// </summary>
    public class GlobalSettingsProvider
    {
        // If these settings change, the app restarts
        private readonly Versioned<GlobalSettingsForClient> _versionedSettings;

        public GlobalSettingsProvider(IEmailSender email, ISmsSender sms)
        {
            // Compute the global settings object
            var settings = new GlobalSettingsForClient
            {
                EmailEnabled = email.IsEnabled,
                SmsEnabled = sms.IsEnabled,
                PushEnabled = false,
            };

            // Compute the version as SHA1 of the JSON representation of the global settings
            var settingsText = JsonConvert.SerializeObject(settings);
            var version = Sha1Hash(settingsText);

            // Construct the for client object
            _versionedSettings = new Versioned<GlobalSettingsForClient>(settings, version);
        }

        /// <summary>
        /// Returns the latest version of the global settings for client.
        /// </summary>
        public Versioned<GlobalSettingsForClient> GetForClient()
        {
            return _versionedSettings;
        }

        /// <summary>
        /// Returns whether or not the provided version string is the latest version of the global settings for client.
        /// </summary>
        public bool IsFresh(string version)
        {
            return _versionedSettings.Version == version;
        }

        /// <summary>
        /// Helper method that computes the SHA1 hash of any string.
        /// </summary>
        private static string Sha1Hash(string text)
        {
            using var sha1 = new SHA1Managed();

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
