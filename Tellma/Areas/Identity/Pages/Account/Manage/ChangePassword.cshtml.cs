﻿using Tellma.Services.EmbeddedIdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
namespace Tellma.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly SignInManager<EmbeddedIdentityServerUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;

        public ChangePasswordModel(
            UserManager<EmbeddedIdentityServerUser> userManager,
            SignInManager<EmbeddedIdentityServerUser> signInManager,
            ILogger<ChangePasswordModel> logger,
            IStringLocalizer<Strings> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = Services.Utilities.Constants.Error_Field0IsRequired)]
            [DataType(DataType.Password)]
            [Display(Name = "CurrentPassword")]
            public string OldPassword { get; set; }

            [Required(ErrorMessage = Services.Utilities.Constants.Error_Field0IsRequired)]
            [StringLength(100, ErrorMessage = Services.Utilities.Constants.Error_Field0LengthMaximumOf1 + "2", MinimumLength = 6)]
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
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
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

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = _localizer["YourPasswordHasBeenChanged"];

            return RedirectToPage();
        }
    }
}
