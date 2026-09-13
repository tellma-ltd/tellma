// tslint:disable:variable-name
import { SettingsBase } from './base/settings-base';

export interface GeneralSettingsForSave extends SettingsBase {
    CompanyName: string;
    CompanyName2: string;
    CompanyName3: string;
    CustomFields: Custom;
    CountryCode: string;
    
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
    Enforce2faOnLocalAccounts: boolean;
    EnforceNoExternalAccounts: boolean;
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

export interface Custom {
    BuildingNumber?: string;
    Street?: string;
    Street2?: string;
    Street3?: string;
    SecondaryNumber?: string;
    District?: string;
    District2?: string;
    District3?: string;
    PostalCode?: string;
    City?: string;
    City2?: string;
    City3?: string;
    CommercialRegistrationNumber?: string;

    // Banner
    
    BannerKey?: string;
    BannerIsDismissable?: boolean;
    BannerType?: 'Info' | 'Warning' | 'Error';
    BannerHeight?: number;
    BannerText?: string;
    BannerText2?: string;
    BannerText3?: string;

    // Marmin (UAE e-invoicing over Peppol).
    // NOTE: every one of these must be bound by general-settings.component.html. The server
    // serializes whatever the client sends, so a field the form omits is wiped on save.

    MarminAeClientId?: string;
    MarminAeBusinessProfileId?: string;
    MarminAeOrgId?: string;
    MarminAeEndpointSchemeId?: string;
    MarminAeDefaultProfileExecutionId?: string;
    MarminAeDefaultPaymentMeansCode?: string;
    MarminAeDefaultPaymentTermDays?: number;
}
