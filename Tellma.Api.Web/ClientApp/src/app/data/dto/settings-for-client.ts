import { Calendar, HmsFormat, YmdFormat } from '../entities/base/metadata-types';

// tslint:disable:variable-name
export interface SettingsForClient {
    ShortCompanyName: string;
    ShortCompanyName2: string;
    ShortCompanyName3: string;
    FunctionalCurrencyId: string;
    FunctionalCurrencyDecimals: number; // decimal places of the functional currency
    FunctionalCurrencyName: string;
    FunctionalCurrencyName2: string;
    FunctionalCurrencyName3: string;
    FunctionalCurrencyDescription: string;
    FunctionalCurrencyDescription2: string;
    FunctionalCurrencyDescription3: string;
    ArchiveDate: string;
    FreezeDate: string;
    TaxIdentificationNumber?: string;
    PrimaryLanguageId: string;
    PrimaryLanguageName: string;
    PrimaryLanguageSymbol: string;
    SecondaryLanguageId: string;
    SecondaryLanguageName: string;
    SecondaryLanguageSymbol: string;
    TernaryLanguageId: string;
    TernaryLanguageName: string;
    TernaryLanguageSymbol: string;
    PrimaryCalendar: Calendar;
    SecondaryCalendar: Calendar;
    DateFormat: YmdFormat;
    TimeFormat: HmsFormat;
    BrandColor: string;
    CreatedAt: string;
    SingleBusinessUnitId?: number;
    SmsEnabled: boolean;

    FeatureFlags: { [key: string]: boolean };

    ZatcaEnvironment: 'Sandbox' | 'Simulation' | 'Production';

    // Banner

    BannerKey?: string;
    BannerIsDismissable?: boolean;
    BannerType?: 'Info' | 'Warning' | 'Error';
    BannerHeight?: number;
    BannerText?: string;
    BannerText2?: string;
    BannerText3?: string;
}
