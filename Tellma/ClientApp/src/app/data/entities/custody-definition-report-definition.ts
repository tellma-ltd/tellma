// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface CustodyDefinitionReportDefinitionForSave extends EntityForSave {
    ReportDefinitionId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
}

export interface CustodyDefinitionReportDefinition extends CustodyDefinitionReportDefinitionForSave {
    CustodyDefinitionId?: number;
    SavedById?: number | string;
}
