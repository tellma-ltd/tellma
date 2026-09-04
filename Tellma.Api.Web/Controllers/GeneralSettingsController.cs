using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/general-settings")]
    [ApiVersion("1.0")]
    public class GeneralSettingsController : ApplicationSettingsControllerBase<GeneralSettingsForSave, GeneralSettings>
    {
        private readonly GeneralSettingsService _service;

        public GeneralSettingsController(IServiceProvider sp, GeneralSettingsService service) : base(sp)
        {
            _service = service;
        }

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<SettingsForClient>>> SettingsForClient(CancellationToken cancellation)
        {
            var result = await _service.SettingsForClient(cancellation);
            return Ok(result);
        }

        [HttpGet("ping")]
        public async Task<ActionResult> Ping(CancellationToken cancellation)
        {
            // If all you want is to check whether the cached versions of settings and permissions 
            // are fresh you can use this API that only does that through the controller filters
            await _service.Ping(cancellation);
            return Ok();
        }


        /// <summary>
        /// Stores this tenant's Marmin (UAE) API credentials, encrypted.
        /// </summary>
        /// <remarks>
        /// A dedicated action rather than fields on the settings form, mirroring onboard-zatca:
        /// the secrets are never returned to the browser, so they cannot round-trip through a
        /// normal save. Authorized on the general-settings Update permission inside the service.
        /// </remarks>
        [HttpPut("marmin-ae-secrets")]
        public async Task<ActionResult> SaveMarminAeSecrets([FromBody] MarminAeSecretsArguments args)
        {
            await _service.SaveMarminAeSecrets(args?.ClientSecret, args?.WebhookSecret);
            return Ok();
        }

        /// <summary>
        /// The credentials posted to <c>marmin-ae-secrets</c>.
        /// </summary>
        /// <remarks>
        /// A body rather than query parameters, unlike the older onboard-zatca action: query
        /// strings are recorded in web server logs, reverse proxy logs and browser history, which
        /// is no place for an API credential.
        /// </remarks>
        public class MarminAeSecretsArguments
        {
            /// <summary>The vendor's client secret. Leave empty to keep the stored one.</summary>
            public string ClientSecret { get; set; }

            /// <summary>
            /// The webhook signing secret. Leave empty to keep the stored one. During a rotation
            /// this may be two secrets separated by a semicolon ("new;old").
            /// </summary>
            public string WebhookSecret { get; set; }
        }

        [HttpPut("onboard-zatca")]
        public async Task<ActionResult> OnboardWithZatca(
            [FromQuery] string otp,
            [FromQuery] string orgUnitName,
            [FromQuery] string industry)
        {
            await _service.OnboardWithZatca(otp, orgUnitName, industry);
            return Ok();
        }

        protected override ApplicationSettingsServiceBase<GeneralSettingsForSave, GeneralSettings> GetSettingsService()
        {
            return _service;
        }
    }
}
