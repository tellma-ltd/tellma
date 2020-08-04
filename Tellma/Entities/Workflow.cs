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
        public short? ToState { get; set; }

        [Display(Name = "Workflow_Signatures")]
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
