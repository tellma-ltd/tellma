namespace Tellma.Api.Dto
{
    public class ActionArguments : SelectExpandArguments
    {
        /// <summary>
        /// Specifies that affected entities should be returned
        /// </summary>
        public bool? ReturnEntities { get; set; } = true;
    }
}
