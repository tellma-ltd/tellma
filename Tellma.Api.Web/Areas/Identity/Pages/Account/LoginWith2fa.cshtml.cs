using Tellma.Services.EmbeddedIdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Tellma.Services.Utilities;
using Tellma.Services.ClientProxy;

namespace Tellma.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ClientAppAddressResolver _resolver;

        public LoginWith2faModel(SignInManager<EmbeddedIdentityServerUser> signInManager,
            ILogger<LoginWith2faModel> logger, 
            IStringLocalizer<Strings> localizer,
            ClientAppAddressResolver resolver)
        {
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
            _resolver = resolver;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = Constants.Error_Field0IsRequired)]
            [StringLength(7, ErrorMessage = Constants.Error_Field0LengthMaximumOf1 + "2", MinimumLength = 6)]
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
                return OnSignIn(returnUrl);
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

        private IActionResult OnSignIn(string returnUrl)
        {
            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                // This url most likely came from identity server
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirect to the root of the web app
                var webAppUrl = _resolver.Resolve();
                if (returnUrl != null && returnUrl.StartsWith(webAppUrl))
                {
                    // If the returnUrl takes the user to the client app
                    return Redirect(returnUrl);
                }
                else
                {
                    // If we could not recognize the returnUrl
                    return Redirect(webAppUrl);
                }
            }
        }
    }
}
