// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface DocumentDefinitionMarkupTemplateForSave extends EntityForSave {
    MarkupTemplateId?: number;
}

export interface DocumentDefinitionMarkupTemplate extends DocumentDefinitionMarkupTemplateForSave {
    DocumentDefinitionId?: number;
    SavedById?: number | string;
}
