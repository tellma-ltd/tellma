namespace Tellma.Data
{
    /// <summary>
    /// A class for storing validation errors that are returned from SQL validation stored procedures
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// The path to the error property
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The key of the translation resource of the error
        /// </summary>
        public string ErrorName { get; set; }

        public string Argument1 { get; set; }

        public string Argument2 { get; set; }

        public string Argument3 { get; set; }

        public string Argument4 { get; set; }

        public string Argument5 { get; set; }
    }
}
