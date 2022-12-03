using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Notifications;
using Tellma.Repository.Application;
using Tellma.Utilities.Common;
using Tellma.Utilities.Email;
using Tellma.Utilities.Sms;
using Tellma.Services.Utilities;
using Tellma.Api.Dto;

namespace Tellma.Services.ClientProxy
{
    /// <summary>
    /// An implementation of <see cref="IClientProxy"/> that interfaces with the web client.
    /// </summary>
    public class ClientAppProxy : IClientProxy
    {
        private readonly IStringLocalizer _localizer;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly ClientAppAddressResolver _clientUriResolver;
        private readonly InboxNotificationsQueue _inboxQueue;

        public ClientAppProxy(
            IStringLocalizer<Strings> localizer,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator,
            ClientAppAddressResolver clientUriResolver,
            InboxNotificationsQueue inboxQueue)
        {
            _localizer = localizer;
            _httpAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _clientUriResolver = clientUriResolver;
            _inboxQueue = inboxQueue;
        }

        public IEnumerable<EmailToSend> MakeEmailsForConfirmedUsers(int tenantId, IEnumerable<ConfirmedEmailInvitation> infos)
        {
            string companyUrl = CompanyUrl(tenantId);

            foreach (var info in infos)
            {
                // Use the recipient's preferred Language
                CultureInfo culture = string.IsNullOrWhiteSpace(info.PreferredLanguage) ?
                    CultureInfo.CurrentUICulture : new CultureInfo(info.PreferredLanguage);

                using var _ = new CultureScope(culture);

                // Prepare the email
                yield return MakeInvitationEmail(
                     emailOfRecipient: info.Email,
                     nameOfRecipient: info.Name,
                     nameOfInviter: info.InviterName,
                     companyName: info.CompanyName,
                     validityInDays: Constants.TokenExpiryInDays,
                     callbackUrl: companyUrl);
            }
        }

        public IEnumerable<EmailToSend> MakeEmailsForUnconfirmedUsers(int tenantId, IEnumerable<UnconfirmedEmailInvitation> infos)
        {
            string companyUrl = CompanyUrl(tenantId);

            foreach (var info in infos)
            {
                // Use the recipient's preferred Language
                CultureInfo culture = string.IsNullOrWhiteSpace(info.PreferredLanguage) ?
                    CultureInfo.CurrentUICulture : new CultureInfo(info.PreferredLanguage);

                using var _ = new CultureScope(culture);

                var callbackUrlBuilder = new UriBuilder(info.EmailConfirmationLink);
                callbackUrlBuilder.Query = $"{callbackUrlBuilder.Query}&returnUrl={UrlEncode(companyUrl)}";
                string callbackUrl = callbackUrlBuilder.Uri.ToString();

                // Prepare the email
                yield return MakeInvitationEmail(
                     emailOfRecipient: info.Email,
                     nameOfRecipient: info.Name,
                     nameOfInviter: info.InviterName,
                     companyName: info.CompanyName,
                     validityInDays: Constants.TokenExpiryInDays,
                     callbackUrl: callbackUrl);
            }
        }

        public IEnumerable<EmailToSend> MakeEmailsForConfirmedAdminUsers(IEnumerable<ConfirmedAdminEmailInvitation> infos)
        {
            string adminUrl = AdminUrl();

            foreach (var info in infos)
            {
                // Prepare the email
                yield return MakeAdminInvitationEmail(
                     emailOfRecipient: info.Email,
                     nameOfRecipient: info.Name,
                     nameOfInviter: info.InviterName,
                     validityInDays: Constants.TokenExpiryInDays,
                     callbackUrl: adminUrl);
            }
        }

        public IEnumerable<EmailToSend> MakeEmailsForUnconfirmedAdminUsers(IEnumerable<UnconfirmedAdminEmailInvitation> infos)
        {
            string adminUrl = AdminUrl();

            foreach (var info in infos)
            {
                var callbackUrlBuilder = new UriBuilder(info.EmailConfirmationLink);
                callbackUrlBuilder.Query = $"{callbackUrlBuilder.Query}&returnUrl={UrlEncode(adminUrl)}";
                string callbackUrl = callbackUrlBuilder.Uri.ToString();

                // Prepare the email
                yield return MakeAdminInvitationEmail(
                     emailOfRecipient: info.Email,
                     nameOfRecipient: info.Name,
                     nameOfInviter: info.InviterName,
                     validityInDays: Constants.TokenExpiryInDays,
                     callbackUrl: callbackUrl);
            }
        }

        public void UpdateInboxStatuses(int tenantId, IEnumerable<InboxStatus> statuses, bool updateInboxList = true)
        {
            if (statuses == null || !statuses.Any())
            {
                return;
            }

            DateTimeOffset now = DateTimeOffset.Now;
            _inboxQueue.QueueBackgroundWorkItem(statuses.Select(e => FromEntity(e, tenantId, updateInboxList, now)));
        }

        public EmailToSend MakeDocumentAssignmentEmail(int tenantId, string contactEmail, NotifyDocumentAssignmentArguments args)
        {
            string linkUrl = AssignmentUrl(tenantId, args);

            // Email
            return MakeInboxNotificationEmail(
                toEmail: contactEmail,
                formattedSerial: args.FormattedSerial,
                singularTitle: args.SingularTitle,
                pluralTitle: args.PluralTitle,
                senderName: args.SenderName,
                docCount: args.DocumentCount,
                comment: args.SenderComment,
                linkUrl);
        }

        public SmsToSend MakeDocumentAssignmentSms(int tenantId, string contactMobile, NotifyDocumentAssignmentArguments args)
        {
            string linkUrl = AssignmentUrl(tenantId, args);

            // SMS notification
            var msgBuilder = new StringBuilder();
            if (args.DocumentCount == 1)
            {
                msgBuilder.Append(_localizer["Document0From1", args.FormattedSerial, args.SenderName]);
            }
            else
            {
                msgBuilder.Append(_localizer["Document0From1", $"{args.DocumentCount} {args.PluralTitle}", args.SenderName]);
            }

            if (!string.IsNullOrWhiteSpace(args.SenderComment))
            {
                msgBuilder.Append($": {args.SenderComment}");
            }

            msgBuilder.AppendLine();
            msgBuilder.Append(linkUrl);

            return new SmsToSend(contactMobile, msgBuilder.ToString());
        }

        public PushToSend MakeDocumentAssignmentPush(int tenantId, NotifyDocumentAssignmentArguments args)
        {
            // TODO
            return null;
        }

        public EmailToSend MakeTenantNotificationEmail(TenantLogEntry le)
        {
            return new EmailToSend
            {
                TenantId = le.TenantId,
                Subject = "Error in a custom Script!",
                Body = $"<p>{HtmlEncode((le as CustomScriptErrorLogEntry).ErrorMessage)}</p>",
                To = new List<string> { "ahmadakra1990@gmail.com" }
            };

            //if (le is CustomScriptErrorLogEntry cle)
            //{

            //}
            //else
            //{

            //}
        }

        #region Email Making

        private EmailToSend MakeInboxNotificationEmail(
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

            var htmlContentBuilder = new StringBuilder($@"<p>
{HtmlEncode(preamble)}
</p>");
            // Sender comment
            if (!string.IsNullOrWhiteSpace(comment))
            {
                htmlContentBuilder.AppendLine($@"<p>
    ""{HtmlEncode(comment)}""
</p>");
            }

            // Button
            htmlContentBuilder.AppendLine($@"<div style=""text-align: center;padding: 1rem 0;"">
            <a href=""{HtmlEncode(linkUrl)}"" style=""{ButtonStyle}"">
                {HtmlEncode(buttonLabel)}
            </a>
        </div>");

            return new EmailToSend(toEmail)
            {
                Subject = subject,
                Body = MakeEmail(htmlContentBuilder.ToString(), includeBanner: false)
            };
        }

        private EmailToSend MakeInvitationEmail(string emailOfRecipient, string nameOfRecipient, string nameOfInviter, string companyName, int validityInDays, string callbackUrl)
        {
            string greeting = _localizer["InvitationEmailGreeting0", nameOfRecipient];
            string appName = _localizer["AppName"];
            string body = _localizer["InvitationEmailBody0123", nameOfInviter, companyName, appName, validityInDays];
            string buttonLabel = _localizer["InvitationEmailButtonLabel"];
            string conclusion = _localizer["InvitationEmailConclusion"];
            string signature = _localizer["InvitationEmailSignature0", appName];

            string mainHtmlContent = $@"
        <p style=""font-weight: bold;font-size: 120%;"">
            {HtmlEncode(greeting)}
        </p>
        <p>
            {HtmlEncode(body)}
        </p>
        <div style=""text-align: center;padding: 1rem 0;"">
            <a href=""{HtmlEncode(callbackUrl)}"" style=""{ButtonStyle}"">
                {HtmlEncode(buttonLabel)}
            </a>
        </div>
        <p>
            {HtmlEncode(conclusion)}
            <br>
            {HtmlEncode(signature)}
        </p>
";
            var emailBody = MakeEmail(mainHtmlContent, includeBanner: true);
            var emailSubject = _localizer["InvitationEmailSubject0", _localizer["AppName"]];

            return new EmailToSend(emailOfRecipient) { Body = emailBody, Subject = emailSubject };
        }

        private EmailToSend MakeAdminInvitationEmail(string emailOfRecipient, string nameOfRecipient, string nameOfInviter, int validityInDays, string callbackUrl)
        {
            string greeting = _localizer["InvitationEmailGreeting0", nameOfRecipient];
            string appName = _localizer["AppName"];
            string body = _localizer["InvitationToAdminEmailBody012", nameOfInviter, appName, validityInDays];
            string buttonLabel = _localizer["InvitationEmailButtonLabel"];
            string conclusion = _localizer["InvitationEmailConclusion"];
            string signature = _localizer["InvitationEmailSignature0", appName];

            string mainHtmlContent = $@"
        <p style=""font-weight: bold;font-size: 120%;"">
            {HtmlEncode(greeting)}
        </p>
        <p>
            {HtmlEncode(body)}
        </p>
        <div style=""text-align: center;padding: 1rem 0;"">
            <a href=""{HtmlEncode(callbackUrl)}"" style=""{ButtonStyle}"">
                {HtmlEncode(buttonLabel)}
            </a>
        </div>
        <p>
            {HtmlEncode(conclusion)}
            <br>
            {HtmlEncode(signature)}
        </p>
";
            var emailBody = MakeEmail(mainHtmlContent, includeBanner: true);
            var emailSubject = _localizer["InvitationEmailSubject0", _localizer["AppName"]];

            return new EmailToSend(emailOfRecipient) { Body = emailBody, Subject = emailSubject };
        }

        #region Helpers

        private const string BrandColor = "#343a40"; // Dark grey
        private const string BackgroundColor = "#f8f9fa"; // Light grey
        private const string HyperlinkColor = "#008784"; // Greenish
        private const string ButtonStyle = "padding: 12px 20px;text-decoration: none;color: white;background: #17a2b8;display: inline-block;"; // Teal

        private string MakeEmail(string mainHtmlContent, bool includeBanner)
        {
            var direction = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "rtl" : "ltr";
            var fontFamily = "BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji'";

            var logoLink = ClientAppUri().WithTrailingSlash() + "assets/tellma.png";
            var copyRightNotice = _localizer["CopyrightNotice0", DateTime.Today.Year];
            var privacyLabel = _localizer["PrivacyPolicy"];
            var termsLabel = _localizer["TermsOfService"];
            var privacyLink = _linkGenerator.GetUriByPage(_httpAccessor.HttpContext, page: "/Privacy");
            var termsLink = _linkGenerator.GetUriByPage(_httpAccessor.HttpContext, page: "/TermsOfService");

            var brandBanner = includeBanner ? $@"<tr>
        <td style=""background: {BrandColor};padding: 1rem;text-align: center;"">
            <img height=""20px"" clicktracking=off src=""{logoLink}""></img>
        </td>
    </tr>" : "";

            return
            $@"<table style=""font-size:1rem;direction: {direction};padding: 0.5rem;background-color: {BackgroundColor};max-width: 900px;border: 1px solid lightgrey;font-family: {fontFamily};"">
    {brandBanner}
    <tr>
        <td style=""padding: 1rem 3rem;"">
            {mainHtmlContent}
        </td>
    </tr>
    <tr>
        <td style=""padding: 1rem 3rem;border-top: 1px solid lightgrey;font-size: 80%;text-align: center;"">
            <p>
                {HtmlEncode(copyRightNotice)}
            </p>
            <a style=""color: {HyperlinkColor};"" clicktracking=off href=""{privacyLink}"">{HtmlEncode(privacyLabel)}</a><span style=""margin-left: 0.5rem;margin-right: 0.5rem;"">|</span><a
                style=""color: {HyperlinkColor};"" clicktracking=off href=""{termsLink}"">{HtmlEncode(termsLabel)}</a>
        </td>
    </tr>
</table>
";
        }

        /// <summary>
        /// If a string value comes from user input or a localization file, it is important to encode
        /// it before inserting it into the HTML document, otherwise characters like © will cause trouble.
        /// </summary>
        private static string HtmlEncode(string value) => HtmlEncoder.Default.Encode(value);

        private static string UrlEncode(string value) => UrlEncoder.Default.Encode(value);

        private string CompanyUrl(int tenantId)
        {
            var urlBuilder = new UriBuilder(ClientAppUri());
            urlBuilder.Path = $"{urlBuilder.Path.WithoutTrailingSlash()}/app/{tenantId}";
            string url = urlBuilder.Uri.ToString();

            return url;
        }

        private string AdminUrl()
        {
            var urlBuilder = new UriBuilder(ClientAppUri());
            urlBuilder.Path = $"{urlBuilder.Path.WithoutTrailingSlash()}/admin/console";
            string url = urlBuilder.Uri.ToString();

            return url;
        }

        private string AssignmentUrl(int tenantId, NotifyDocumentAssignmentArguments args)
        {
            // Prepare the document/inbox link
            var clientAppUriBldr = new UriBuilder(ClientAppUri());
            var basePath = clientAppUriBldr.Path.WithoutTrailingSlash();
            if (args.DocumentCount == 1 && args.DefinitionId != 0 && args.DocumentId != 0)
            {
                clientAppUriBldr.Path = $"{basePath}/a/{tenantId}/d/{args.DefinitionId}/{args.DocumentId}";
            }
            else
            {
                clientAppUriBldr.Path = $"{basePath}/a/{tenantId}/inbox";
            }

            string linkUrl = clientAppUriBldr.Uri.ToString();
            return linkUrl;
        }

        private string ClientAppUri() => _clientUriResolver.Resolve();

        #endregion

        #endregion

        #region Helper Functions

        /// <summary>
        /// Helper function.
        /// </summary>
        private static InboxStatusToSend FromEntity(InboxStatus e, int tenantId, bool updateInboxList, DateTimeOffset? serverTime)
        {
            return new InboxStatusToSend
            {
                Count = e.Count,
                UnknownCount = e.UnknownCount,
                UpdateInboxList = updateInboxList,
                TenantId = tenantId,
                ServerTime = serverTime ?? DateTimeOffset.Now,
                ExternalId = e.ExternalId
            };
        }

        #endregion
    }
}
