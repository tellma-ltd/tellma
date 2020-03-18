namespace Tellma.Data
{
    /// <summary>
    /// One class to store permissions from either the application or the admin database
    /// </summary>
    public class AbstractPermission
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

        /// <summary>
        /// "UnitType,BaseAmount"
        /// </summary>
        public string Mask { get; set; }
    }
}
