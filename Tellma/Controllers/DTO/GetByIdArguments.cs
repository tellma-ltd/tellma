namespace Tellma.Controllers.Dto
{
    public class GetByIdArguments
    {
        /// <summary>
        /// Equivalent to linq's "Include", determines which related entities to include in 
        /// the result, if left empty it means retrieve all properties
        /// </summary>
        public string Expand { get; set; }

        /// <summary>
        /// Equivalent to linq's "Select", determines which properties of the principal entities
        /// or of the included related entities to return the result. If left empty then all
        /// properties of the principalentity and included entities are returned
        /// </summary>
        public string Select { get; set; }

        /// <summary>
        /// Each controller may associate some keywords with canned select parameters, so the API request
        /// can supply only the keyword (instead of the full select param), if the controller recognizes 
        /// the supplied keyword, the <see cref="Select"/> and <see cref="Expand"/> arguments are ignored
        /// </summary>
        public string SelectTemplate { get; set; }
    }
}
