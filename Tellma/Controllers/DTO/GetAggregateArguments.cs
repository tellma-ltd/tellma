namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// A DTO that carries the parameters for most Http GET aggregate requests
    /// </summary>
    public class GetAggregateArguments
    {
        /// <summary>
        /// Specifies the number of rows that the server should return
        /// </summary>
        public int Top { get; set; } = 0;

        /// <summary>
        /// An expression string that is compiled into a SQL ORDER BY clause. Atoms are
        /// optionally postfixed with the order direction "asc" or "desc"
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// An expression string that is compiled into a SQL WHERE clause enabling a rich 
        /// query experience
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// An expression string that is compiled into a SQL HAVING clause enabling a rich 
        /// query experience
        /// </summary>
        public string Having { get; set; }

        /// <summary>
        /// Equivalent to linq's "Select", but tailored for an aggregate query, the atoms can be
        /// either aggregated (measures) or not (dimensions)
        /// An aggregated atom encloses every column access in an aggregation function like: Sum(Amount)
        /// A non-aggregated atom, contains no aggregation functions the form "Resource.Name"
        /// </summary>
        public string Select { get; set; }
    }
}
