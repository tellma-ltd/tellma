using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    // Lines are a weak entity that only appears attached to a document
    // But if we wanted to query lines directly, we use this type instead
    [StrongEntity]
    public class LineForQuery : Line
    {
        [Display(Name = "Line_Document")]
        [ForeignKey(nameof(DocumentId))]
        public Document Document { get; set; }
    }
}
