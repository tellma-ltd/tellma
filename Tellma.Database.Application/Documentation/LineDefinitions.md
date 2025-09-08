# LineDefinitions Table Documentation

## Overview
The `LineDefinitions` table defines the structure and behavior of document lines in the system. It specifies how lines should be processed, validated, and displayed in various document types.

## Key Characteristics

1. **Document Types**
   - 20: T for P (Template for Planning)
     - Used for planning and forecasting
     - Template for creating plan documents
   - 40: Plan
     - Planning documents
     - Used for budgeting and financial planning
   - 60: T for E (Template for Event)
     - Template for creating event-based documents
     - Used as a base for event transactions
   - 80: Model
     - Template documents
     - Used as reusable templates
   - 100: Event
     - Actual transactions affecting financial statements
     - Records actual business events
   - 120: Regulatory
     - Transactions for regulatory purposes only
     - Not of interest to management
     - Used for compliance reporting

2. **Multi-language Support**
   - Primary language (TitleSingular/TitlePlural)
   - Secondary language (TitleSingular2/TitlePlural2)
   - Tertiary language (TitleSingular3/TitlePlural3)
   - All translations must be maintained

3. **Scripting Capabilities**
   - GenerateScript: For UI line generation
   - PreprocessScript: For line preprocessing
   - ValidateScript: For line validation
   - SignValidateScript: For signing validation
   - UnsignValidateScript: For unsigning validation

## Purpose
The LineDefinitions table defines the structure and behavior of document lines, including:

### Best Practices for Code Naming
1. **Embed Account Types in Code**
   - Use descriptive prefixes to indicate account types
   - Use To/From to indicate debit/credit direction
   - End with line type indicator (E/M/P)
   - Example: ToCashFromCash.E
     - To: Indicates debit direction
     - From: Indicates credit direction
     - Cash: Account type
     - E: Line type (Event)

2. **Line Type Indicators**
   - E: Event (Transaction)
   - M: Model (Template)
   - P: Plan

### Key Relationships
- Each line definition must have a unique Code
- Supports multiple languages through TitleSingular/Plural fields
- Contains various scripts for different processing stages
- References Users table through SavedById
- Supports system-versioning through ValidFrom/ValidTo columns

### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[Code] NVARCHAR(100),
[LineType] TINYINT,
[TitleSingular] NVARCHAR(100),
[TitlePlural] NVARCHAR(100),
[Description] NVARCHAR(1024)
```
   - Id: Primary key
   - Code: Unique identifier for the line definition
   - LineType: Defines the type of document line
   - TitleSingular/Plural: Display names in primary language
   - Description: Detailed description of the line type

2. **Behavior Fields**
```sql
[AllowSelectiveSigning] BIT,
[ViewDefaultsToForm] BIT,
[BarcodeColumnIndex] INT,
[BarcodeProperty] NVARCHAR(50),
[BarcodeExistingItemHandling] NVARCHAR(50),
[BarcodeBeepsEnabled] BIT
```
   - AllowSelectiveSigning: Controls selective signing capability
   - ViewDefaultsToForm: Controls default view behavior
   - Barcode-related fields: Control barcode scanning behavior

3. **Script Fields**
```sql
[GenerateScript] NVARCHAR(MAX),
[PreprocessScript] NVARCHAR(MAX),
[ValidateScript] NVARCHAR(MAX),
[SignValidateScript] NVARCHAR(MAX),
[UnsignValidateScript] NVARCHAR(MAX)
```
   - GenerateScript: For UI line generation
   - PreprocessScript: For line preprocessing
   - ValidateScript: For line validation
   - SignValidateScript: For signing validation
   - UnsignValidateScript: For unsigning validation

4. **Audit Fields**
```sql
[SavedById] INT,
[ValidFrom] DATETIME2,
[ValidTo] DATETIME2
```
   - SavedById: References Users table
   - ValidFrom/ValidTo: System-versioning timestamps

## Related Tables

### Users
- References for SavedById
- Tracks accountability for record changes

### LineDefinitionEntries
- Defines the specific entries for each line definition
- Controls the structure of document lines

## Best Practices

### Line Type Management
- Use appropriate LineType value based on document purpose
- Keep descriptions clear and consistent
- Use multi-language support for international deployments

### Scripting
- Use GenerateScript for UI-specific logic
- Implement validation in ValidateScript
- Handle signing logic in SignValidateScript
- Keep scripts maintainable and documented

### Barcode Handling
- Configure BarcodeColumnIndex appropriately
- Set proper BarcodeExistingItemHandling behavior
- Enable/disable beeps based on user preference

## Performance Considerations
- Keep scripts optimized for performance
- Use proper indexes on frequently queried columns
- Consider partitioning for large line definition sets
- Cache frequently used line definitions
