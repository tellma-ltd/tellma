namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// DTO that carries the parameters for most Http GET aggregate requests
    /// </summary>
    public class GetAggregateArguments
    {
        /// <summary>
        /// Specifies the number of items the server should return
        /// </summary>
        public int Top { get; set; } = 0;
        
        /// <summary>
        /// An OData style filter string that is parsed into a SQL WHERE-Clause enabling a rich 
        /// query experience
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Equivalent to linq's "Select", but tailored for an aggregate query, the atoms can be
        /// either aggregated or not
        /// An aggregated atom, takes the form "sum(Amount) desc"
        /// A non-aggregated atom, takes the form "Resource/Name asc"
        /// atoms are also optionally postfixed with the order direction "asc" or "desc"
        /// </summary>
        public string Select { get; set; }
    }
}
