namespace Tellma.Controllers.Dto
{
    public class ActionArguments
    {
        /// <summary>
        /// Specifies that affected entities should be returned
        /// </summary>
        public bool? ReturnEntities { get; set; } = true;

        /// <summary>
        /// Specifies what navigation properties to expand in the returned entities
        /// (if <see cref="ReturnEntities"/> is set to false
        /// this parameter will be ignored
        /// </summary>
        public string Expand { get; set; }

        /// <summary>
        /// Specifies what properties to select in the returned entities
        /// (if <see cref="ReturnEntities"/> is set to false
        /// this parameter will be ignored
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
