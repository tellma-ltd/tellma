namespace BSharp.Controllers.Dto
{
    public class ActionArguments
    {
        /// <summary>
        /// Specifies that affected entities should be returned
        /// </summary>
        public bool? ReturnEntities { get; set; } = true;

        /// <summary>
        /// Specifies what navigation properties to expand in the returned entities
        /// (if <see cref="ActionArguments.ReturnEntities"/> is set to false
        /// this parameter will be ignored
        /// </summary>
        public string Expand { get; set; }
    }
}
