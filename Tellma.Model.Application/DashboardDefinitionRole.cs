using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DashboardDefinitionRole", GroupName = "DashboardDefinitionRoles")]
    public class DashboardDefinitionRoleForSave : EntityWithKey<int>
    {
        [Display(Name = "Definition_Role")]
        [Required, ValidateRequired]
        public int? RoleId { get; set; }
    }

    public class DashboardDefinitionRole : DashboardDefinitionRoleForSave
    {
        [Required]
        public int? DashboardDefinitionId { get; set; }

        [Display(Name = "Definition_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }
    }
}
