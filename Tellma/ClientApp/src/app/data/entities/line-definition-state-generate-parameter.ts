import { EntityForSave } from './base/entity-for-save';
import { DefinitionVisibility } from './base/definition-common';

export interface LineDefinitionGenerateParameterForSave extends EntityForSave {
    Key?: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
    Visibility?: DefinitionVisibility;
    DataType?: string; // TODO: Use better type
    Filter?: string;
}

export interface LineDefinitionGenerateParameter extends LineDefinitionGenerateParameterForSave {
    LineDefinitionId?: number;
    SavedById?: number;
}
