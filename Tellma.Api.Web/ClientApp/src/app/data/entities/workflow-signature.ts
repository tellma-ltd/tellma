import { EntityForSave } from './base/entity-for-save';

export type RuleType =  'ByRole'| 'ByCustodian'| 'ByUser'| 'Public';
export const ruleTypes: RuleType[] = ['ByRole', 'ByCustodian', 'ByUser', 'Public'];

export type PredicateType =  'ValueGreaterOrEqual';
export const predicateTypes: PredicateType[] = ['ValueGreaterOrEqual'];

export interface WorkflowSignatureForSave extends EntityForSave {
    RuleType?: RuleType;
    RuleTypeEntryIndex?: number;
    RoleId?: number;
    UserId?: number;
    PredicateType?: PredicateType;
    PredicateTypeEntryIndex?: number;
    Value?: number;
    ProxyRoleId?: number;
}

export interface WorkflowSignature extends WorkflowSignatureForSave {
    WorkflowId?: number;
    SavedById?: number;
}
