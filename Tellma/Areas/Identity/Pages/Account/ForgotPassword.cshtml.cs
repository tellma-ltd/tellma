using Tellma.Services.Email;
using Tellma.Services.EmbeddedIdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer _localizer;

        public ForgotPasswordModel(UserManager<EmbeddedIdentityServerUser> userManager, IEmailSender emailSender, IStringLocalizer<Strings> localizer)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _localizer = localizer;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = Constants.Error_Field0IsRequired)]
            [EmailAddress(ErrorMessage = Constants.Error_Field0IsNotValidEmail)]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Page(
                    pageName: "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code, email = Input.Email },
                    protocol: Request.Scheme);

                string toResetClickTheFollowingLink = _localizer["ResetPasswordEmailMessage"];
                string resetMyPassword = _localizer["ResetMyPassword"];
                string resetLink = $@" <a href=""{HtmlEncoder.Default.Encode(callbackUrl)}"" >{resetMyPassword}</a>";
                string emailBody = toResetClickTheFollowingLink + resetLink;

                string emailSubject = _localizer["ResetYourPassword"];
                await _emailSender.SendAsync(new Email(Input.Email)
                {
                    Subject = emailSubject,
                    Body = emailBody
                });

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
