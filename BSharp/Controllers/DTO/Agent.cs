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

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Agent_Title")]
        public string Title { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Agent_Title2")]
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
        [Display(Name = "Agent_AgentType")]
        public string AgentType { get; set; }

        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }
    }
}
