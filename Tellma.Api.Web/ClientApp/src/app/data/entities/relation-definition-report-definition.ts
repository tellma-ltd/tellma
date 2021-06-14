// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface RelationDefinitionReportDefinitionForSave extends EntityForSave {
    ReportDefinitionId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
}

export interface RelationDefinitionReportDefinition extends RelationDefinitionReportDefinitionForSave {
    RelationDefinitionId?: number;
    SavedById?: number | string;
}
