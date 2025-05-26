# Currencies Table

## Purpose
The `Currencies` table stores information about different currencies used in the system. Each currency is uniquely identified by its ISO 4217 code and includes support for multiple languages and descriptions.

## Key Features

### 1. Currency Identification
- `Id`: 3-character ISO 4217 currency code (e.g., USD, EUR, GBP)
- `NumericCode`: Unique numeric code for the currency
- Both `Id` and `NumericCode` must be unique across all currencies

### 2. Multi-Language Support
- Multiple language support for currency names:
  - `Name`: Primary name
  - `Name2`: Secondary name
  - `Name3`: Tertiary name
- All names must be unique across all currencies
- Each name is limited to 50 characters

### 3. Descriptions
- Multiple language support for currency descriptions:
  - `Description`: Primary description
  - `Description2`: Secondary description
  - `Description3`: Tertiary description
- All descriptions must be unique across all currencies
- Each description is limited to 255 characters

### 4. Decimal Places
- `E`: Number of decimal places for the currency
- Valid values are: 0, 2, 3 (restricted by ChoiceList attribute)
- Represents the number of decimal places used in calculations and display
- Display name: Currency_DecimalPlaces

### 5. Status
- `IsActive`: Controls whether the currency is active in the system
- Default value is 0 (inactive)
- Inactive currencies cannot be used in transactions

### 6. Audit Tracking
- Complete audit trail with:
  - `CreatedAt`
  - `CreatedById`
  - `ModifiedAt`
  - `ModifiedById`
- All timestamps are stored with timezone information (DATETIMEOFFSET)

## Usage

1. **Currency Management**
   - Add new currencies with their ISO codes and numeric codes
   - Support multiple language names and descriptions
   - Configure decimal places for calculations (0, 2, or 3)
   - Control currency activation status
   - Track creation and modification history

2. **Validation**
   - All required fields use both Required and ValidateRequired attributes
   - Unique currency codes (both alpha and numeric)
   - Unique names and descriptions
   - Valid decimal place configurations (0, 2, or 3)
   - Active/inactive status enforcement

3. **Entity Relationships**
   - Tracks who created and modified each currency
   - Relationships with User table for audit tracking
   - All audit fields are required

4. **Multi-Language Support**
   - Support up to 3 language versions for names
   - Support up to 3 language versions for descriptions
   - Names and descriptions are required in the primary language
   - Display names are internationalized

2. **Validation**
   - Unique currency codes (both alpha and numeric)
   - Unique names and descriptions
   - Valid decimal place configurations
   - Active/inactive status enforcement

3. **Multi-Language Support**
   - Support up to 3 language versions for names
   - Support up to 3 language versions for descriptions
   - Names and descriptions are required in the primary language

## Technical Details

1. **Constraints**
   - Primary key on `Id`
   - Unique constraints on:
     - `Name`
     - `Description`
     - `NumericCode`
   - Check constraint on `E`: Must be one of (0, 2, 3)
   - Foreign key constraints for audit fields

2. **Validation**
   - All required fields use both Required and ValidateRequired attributes
   - ChoiceList validation for decimal places
   - Internationalized display names

2. **Default Values**
   - `E`: 2 decimal places
   - `IsActive`: 0 (inactive)
   - Timestamps: SYSDATETIMEOFFSET()
   - Decimal places: 2

## Note
The table is designed to support internationalization with multi-language support for both names and descriptions, while maintaining strict validation of currency codes and decimal place configurations.
