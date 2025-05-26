# Settings Table

## Purpose
The `Settings` table stores configuration settings for each company in the Tellma multi-company system. Each company has its own row in this table, containing settings related to:
- Company information
- Language and calendar preferences
- Financial settings
- Security policies
- Zatca compliance (for KSA tenants)

## Key Features

### 1. Multi-Language Support
- Supports up to 3 languages for the user interface
- Current translations available:
  - Arabic
  - Amharic
  - Oromo
  - Chinese
- Additional translation tables can be easily added
- Each language has its own symbol for identification

### 2. Calendar Support
- Supports 3 calendar systems:
  - Gregorian Calendar (GC)
  - Hijri (Umm ALQura) Calendar
  - Ethiopian Calendar (13 months)
- Primary calendar is required
- Secondary calendar is optional

### 3. Date and Time Formatting
- Customizable date format (default: 'yyyy-MM-dd')
- Customizable time format (default: 'HH:mm:ss')
- Formats are used throughout the user interface

### 4. Branding
- `BrandColor`: Specifies the color for the top band of the UI
- Uses 7-character color code format

### 5. Version Control
- Multiple version columns to track changes:
  - `DefinitionsVersion`
  - `SettingsVersion`
  - `SchedulesVersion`
- Each version has its own modification tracking:
  - ModifiedAt
  - ModifiedById

## Financial Settings

### 1. Currency and Tax
- `FunctionalCurrencyId`: Primary currency for the company
- `TaxIdentificationNumber`: Company's tax identification number

### 2. Date Controls
- `ArchiveDate`: 
  - All transactions before this date must be posted
  - No new transactions can be added before this date
  - No existing transactions can be edited or deleted before this date
- `FreezeDate`:
  - Prevents adding new transactions before this date
  - Existing transactions can still be posted or canceled
  - Note: `FirstDayOfPeriod` is no longer used

## Security Settings

### 1. Login Policies
- `Enforce2faOnLocalAccounts`: Requires 2FA for local accounts
- `EnforceNoExternalAccounts`: Prevents external account usage
- SMS functionality can be enabled/disabled from admin console

### 2. Zatca Compliance
- Special settings for KSA tenants to comply with e-Invoice requirements
- Includes encrypted secret and security token handling
- Supports different environments: Sandbox, Simulation, Production

## Company Information

- Multiple language support for company names:
  - `CompanyName`
  - `CompanyName2`
  - `CompanyName3`
- Short company names for display:
  - `ShortCompanyName`
  - `ShortCompanyName2`
  - `ShortCompanyName3`
- Country code for company location
- Custom fields JSON for additional company information

## Note
The table is designed to be extensible, allowing for additional columns to be added as needed (up to 30,000 columns).
- SupportEmails: Semi-colon separated list of email addresses that will receive copies of any error messages faced by users
