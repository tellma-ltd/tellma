using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class ReportDefinitionRoleForSave : EntityWithKey<int>
    {
        [Display(Name = "Definition_Role")]
        [Required, ValidateRequired]
        public int? RoleId { get; set; }
    }

    public class ReportDefinitionRole : ReportDefinitionRoleForSave
    {
        [Required]
        public int? ReportDefinitionId { get; set; }

        [Display(Name = "Definition_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }
    }
}
