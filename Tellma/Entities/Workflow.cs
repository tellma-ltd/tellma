using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "Workflow", Plural = "Workflows")]
    public class WorkflowForSave<TWorkflowSignature> : EntityWithKey<int>
    {
        [Display(Name = "Workflow_ToState")]
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
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
