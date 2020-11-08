// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface LookupDefinitionReportDefinitionForSave extends EntityForSave {
    ReportDefinitionId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
}

export interface LookupDefinitionReportDefinition extends LookupDefinitionReportDefinitionForSave {
    LookupDefinitionId?: number;
    SavedById?: number | string;
}
