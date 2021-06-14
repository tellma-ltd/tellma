using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Tellma.Services.Email;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Utiltites
{
    public class EmailTemplatesProvider
    {
        public const string BrandColor = "#343a40"; // Dark grey
        public const string BackgroundColor = "#f8f9fa"; // Light grey
        public const string ButtonColor = "#17a2b8"; // Teal
        public const string HyperlinkColor = "#008784"; // Greenish

        private readonly IStringLocalizer _localizer;
        private readonly IHttpContextAccessor _httpAccessor;

        public EmailTemplatesProvider(IStringLocalizer<Strings> localizer, IHttpContextAccessor httpAccessor)
        {
            _localizer = localizer;
            _httpAccessor = httpAccessor;
        }

        private string AppServerDomain()
        {
            var request = _httpAccessor.HttpContext.Request;
            return $"https://{request?.Host}/{request?.PathBase}";
        }

        public Email MakeInboxNotificationEmail(
            string toEmail,
            string formattedSerial,
            string singularTitle,
            string pluralTitle,
            string senderName,
            int docCount,
            string comment,
            string linkUrl)
        {
            string subject;
            string preamble;
            string buttonLabel;
            if (docCount == 1)
            {
                subject = _localizer["Document0From1", formattedSerial, senderName];
                preamble = _localizer["User0SentYouDocument12", senderName, singularTitle, formattedSerial];
                buttonLabel = _localizer["GoTo0", formattedSerial];
            }
            else
            {
                subject = _localizer["Document0From1", $"{docCount} {pluralTitle}", senderName];
                preamble = _localizer["User0SendYou1DocumentsOfType2", senderName, docCount, pluralTitle];
                buttonLabel = _localizer["GoTo0", _localizer["Inbox"]];
            }            

            StringBuilder htmlContentBuilder = new StringBuilder($@"<p>
{Encode(preamble)}
</p>");
            // Sender comment
            if (!string.IsNullOrWhiteSpace(comment))
            {
                htmlContentBuilder.AppendLine($@"<p>
    ""{Encode(comment)}""
</p>");
            }

            // Button
            htmlContentBuilder.AppendLine($@"<div style=""text-align: center;padding: 1rem 0;"">
            <a href=""{Encode(linkUrl)}"" style=""padding: 12px 20px;text-decoration: none;color: white;background: {ButtonColor};display: inline-block;"">
                {Encode(buttonLabel)}
            </a>
        </div>");

            return new Email(toEmail)
            {
                Subject = subject,
                Body = MakeEmail(htmlContentBuilder.ToString(), includeBanner: false)
            };
        }

        public string MakeInvitationEmail(string nameOfRecipient, string nameOfInvitor, int validityInDays, string userId, string callbackUrl, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentUICulture;
            using var _ = new CultureScope(culture);

            string greeting = _localizer["InvitationEmailGreeting0", nameOfRecipient];
            string appName = _localizer["AppName"];
            string body = _localizer["InvitationEmailBody012", nameOfInvitor, appName, validityInDays];
            string buttonLabel = _localizer["InvitationEmailButtonLabel"];
            string conclusion = _localizer["InvitationEmailConclusion"];
            string signature = _localizer["InvitationEmailSignature0", appName];

            string mainHtmlContent = $@"
        <p style=""font-weight: bold;font-size: 120%;"">
            {Encode(greeting)}
        </p>
        <p>
            {Encode(body)}
        </p>
        <div style=""text-align: center;padding: 1rem 0;"">
            <a href=""{Encode(callbackUrl)}"" style=""padding: 12px 20px;text-decoration: none;color: white;background: {ButtonColor};display: inline-block;"">
                {Encode(buttonLabel)}
            </a>
        </div>
        <p>
            {Encode(conclusion)}
            <br>
            {Encode(signature)}
        </p>
";
            return MakeEmail(mainHtmlContent, includeBanner: true, culture);
        }

        public string MakeEmail(string mainHtmlContent, bool includeBanner, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentUICulture;
            using var _ = new CultureScope(culture);

            var appName = _localizer["AppName"];
            var appDomain = AppServerDomain();
            var copyRightNotice = _localizer["CopyrightNotice0", DateTime.Today.Year];
            var privacyPolicy = _localizer["PrivacyPolicy"];
            var termsOfService = _localizer["TermsOfService"];
            var direction = culture.TextInfo.IsRightToLeft ? "rtl" : "ltr";


            var brandBanner = includeBanner ? $@"<tr>
        <td style=""background: {BrandColor};padding: 1rem;text-align: center;"">
            <img height=""20px"" clicktracking=off  src=""{appDomain}img/tellma.png""></img>
        </td>
    </tr>" : "";

            string result =
            $@"
<table style=""font-size:1rem;direction: {direction};padding: 0.5rem;background-color: {BackgroundColor};max-width: 900px;border: 1px solid lightgrey;font-family: BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji';"">{brandBanner}
    <tr>
        <td style=""padding: 1rem 3rem;"">
            {mainHtmlContent}
        </td>
    </tr>
    <tr>
        <td style=""padding: 1rem 3rem;border-top: 1px solid lightgrey;font-size: 80%;text-align: center;"">
            <p>
                {Encode(copyRightNotice)}
            </p>
            <a style=""color: {HyperlinkColor};"" clicktracking=off href=""{appDomain}privacy"">{Encode(privacyPolicy)}</a><span style=""margin-left: 0.5rem;margin-right: 0.5rem;"">|</span><a
                style=""color: {HyperlinkColor};"" clicktracking=off href=""{appDomain}terms-of-service"">{Encode(termsOfService)}</a>
        </td>
    </tr>
</table>
";
            return result;
        }

        /// <summary>
        /// If a string value comes from user input or a localization file, it is important to encode it before inserting it into the HTML document, characters like © will cause trouble
        /// </summary>
        private static string Encode(string value)
        {
            return HtmlEncoder.Default.Encode(value);
        }
    }
}
