namespace Tellma.Api.Dto
{
    /// <summary>
    /// Arguments for the API of reassigning the document to another user.
    /// </summary>
    public class AssignArguments : ActionArguments
    {
        /// <summary>
        /// The new assignee.
        /// </summary>
        public int AssigneeId { get; set; }

        /// <summary>
        /// Comment for the new assignee.
        /// </summary>
        public string Comment { get; set; }
    }
}
