// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface AgentDefinitionReportDefinitionForSave extends EntityForSave {
    ReportDefinitionId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
}

export interface AgentDefinitionReportDefinition extends AgentDefinitionReportDefinitionForSave {
    AgentDefinitionId?: number;
    SavedById?: number | string;
}
