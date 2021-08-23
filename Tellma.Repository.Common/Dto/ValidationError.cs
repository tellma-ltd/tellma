namespace Tellma.Repository.Common
{
    /// <summary>
    /// The .NET version of [dbo].[ValidationErrorList], used for storing validation
    /// errors that are returned from SQL validation stored procedures.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// The path to the invalid property.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The key of the translation resource of the error message.
        /// </summary>
        public string ErrorName { get; set; }

        /// <summary>
        /// Replaces "{0}" in the error message.
        /// </summary>
        public string Argument1 { get; set; }

        /// <summary>
        /// Replaces "{1}" in the error message.
        /// </summary>
        public string Argument2 { get; set; }

        /// <summary>
        /// Replaces "{2}" in the error message.
        /// </summary>
        public string Argument3 { get; set; }

        /// <summary>
        /// Replaces "{3}" in the error message.
        /// </summary>
        public string Argument4 { get; set; }

        /// <summary>
        /// Replaces "{4}" in the error message.
        /// </summary>
        public string Argument5 { get; set; }
    }
}
