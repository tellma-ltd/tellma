﻿using Tellma.Model.Admin;

namespace Tellma.Repository.Admin
{
    /// <summary>
    /// A class for transferring basic information about the currently authenticated user, information
    /// that is retrieved from the matching <see cref="AdminUser"/> in the admin database.
    /// </summary>
    public class OnConnectOutput
    {
        /// <summary>
        /// The id of the user in the admin database.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// The email of the user in the admin database.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The external Id of the user in the admin database.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// The permissions version of the user in the admin database.
        /// </summary>
        public string PermissionsVersion { get; set; }

        /// <summary>
        /// The settings version of the user in the admin database.
        /// </summary>
        public string UserSettingsVersion { get; set; }
    }
}
