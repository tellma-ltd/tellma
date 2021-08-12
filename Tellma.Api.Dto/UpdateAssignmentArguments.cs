namespace Tellma.Api.Dto
{
    /// <summary>
    /// Arguments for the API of updating the assignment comment.
    /// </summary>
    public class UpdateAssignmentArguments : ActionArguments
    {
        /// <summary>
        /// The Id of the assignment.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// New comment
        /// </summary>
        public string Comment { get; set; }
    }
}
