// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface DocumentDefinitionLineDefinitionForSave extends EntityForSave {
    LineDefinitionId?: number;
    IsVisibleByDefault?: boolean;
}

export interface DocumentDefinitionLineDefinition extends DocumentDefinitionLineDefinitionForSave {
    DocumentDefinitionId?: number;
    SavedById?: number | string;
}
