using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Model.Application
{
    // Lines are weak entities that only appear attached to a document
    // But if we wanted to query lines directly, we use this type instead.
    public class LineForQuery : Line
    {
        [Display(Name = "Line_Document")]
        [ForeignKey(nameof(DocumentId))]
        public Document Document { get; set; }

        [Display(Name = "Line_Employee")]
        [ForeignKey(nameof(EmployeeId))]
        public Agent Employee { get; set; }

        [Display(Name = "Line_Customer")]
        [ForeignKey(nameof(CustomerId))]
        public Agent Customer { get; set; }

        [Display(Name = "Line_Supplier")]
        [ForeignKey(nameof(SupplierId))]
        public Agent Supplier { get; set; }
    }
}
