// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface ResourceDefinitionReportDefinitionForSave extends EntityForSave {
    ReportDefinitionId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
}

export interface ResourceDefinitionReportDefinition extends ResourceDefinitionReportDefinitionForSave {
    ResourceDefinitionId?: number;
    SavedById?: number | string;
}
