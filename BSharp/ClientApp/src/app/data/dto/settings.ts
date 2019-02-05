import { DtoBase } from './dto-base';

export class SettingsForSave extends DtoBase {
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
    ProvisionedAt: string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

export class SettingsForClient extends DtoBase {
    ShortCompanyName: string;
    ShortCompanyName2: string;
    PrimaryLanguageId: string;
    PrimaryLanguageName: string;
    PrimaryLanguageSymbol: string;
    SecondaryLanguageId: string;
    SecondaryLanguageName: string;
    SecondaryLanguageSymbol: string;
    BrandColor: string;
    ProvisionedAt: string;
}
