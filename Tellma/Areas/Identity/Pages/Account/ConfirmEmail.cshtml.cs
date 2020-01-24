using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Services.EmbeddedIdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Tellma.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public ConfirmEmailModel(UserManager<EmbeddedIdentityServerUser> userManager, ILogger<ConfirmEmailModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code, string passwordCode)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("./Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                string message = $"Error confirming email for user with ID '{userId}':";
                _logger.LogInformation(message);
                return BadRequest(message);
            }

            if(passwordCode != null)
            {
                return RedirectToPage("./ResetPassword", new { code = passwordCode, email = user.Email, justConfirmedEmail = true });
            }
            else
            {
                return Page();
            }
        }
    }
}
