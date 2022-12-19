using System.Collections.Generic;
using Tellma.Api.Behaviors;

namespace Tellma.Api
{
    /// <summary>
    /// Signifies a bug in one of the custom SQL scripts
    /// </summary>
    public class CustomScriptErrorLogEntry : TenantLogEntry
    {
        public CustomScriptErrorLogEntry() : base(TenantLogLevel.Error)
        {
        }

        /// <summary>
        /// Which entity caused a problem, e.g. "Document" or "Resource" etc...
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// The Id of the definition that has the problematic script.
        /// </summary>
        public int? DefinitionId { get; set; }

        /// <summary>
        /// The name of the definition that has the problematic script.
        /// </summary>
        public string DefinitionName { get; set; }

        /// <summary>
        /// Which script threw the error, e.g. "Preprocess Script".
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// Optional Id of the entity on which the operation was
        /// performed (if the operation was done on a single entity).
        /// </summary>
        public IEnumerable<int> EntityIds { get; set; }

        /// <summary>
        /// The email of the user who triggered the error.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// The username of the user who triggered the error.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The SQL error number, should be between 0 and 49,999.
        /// </summary>
        public int ErrorNumber { get; set; }

        /// <summary>
        /// The message of the SQL script error.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
