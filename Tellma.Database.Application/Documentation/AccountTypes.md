# AccountTypes Table Documentation

## Overview
The AccountTypes table is a critical component of the Tellma accounting system, serving as the foundation for defining account structures and behaviors. The table is populated with IFRS concepts and includes two main categories:

1. **IFRS Account Types**
   - Filled from IFRS taxonomies for:
     - Balance Sheet
     - Income Statement
     - Related Notes
   - Each row is derived from:
     - Name: Used for account type code and title
     - Concept: Defines the accounting concept
     - Description: Provides detailed explanation

2. **Statistical Account Types**
   - Additional set of rows for statistical accounts
   - Used for non-financial reporting and analysis
   - Maintains separate hierarchy from IFRS accounts
   - Allows tracking of statistical data alongside financial accounts

## Data Sources

### IFRS Taxonomy
- Source of truth for financial account types
- Maintains consistency with international accounting standards
- Updated regularly to align with latest IFRS changes
- Includes:
  - Balance Sheet items
  - Income Statement items
  - Related disclosure notes
  - Supporting schedules

### Statistical Accounts
- Added to support non-financial reporting
- Used for:
  - Operational metrics
  - Performance indicators
  - Statistical analysis
  - Regulatory reporting
- Maintains separate hierarchy from financial accounts
- Allows flexible reporting alongside financial data

## Key Characteristics

1. **System Criticality**
   - This table should only be modified by experienced developers
   - Modifications can have significant impact on the accounting system
   - Changes must be carefully considered and tested

2. **Pre-populated Content**
   - Contains IFRS-compliant account type definitions
   - Maintains consistency with international accounting standards
   - Should not be modified without thorough understanding of IFRS implications

## Table Structure

### Basic Information
```sql
CREATE TABLE [dbo].[AccountTypes] (
    [Id] INT CONSTRAINT [PK_AccountTypes] PRIMARY KEY IDENTITY,
    [Code] NVARCHAR(50) NOT NULL CONSTRAINT [UQ_AccountTypes__Code] UNIQUE,
    [TitleSingular] NVARCHAR(100),
    [TitleSingular2] NVARCHAR(100),
    [TitleSingular3] NVARCHAR(100),
    [TitlePlural] NVARCHAR(100),
    [TitlePlural2] NVARCHAR(100),
    [TitlePlural3] NVARCHAR(100),
    [ParentId] INT CONSTRAINT [FK_AccountTypes__ParentId] REFERENCES [dbo].[AccountTypes]([Id])
)
```

### Key Properties
```sql
[IsMonetary] BIT DEFAULT 1,
[IsAssignable] BIT NOT NULL DEFAULT 1,
[StandardAndPure] BIT DEFAULT 0,
[EntryTypeParentId] INT REFERENCES [dbo].[EntryTypes]([Id]),
[Code] NVARCHAR(50) NOT NULL,
[IsSystem] BIT NOT NULL DEFAULT 0,
```

### Field Usage

1. **Account Type Code**
   - Used for sorting in UI tree view
   - Controls display order of account types
   - Should be carefully managed to maintain proper hierarchy

2. **IsMonetary**
   - When set to 1:
     - Entry is included in exchange gain/loss calculations
     - Affects currency conversion processing
     - Critical for international accounting

3. **IsSystem**
   - When set to 1:
     - Added by Tellma designers
     - Part of core system functionality
     - Should not be modified
   - When set to 0:
     - Added by implementation team developers
     - Custom additions for specific implementations
     - Can be modified as needed

4. **IFRS Mapping**
   - Account type Name comes from Standard Label field in IFRS
   - Account type Description comes from Documentation field in IFRS
   - Maintains consistency with IFRS standards
   - Used for accurate financial reporting

2. **IsAssignable**
   - When set to 1:
     - Account type can be assigned to actual accounts
   - When set to 0:
     - Acts as a parent node only
     - Cannot be assigned to accounts

3. **StandardAndPure**
   - Currently not used in the system
   - Reserved for future functionality

4. **EntryTypeParentId**
   - Defines compatible entry types in table Entries
   - Controls which entry types can be used with accounts of this type
   - Maintains transaction type consistency

### Entry Field Labels

1. **Time Fields**
```sql
[Time1Label] NVARCHAR(50),
[Time2Label] NVARCHAR(50),
```
   - Time1Label and Time2Label control visibility of time fields in table Entries
   - Fields are visible only if their labels are not null
   - Used for time-related accounting entries
   - **Note**: The actual meaning of Time1 and Time2 fields is defined in the Entries table documentation

2. **Reference Fields**
```sql
[ExternalReferenceLabel] NVARCHAR(50),
[ReferenceSourceLabel] NVARCHAR(50),
[InternalReferenceLabel] NVARCHAR(50),
```
   - Controls visibility of reference fields in table Entries
   - Fields are visible only if their labels are not null
   - Used for tracking external and internal references
   - **Note**: The actual meaning of ReferenceSource and InternalReference fields is defined in the Entries table documentation

3. **Noted Fields**
```sql
[NotedAgentNameLabel] NVARCHAR(50),
[NotedAmountLabel] NVARCHAR(50),
[NotedDateLabel] NVARCHAR(50)
```
   - Controls visibility of noted fields in table Entries
   - Fields are visible only if their labels are not null
   - Used for tracking additional information in entries
   - **Note**: The actual meaning of NotedAgentName, NotedAmount and NotedDate fields is defined in the Entries table documentation

## Relationship with Accounts Table
- Each account in the Accounts table must have an AccountType
- AccountType defines:
  - What information can be stored in the account
  - What information is shown in entries when using the account
  - Which entry types are compatible with the account
  - Which agent types are allowed (via AccountTypeAgentDefinitions)

### How It Works in Entries
1. Entries table uses Accounts directly
2. When creating an entry:
   - The system looks up the Account's AccountType
   - Uses AccountType to determine:
     - Which fields are visible
     - Which entry types are allowed
     - Which agent types are valid (via AccountTypeAgentDefinitions)
   - Enforces these rules during entry creation

## Best Practices

1. **Modification Guidelines**
   - Only modify when absolutely necessary
   - Always consult IFRS standards
   - Test thoroughly before deployment
   - Document all changes

2. **Hierarchy Management**
   - Use ParentId to maintain proper hierarchy
   - Set IsAssignable = 0 for parent nodes
   - Keep hierarchy consistent with IFRS structure

3. **Entry Type Compatibility**
   - Set appropriate EntryTypeParentId
   - Ensure compatibility with account usage
   - Maintain consistent transaction patterns

4. **Field Visibility**
   - Use labels to control field visibility
   - Follow IFRS reporting requirements
   - Keep visibility consistent across similar accounts

## System Features

1. **Multi-language Support**
   - All title fields support multiple languages
   - Labels support multiple languages
   - Consider regional requirements

2. **System Versioning**
   - All changes are tracked
   - History available in AccountTypesHistory
   - Maintain versioning for audit purposes

3. **State Management**
   - IsActive: Controls account type availability
   - IsSystem: Marks system-defined types
   - ChildCount: Tracks number of child types
   - ActiveChildCount: Tracks number of active children

4. **Audit Trail**
   - SavedById: Records who made changes
   - ValidFrom/ValidTo: Tracks change periods
   - System versioning enabled for full history
