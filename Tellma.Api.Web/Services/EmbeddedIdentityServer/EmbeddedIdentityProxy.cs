using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Api;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Implementation of <see cref="IIdentityProxy"/> that interfaces with the embedded identity server.
    /// </summary>
    public class EmbeddedIdentityProxy : IIdentityProxy
    {
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly IClientProxy _clientProxy;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly LinkGenerator _linkGenerator;

        public EmbeddedIdentityProxy(
            UserManager<EmbeddedIdentityServerUser> userManager,
            IClientProxy clientProxy,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _clientProxy = clientProxy;
            _httpAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        public bool CanCreateUsers => true;
        public bool CanInviteUsers => _clientProxy.EmailEnabled;

        public async Task CreateUsersIfNotExist(IEnumerable<string> emails, bool emailConfirmed = false)
        {
            bool confirmed = emailConfirmed || !CanInviteUsers;
            foreach (var email in emails)
            {
                await GetOrCreateUser(email, emailConfirmed: confirmed);
            }

            //// This causes concurrent access to the DbContext used by the userManager, need a better solution
            // await Task.WhenAll(emails.Select(async email => await GetOrCreateUser(email, emailConfirmed: confirmed)));
        }

        /// <summary>
        /// For every email: <br/>
        /// - If the email is confirmed, send an invitation containing a link to the tenant main menu. <br/>
        /// - If the email is not confirmed, send an email containing an email confirmation link. <br/>
        /// Note: Users that do not exist will be created automatically.
        /// </summary>
        /// <param name="emails">The emails to invite.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task InviteUsersToTenant(int tenantId, IEnumerable<UserForInvitation> ufis)
        {
            // Note: If the system is integrated with an email service, user emails are automatically
            // confirmed, otherwise users must receive an email invitation to confirm their emails
            if (!_clientProxy.EmailEnabled)
            {
                throw new InvalidOperationException(
                    $"Attempt to call {nameof(EmbeddedIdentityProxy)}.{nameof(EmbeddedIdentityProxy.InviteUsersToTenant)} when email is not enabled.");
            }

            var confirmedUsers = new List<ConfirmedEmailInvitation>();
            var unconfirmedUsers = new List<UnconfirmedEmailInvitation>();

            foreach (var ufi in ufis)
            {
                var email = ufi.Email;
                var idUser = await GetOrCreateUser(email, emailConfirmed: false);

                if (idUser.EmailConfirmed)
                {
                    confirmedUsers.Add(new ConfirmedEmailInvitation
                    {
                        Email = ufi.Email,
                        Name = ufi.Name,
                        InviterName = ufi.InviterName,
                        PreferredLanguage = ufi.PreferredLanguage,
                        CompanyName = ufi.CompanyName
                    });
                }
                else
                {
                    // Generate a email confirmation token and a password reset token for the new user
                    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(idUser);
                    var passwordToken = await _userManager.GeneratePasswordResetTokenAsync(idUser);

                    // Create the email confirmation link pointing to the embedded Identity server
                    // Note: This is safe as long as we use host filtering (set AllowedHosts in configuration)
                    var confirmationLink = _linkGenerator.GetUriByPage(_httpAccessor.HttpContext,
                           page: "/Account/ConfirmEmail",
                           values: new
                           {
                               userId = idUser.Id,
                               code = emailToken,
                               passwordCode = passwordToken,
                               area = "Identity"
                           }
                       );

                    unconfirmedUsers.Add(new UnconfirmedEmailInvitation
                    {
                        Email = ufi.Email,
                        Name = ufi.Name,
                        InviterName = ufi.InviterName,
                        CompanyName = ufi.CompanyName,
                        PreferredLanguage = ufi.PreferredLanguage,

                        EmailConfirmationLink = confirmationLink
                    });
                }
            }

            if (confirmedUsers.Any())
            {
                await _clientProxy.InviteConfirmedUsersToTenant(tenantId, confirmedUsers);
            }

            if (unconfirmedUsers.Any())
            {
                await _clientProxy.InviteUnconfirmedUsersToTenant(tenantId, unconfirmedUsers);
            }
        }

        public async Task InviteUsersToAdmin(IEnumerable<AdminUserForInvitation> ufis)
        {
            // Note: If the system is integrated with an email service, user emails are automatically
            // confirmed, otherwise users must receive an email invitation to confirm their emails
            if (!_clientProxy.EmailEnabled)
            {
                throw new InvalidOperationException(
                    $"Attempt to call {nameof(EmbeddedIdentityProxy)}.{nameof(EmbeddedIdentityProxy.InviteUsersToAdmin)} when email is not enabled.");
            }

            var confirmedUsers = new List<ConfirmedAdminEmailInvitation>();
            var unconfirmedUsers = new List<UnconfirmedAdminEmailInvitation>();

            foreach (var ufi in ufis)
            {
                var email = ufi.Email;
                var idUser = await GetOrCreateUser(email, emailConfirmed: false);

                if (idUser.EmailConfirmed)
                {
                    confirmedUsers.Add(new ConfirmedAdminEmailInvitation
                    {
                        Email = ufi.Email,
                        Name = ufi.Name,
                        InviterName = ufi.InviterName
                    });
                }
                else
                {
                    // Generate a email confirmation token and a password reset token for the new user
                    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(idUser);
                    var passwordToken = await _userManager.GeneratePasswordResetTokenAsync(idUser);

                    // Create the email confirmation link pointing to the embedded Identity server
                    // Note: This is safe as long as we use host filtering (set AllowedHosts in configuration)
                    var confirmationLink = _linkGenerator.GetUriByPage(_httpAccessor.HttpContext,
                           page: "/Account/ConfirmEmail",
                           values: new
                           {
                               userId = idUser.Id,
                               code = emailToken,
                               passwordCode = passwordToken,
                               area = "Identity"
                           }
                       );

                    unconfirmedUsers.Add(new UnconfirmedAdminEmailInvitation
                    {
                        Email = ufi.Email,
                        Name = ufi.Name,
                        InviterName = ufi.InviterName,

                        EmailConfirmationLink = confirmationLink
                    });
                }
            }

            if (confirmedUsers.Any())
            {
                await _clientProxy.InviteConfirmedUsersToAdmin(confirmedUsers);
            }

            if (unconfirmedUsers.Any())
            {
                await _clientProxy.InviteUnconfirmedUsersToAdmin(unconfirmedUsers);
            }
        }

        /// <summary>
        /// Helper function.
        /// </summary>
        private async Task<EmbeddedIdentityServerUser> GetOrCreateUser(string email, bool emailConfirmed)
        {
            var idUser = await _userManager.FindByNameAsync(email) ??
                    await _userManager.FindByEmailAsync(email);

            if (idUser == null)
            {
                // Create the identity user if it doesn't exist
                idUser = new EmbeddedIdentityServerUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = emailConfirmed
                };

                var result = await _userManager.CreateAsync(idUser);
                if (!result.Succeeded)
                {
                    string msg = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(msg);
                }
            }
            return idUser;
        }
    }
}
