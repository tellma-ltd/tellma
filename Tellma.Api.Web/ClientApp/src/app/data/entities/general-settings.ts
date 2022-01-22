// tslint:disable:variable-name
import { SettingsBase } from './base/settings-base';

export interface GeneralSettingsForSave extends SettingsBase {
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
    SupportEmails: string;
}

export interface GeneralSettings extends GeneralSettingsForSave {
    DefinitionsVersion: string;
    SettingsVersion: string;
    SmsEnabled: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}
