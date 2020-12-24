import { EntityForSave } from './base/entity-for-save';
import { DefinitionVisibility } from './base/definition-common';
import { Control } from './base/metadata';

export interface LineDefinitionGenerateParameterForSave extends EntityForSave {
    Key?: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
    Visibility?: DefinitionVisibility;
    Control?: Control;
    ControlOptions?: string; // JSON
}

export interface LineDefinitionGenerateParameter extends LineDefinitionGenerateParameterForSave {
    LineDefinitionId?: number;
    SavedById?: number;
}
