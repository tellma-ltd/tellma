using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BSharp.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BSharp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public ResetPasswordModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public bool JustConfirmedEmail { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            //[Required(ErrorMessage = nameof(RequiredAttribute))]
            //[EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
            //[Display(Name = "Email")]
            //public string Email { get; set; }

            [Required(ErrorMessage = nameof(RequiredAttribute))]
            [StringLength(100, ErrorMessage = nameof(StringLengthAttribute) + "2", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "ConfirmPassword")]
            [Compare("Password", ErrorMessage = "Error_ThePasswordAndConfirmationPasswordDoNotMatch")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }
            public string Email { get; set; }
        }

        public IActionResult OnGet(string code = null, string email = null, bool justConfirmedEmail = false)
        {
            if (code == null)
            {
                // Should not reach here under normal circumstances
                return BadRequest("A code must be supplied for password reset.");
            }
            else if(email == null)
            {
                return BadRequest("An email must be supplied for password reset");
            }
            else
            {
                JustConfirmedEmail = justConfirmedEmail;

                Input = new InputModel
                {
                    Code = code      ,
                    Email = email
                };

                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
