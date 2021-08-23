using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Workflow", GroupName = "Workflows")]
    public class WorkflowForSave<TWorkflowSignature> : EntityWithKey<int>
    {
        [Display(Name = "Workflow_ToState")]
        [Required]
        [ChoiceList(new object[] {
            LineState.Requested,
            LineState.Authorized,
            LineState.Completed,
            LineState.Posted
        },
            new string[] {
            LineStateName.Requested,
            LineStateName.Authorized,
            LineStateName.Completed,
            LineStateName.Posted
        })]
        public short? ToState { get; set; }

        [Display(Name = "Workflow_Signatures")]
        [ForeignKey(nameof(WorkflowSignature.WorkflowId))]
        public List<TWorkflowSignature> Signatures { get; set; }
    }

    public class WorkflowForSave : WorkflowForSave<WorkflowSignatureForSave>
    {
    }

    public class Workflow : WorkflowForSave<WorkflowSignature>
    {
        [Display(Name = "Workflow_LineDefinition")]
        [Required]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
