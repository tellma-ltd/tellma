using System;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// A class for transferring basic information about the current company and the currently 
    /// authenticated user, information that is retrieved from the matching <see cref="User"/> 
    /// in the application database.
    /// </summary>
    public class OnConnectResult
    {
        public OnConnectResult(
            int? userId, 
            string email, 
            string externalId,
            Guid? permissionsVersion,
            Guid? userSettingsVersion,
            Guid settingsVersion,
            Guid definitionsVersion)
        {
            UserId = userId;
            Email = email;
            ExternalId = externalId;
            PermissionsVersion = permissionsVersion;
            UserSettingsVersion = userSettingsVersion;
            SettingsVersion = settingsVersion;
            DefinitionsVersion = definitionsVersion;
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
    }
}
