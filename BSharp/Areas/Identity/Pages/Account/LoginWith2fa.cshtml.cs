using BSharp.Services.EmbeddedIdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BSharp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ClientApplicationsOptions _config;

        public LoginWith2faModel(SignInManager<EmbeddedIdentityServerUser> signInManager, ILogger<LoginWith2faModel> logger, 
            IStringLocalizer<Strings> localizer, IOptions<ClientApplicationsOptions> options)
        {
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
            _config = options.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = nameof(RequiredAttribute))]
            [StringLength(7, ErrorMessage = nameof(StringLengthAttribute) + "2", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "AuthenticatorCode")]
            public string TwoFactorCode { get; set; }

            [Display(Name = "RememberThisDevice")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return BadRequest($"Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? _config.WebClientUri ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return BadRequest($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return SafeRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, _localizer["InvalidAuthenticatorCode"]);
                return Page();
            }
        }

        private ActionResult SafeRedirect(string url)
        {
            if (url == _config.WebClientUri)
            {
                return Redirect(url);
            }
            else
            {
                return LocalRedirect(url);
            }
        }
    }
}
