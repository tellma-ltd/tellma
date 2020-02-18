namespace Tellma.Data
{
    /// <summary>
    /// A class for storing basic information about the currently authenticated user, information
    /// that is retrieved from the matching <see cref="Entities.AdminUser"/> in the admin database
    /// </summary>
    public class AdminUserInfo
    {
        public int? UserId { get; set; }
        public string Email { get; set; }
        public string ExternalId { get; set; }
        public string PermissionsVersion { get; set; }
        public string UserSettingsVersion { get; set; }
    }
}
