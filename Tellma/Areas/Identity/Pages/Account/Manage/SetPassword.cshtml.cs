using Tellma.Services.EmbeddedIdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Tellma.Areas.Identity.Pages.Account.Manage
{
    public class SetPasswordModel : PageModel
    {
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private readonly IStringLocalizer _localizer;

        public SetPasswordModel(
            UserManager<EmbeddedIdentityServerUser> userManager,
            SignInManager<EmbeddedIdentityServerUser> signInManager,
            IStringLocalizer<Strings> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
            [StringLength(100, ErrorMessage = nameof(StringLengthAttribute) + "2", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "NewPassword")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "ConfirmNewPassword")]
            [Compare("NewPassword", ErrorMessage = "Error_ThePasswordAndConfirmationPasswordDoNotMatch")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
              //  return RedirectToPage("./ChangePassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer["YourPasswordHasBeenSet"];

            return RedirectToPage();
        }
    }
}
