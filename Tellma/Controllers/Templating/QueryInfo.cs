using System;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Base class for a bunch of immutable data structures that each encode all the information about
    /// an intended invocation to one of the API methods in <see cref="ServiceBase"/> that access the database.
    /// This class is immutable and behave like structs when comparing them to one another using <see cref="Equals(object)"/> 
    /// (implements <see cref="Equals(object)"/> and <see cref="GetHashCode()"/>).
    /// <see cref="QueryInfo"/> objects are used to group all API calls in all template expressions, calculate the 
    /// SELECT parameter of each API call so that one API call per <see cref="QueryInfo"/> is required to generate the output
    /// </summary>
    public abstract class QueryInfo
    {
        public QueryInfo(string collection, int? definitionId)
        {
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            DefinitionId = definitionId;
        }

        /// <summary>
        /// Determines which <see cref="ServiceBase"/> to resolve and call. E.g. "Document"
        /// </summary>
        public string Collection { get; }

        /// <summary>
        /// For definitioned collections, this determines which definition (if any) to set on the resolved <see cref="ServiceBase"/>
        /// </summary>
        public int? DefinitionId { get; }

        // This object implements equality like a struct. The purpose is to be able to use it in a dictionary to efficiently group
        // all select paths together from all the different API calls in a template

        #region Equality Comparison

        private string _code;

        private string Code => _code ??= Encode();

        /// <summary>
        /// Bijectively maps the <see cref="QueryInfo"/> to a string code that is used to
        /// implement struct-like comparison behavior with other <see cref="QueryInfo"/>s
        /// </summary>
        protected abstract string Encode();

        public override bool Equals(object obj)
        {
            return obj is QueryInfo q && q.Code == Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        #endregion
    }
}
