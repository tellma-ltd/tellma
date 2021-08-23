using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
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
    public class LoginModel : PageModel
    {
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ClientAppAddressResolver _resolver;

        public LoginModel(SignInManager<EmbeddedIdentityServerUser> signInManager, 
            ILogger<LoginModel> logger, IStringLocalizer<Strings> localizer, 
            ClientAppAddressResolver resolver)
        {
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
            _resolver = resolver;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = Constants.Error_Field0IsRequired)]
            [EmailAddress(ErrorMessage = Constants.Error_Field0IsNotValidEmail)]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = Constants.Error_Field0IsRequired)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Display(Name = "RememberMe")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // returnUrl ??= DefaultReturnUrl();

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return OnSignIn(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    // Clear the existing external cookie to ensure a clean login process
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                    ModelState.AddModelError(string.Empty, _localizer["Error_InvalidLoginAttempt"]);
                    return Page();
                }
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // If we got this far, something failed, redisplay form
            return Page();
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
