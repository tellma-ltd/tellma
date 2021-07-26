using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.Utilities;
using Tellma.Services.ClientProxy;

namespace Tellma.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWithRecoveryCodeModel : PageModel
    {
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ClientAppAddressResolver _resolver;

        public LoginWithRecoveryCodeModel(SignInManager<EmbeddedIdentityServerUser> signInManager, ILogger<LoginWithRecoveryCodeModel> logger, 
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

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [BindProperty]
            [Required(ErrorMessage = Constants.Error_Field0IsRequired)]
            [DataType(DataType.Text)]
            [Display(Name = "RecoveryCode")]
            public string RecoveryCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return BadRequest($"Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
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

            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return OnSignIn(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
                ModelState.AddModelError(string.Empty, _localizer["InvalidRecoveryCode"]);
                return Page();
            }
        }

        private IActionResult OnSignIn(string returnUrl)
        {
            if (returnUrl != null)
            {
                // This url most likely came from identity server
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirect to the root of the web app
                var url = _resolver.Resolve();
                return Redirect(url);
            }
        }
    }
}
