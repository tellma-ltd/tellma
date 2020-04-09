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
        public AssignmentAttribute() : base(new object[] { 'N', 'A', 'E' },
            new string[] { "Assignment_N", "Assignment_A", "Assignment_E" })
        {
        }
    }
}
