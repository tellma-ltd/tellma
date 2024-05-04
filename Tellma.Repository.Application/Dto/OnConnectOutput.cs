using System;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// A class for transferring basic information about the current company and the currently 
    /// authenticated user, information that is retrieved from the matching <see cref="User"/> 
    /// in the application database.
    /// </summary>
    public class OnConnectOutput
    {
        private static readonly OnConnectOutput _empty = new();

        public static OnConnectOutput Empty => _empty;

        public OnConnectOutput(
            int? userId, 
            string email, 
            string externalId,
            Guid? permissionsVersion,
            Guid? userSettingsVersion,
            Guid settingsVersion,
            Guid definitionsVersion,
            bool enforce2faOnLocalAccounts,
            bool enforceNoExternalAccounts)
        {
            UserId = userId;
            Email = email;
            ExternalId = externalId;
            PermissionsVersion = permissionsVersion;
            UserSettingsVersion = userSettingsVersion;
            SettingsVersion = settingsVersion;
            DefinitionsVersion = definitionsVersion;
            Enforce2faOnLocalAccounts = enforce2faOnLocalAccounts;
            EnforceNoExternalAccounts = enforceNoExternalAccounts;
        }

        private OnConnectOutput()
        {
        }

        /// <summary>
        /// The id of the user in the application database.
        /// </summary>
        public int? UserId { get; }

        /// <summary>
        /// The email of the user in the application database
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// The external Id of the user in the application database.
        /// </summary>
        public string ExternalId { get; }

        /// <summary>
        /// The permissions version of the user in the application database.
        /// </summary>
        public Guid? PermissionsVersion { get; }

        /// <summary>
        /// The settings version of the user in the application database.
        /// </summary>
        public Guid? UserSettingsVersion { get; }

        /// <summary>
        /// The settings version of the company in the application database.
        /// </summary>
        public Guid SettingsVersion { get; }

        /// <summary>
        /// The definitions version of the company in the application database.
        /// </summary>
        public Guid DefinitionsVersion { get; }

        /// <summary>
        /// All users accessing this company should have 2FA enabled on their local account.
        /// </summary>
        public bool Enforce2faOnLocalAccounts { get; }

        /// <summary>
        /// All users accessing this company should not have any linked external accounts.
        /// </summary>
        public bool EnforceNoExternalAccounts { get; }
    }
}
