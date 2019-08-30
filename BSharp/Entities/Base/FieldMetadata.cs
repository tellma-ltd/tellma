namespace BSharp.Entities
{
    /// <summary>
    /// For the metadata of properties of entities
    /// </summary>
    public enum FieldMetadata
    {
        /// <summary>
        /// Requested by the user but the user account does not have read-permissions over this field
        /// </summary>
        Restricted = 1,

        /// <summary>
        /// Requested by the user, and the user has read-permissions over this field
        /// </summary>
        Loaded = 2
    }
}
