namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// One class to transfer permissions from either the application or the admin database.
    /// </summary>
    public class UserPermission
    {
        /// <summary>
        /// "units"
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// "Read", "Update", etc...
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// "UnitType eq 'Distance'"
        /// </summary>
        public string Criteria { get; set; }
    }
}
