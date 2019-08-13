using System;
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
using Microsoft.Extensions.Options;
using BSharp.Services.EmbeddedIdentityServer;

namespace BSharp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ClientApplicationsOptions _config;

        public LoginModel(SignInManager<EmbeddedIdentityServerUser> signInManager, 
            ILogger<LoginModel> logger, IStringLocalizer<Strings> localizer, IOptions<ClientApplicationsOptions> options)
        {
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
            _config = options.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = nameof(RequiredAttribute))]
            [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = nameof(RequiredAttribute))]
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

            returnUrl = returnUrl ?? DefaultReturnUrl();

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        private string DefaultReturnUrl()
        {
            return _config.WebClientUri ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? DefaultReturnUrl(); // Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    if(returnUrl != null)
                    {
                        return SafeRedirect(returnUrl);
                    }
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
