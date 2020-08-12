import { EntityForSave } from './base/entity-for-save';
import { NegativeLineState } from './line';

export interface LineDefinitionStateReasonForSave extends EntityForSave {
    State?: NegativeLineState;
    Name?: string;
    Name2?: string;
    Name3?: string;
    IsActive?: boolean;
}

export interface LineDefinitionStateReason extends LineDefinitionStateReasonForSave {
    LineDefinitionId?: number;
    SavedById?: number;
}
