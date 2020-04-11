using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Tellma.Entities
{
    /// <summary>
    /// For all the "Assignment" fields in <see cref="AccountTypeForSave"/>
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class AssignmentAttribute : ChoiceListAttribute
    {
        public AssignmentAttribute(AssignmentType type = AssignmentType.Optional) : base(
            type == AssignmentType.Optional ? new object[] { 'N', 'A', 'E' } :
            type == AssignmentType.Required ? new object[] { 'A', 'E' } :
            type == AssignmentType.EntryOnly ? new object[] { 'N', 'E' } : throw new Exception("Unknown AssignmentType"),
            type == AssignmentType.Optional ? new string[] { "Assignment_N", "Assignment_A", "Assignment_E" } :
            type == AssignmentType.Required ? new string[] { "Assignment_A", "Assignment_E" } :
            type == AssignmentType.EntryOnly ? new string[] { "Assignment_N", "Assignment_E" } : throw new Exception("Unknown AssignmentType"))
        {
        }
    }

    public enum AssignmentType
    {
        Optional, Required, EntryOnly
    }
}
