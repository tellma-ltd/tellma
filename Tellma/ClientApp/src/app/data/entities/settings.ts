// tslint:disable:variable-name
import { Entity } from './base/entity';

export class SettingsForSave extends Entity {
    ShortCompanyName: string;
    ShortCompanyName2: string;
    ShortCompanyName3: string;
    PrimaryLanguageId: string;
    PrimaryLanguageSymbol: string;
    SecondaryLanguageId: string;
    SecondaryLanguageSymbol: string;
    TernaryLanguageId: string;
    TernaryLanguageSymbol: string;
    BrandColor: string;
}

export class Settings extends SettingsForSave {
    DefinitionsVersion: string;
    SettingsVersion: string;
    ModifiedAt: string;
    ModifiedById: number | string;
}
