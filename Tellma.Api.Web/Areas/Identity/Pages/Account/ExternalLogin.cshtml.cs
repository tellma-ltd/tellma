using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Tellma.Services.ClientProxy;
using Tellma.Services.EmbeddedIdentityServer;

namespace Tellma.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ClientAppAddressResolver _resolver;

        public ExternalLoginModel(
            SignInManager<EmbeddedIdentityServerUser> signInManager,
            UserManager<EmbeddedIdentityServerUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IStringLocalizer<Strings> localizer,
            ClientAppAddressResolver resolver)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _localizer = localizer;
            _resolver = resolver;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = _localizer["Error_ErrorFromExternalProvider0", remoteError];
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = _localizer["Error_LoadingExternalLoginInformation"];
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return OnSignIn(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If we don't find a matching login we try to match the email claim to one of the existing users
                string email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if(email == null)
                {
                    ErrorMessage = _localizer["Error_EmailNotProvidedByExternalSignIn"];
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if(user == null)
                {
                    ErrorMessage = _localizer["Error_AUserWithEmail0CouldNotBeFound", email];
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                if(!await _userManager.IsEmailConfirmedAsync(user))
                {
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, emailConfirmationToken);

                    if(!confirmEmailResult.Succeeded)
                    {
                        ErrorMessage = _localizer["Error_ConfirmingYourEmail"];
                        string errors = string.Join(", ", confirmEmailResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to confirm email for user with ID '{user.Id}' and email '{user.Email}', errors: " + errors);

                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                    }
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    ErrorMessage = _localizer["Error_AddingLoginForUserWithEmail0", email];
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                    return OnSignIn(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ErrorMessage = _localizer["Error_TryingToSignYouInWithExternalProvider"];
                    _logger.LogError($"Failed to log user with ID '{user.Id}' and email '{user.Email}'");
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
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
