namespace BSharp.Data
{
    /// <summary>
    /// A class for storing basic information about the currently authenticated user, information
    /// that is retrieved from the matching <see cref="Entities.User"/> in the tenant database
    /// </summary>
    public class UserInfo
    {
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Email { get; set; }
        public string ExternalId { get; set; }
        public string PermissionsVersion { get; set; }
        public string UserSettingsVersion { get; set; }
    }
}
