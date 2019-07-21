using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class AgentForSave : CustodyForSave
    {
        [Display(Name = "Agent_IsRelated")]
        public bool? IsRelated { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Agent_TaxIdentificationNumber")]
        public string TaxIdentificationNumber { get; set; }

        [BasicField]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Agent_Title", Language = Language.Primary)]
        public string Title { get; set; }

        [BasicField]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Agent_Title", Language = Language.Secondary)]
        public string Title2 { get; set; }

        [ChoiceList(new object[] { 'M', 'F' }, new string[] { "Agent_Male", "Agent_Female" })]
        [Display(Name = "Agent_Gender")]
        public char? Gender { get; set; }
    }

    public class Agent : AgentForSave // but also agentforsave 
    {
        // Agent/Place
        [Display(Name = "Custody_CustodyType")]
        public string CustodyType { get; set; }

        ////// The properties below come from  Custody, since the 
        ////// same class cannot inherit from 2 base classes

        // Individual/Organization
        [BasicField]
        [Display(Name = "Agent_AgentType")]
        public string AgentType { get; set; }

        [BasicField]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [ForeignKey]
        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [ForeignKey]
        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [NavigationProperty(ForeignKey = nameof(CreatedById))]
        public LocalUser CreatedBy { get; set; }

        [NavigationProperty(ForeignKey = nameof(ModifiedById))]
        public LocalUser ModifiedBy { get; set; }
    }
}
