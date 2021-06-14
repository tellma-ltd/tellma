import { EntityForSave } from './base/entity-for-save';
import { PositiveLineState } from './line';
import { WorkflowSignatureForSave, WorkflowSignature } from './workflow-signature';

export interface WorkflowForSave<TSignature = WorkflowSignatureForSave> extends EntityForSave {
    ToState?: PositiveLineState;
    Signatures?: TSignature[];
}

export interface Workflow extends WorkflowForSave<WorkflowSignature> {
    LineDefinitionId?: number;
    SavedById?: number;
}
