using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class PrintingTemplateRoleForSave : EntityWithKey<int>
    {
        [Display(Name = "Definition_Role")]
        [Required, ValidateRequired]
        public int? RoleId { get; set; }
    }

    public class PrintingTemplateRole : PrintingTemplateRoleForSave
    {
        [Required]
        public int? PrintingTemplateId { get; set; }

        [Display(Name = "Definition_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }
    }
}
