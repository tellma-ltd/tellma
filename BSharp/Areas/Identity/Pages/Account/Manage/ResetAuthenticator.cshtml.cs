using System.Threading.Tasks;
using BSharp.Services.EmbeddedIdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BSharp.Areas.Identity.Pages.Account.Manage
{
    public class ResetAuthenticatorModel : PageModel
    {
        private UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private ILogger _logger;
        private readonly IStringLocalizer _localizer;

        public ResetAuthenticatorModel(
            UserManager<EmbeddedIdentityServerUser> userManager,
            SignInManager<EmbeddedIdentityServerUser> signInManager,
            ILogger<ResetAuthenticatorModel> logger,
            IStringLocalizer<Strings> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer["YourAuthenticatorAppKeyHasBeenReset"];

            return RedirectToPage("./EnableAuthenticator");
        }
    }
}
