using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace Tellma.Pages
{
    public class SetCultureModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("~/");
        }

        /// <summary>
        /// All this does is set the ASP.NET Core culture cookie, and redirect back to the same URL
        /// </summary>
        public IActionResult OnPost(string currentUrl = null, string culture = null)
        {
            if(currentUrl == null)
            {
                return BadRequest("Must supply the current URL");
            }

            if (culture == null)
            {
                return LocalRedirect(currentUrl);
            }

            var cultureInfo = new CultureInfo(culture);
            if(cultureInfo == null)
            {
                return LocalRedirect(currentUrl);
            }

            // set the culture cookie according to the standard format https://bit.ly/2TRp252
            string cookieName = CookieRequestCultureProvider.DefaultCookieName;
            string cookieValue = $"c={CultureInfo.CurrentCulture.Name}|uic={cultureInfo.Name}";
            Response.Cookies.Append(cookieName, cookieValue);

            return LocalRedirect(currentUrl);
        }
    }
}
