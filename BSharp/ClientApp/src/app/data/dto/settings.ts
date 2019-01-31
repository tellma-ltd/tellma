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
