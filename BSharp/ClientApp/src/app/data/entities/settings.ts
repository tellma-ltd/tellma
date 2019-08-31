import { Entity } from './base/entity';

export class SettingsForSave extends Entity {
    ShortCompanyName: string;
    ShortCompanyName2: string;
    PrimaryLanguageId: string;
    PrimaryLanguageSymbol: string;
    SecondaryLanguageId: string;
    SecondaryLanguageSymbol: string;
    BrandColor: string;
}

export class Settings extends SettingsForSave {
    ViewsAndSpecsVersion: string;
    SettingsVersion: string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

export class SettingsForClient extends Entity {
    ShortCompanyName: string;
    ShortCompanyName2: string;
    PrimaryLanguageId: string;
    PrimaryLanguageName: string;
    PrimaryLanguageSymbol: string;
    SecondaryLanguageId: string;
    SecondaryLanguageName: string;
    SecondaryLanguageSymbol: string;
    TernaryLanguageId: string;
    TernaryLanguageName: string;
    TernaryLanguageSymbol: string;
    BrandColor: string;
    CreatedAt: string;
}
