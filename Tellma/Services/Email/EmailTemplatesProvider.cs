using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Globalization;
using System.Text.Encodings.Web;

namespace Tellma.Services.Email
{
    public class EmailTemplatesProvider
    {
        private readonly IStringLocalizer _localizer;
        private readonly IHttpContextAccessor _httpAccessor;

        public EmailTemplatesProvider(IStringLocalizer<Strings> localizer, IHttpContextAccessor httpAccessor)
        {
            _localizer = localizer;
            _httpAccessor = httpAccessor;
        }

        private string AppDomain()
        {
            var request = _httpAccessor.HttpContext.Request;
            return $"https://{request?.Host}/{request?.PathBase}";
        }

        public string MakeInvitationEmail(string nameOfRecipient, string nameOfInvitor, int validityInDays, string userId, string callbackUrl, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentUICulture;
            var localizer = _localizer.WithCulture(culture);
            string greeting = localizer["InvitationEmailGreeting0", nameOfRecipient];
            string appName = localizer["AppName"];
            string body = localizer["InvitationEmailBody012", nameOfInvitor, appName, validityInDays];
            string buttonLabel = localizer["InvitationEmailButtonLabel"];
            string conclusion = localizer["InvitationEmailConclusion"];
            string signature = localizer["InvitationEmailSignature0", appName];

            string mainContent = $@"
        <p style=""font-weight: bold;font-size: 120%;"">
            {greeting}
        </p>
        <p>
            {body}
        </p>
        <div style=""text-align: center;padding: 2rem;"">

            <a href=""{HtmlEncoder.Default.Encode(callbackUrl)}"" style=""padding: 12px 20px;text-decoration: none;color: white;background: #17a2b8;"">
                {buttonLabel}
            </a>
        </div>
        <p>
            {conclusion}
            <br>
            {signature}
        </p>
";

            return MakeEmail(mainContent, culture);
        }

        public string MakeEmail(string mainContent, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentUICulture;
            var localizer = _localizer.WithCulture(culture);

            var appName = localizer["AppName"];
            var appDomain = AppDomain();
            var copyRightNotice = localizer["CopyrightNotice0", DateTime.Today.Year];
            var privacyPolicy = localizer["PrivacyPolicy"];
            var termsOfService = localizer["TermsOfService"];
            var direction = culture.TextInfo.IsRightToLeft ? "rtl" : "ltr";

            string result =
            $@"
<table style=""font-size:1rem;direction: {direction};padding: 0.5rem;background-color: #f8f9fa;max-width: 900px;border: 1px solid lightgrey;font-family: BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji';"">
    <tr>
        <td style=""background: #343a40;padding: 1rem;text-align: center;"">
            <img height=""30px"" src=""{appDomain}img/tellma.png""></img>
        </td>
    </tr>
    <tr>
        <td style=""padding: 1rem 3rem;"">
            {mainContent}
        </td>
    </tr>
    <tr>
        <td style=""padding: 1rem 3rem;border-top: 1px solid lightgrey;font-size: 80%;text-align: center;"">
            <p>
                {copyRightNotice}
            </p>
            <a style=""color: #008784;"" href=""{appDomain}privacy"">{privacyPolicy}</a>&nbsp;&nbsp;|&nbsp;&nbsp;<a
                style=""color: #008784;"" href=""{appDomain}terms-of-service"">{termsOfService}</a>
        </td>
    </tr>
</table>
";
            return result;
        }
    }
}
