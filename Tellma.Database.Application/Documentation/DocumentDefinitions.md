# DocumentDefinitions Table Documentation

## Overview
The DocumentDefinitions table defines screens and document types in the Tellma application. It is managed by Tellma implementation partner (e.g., Banan IT) and is essential for defining document types across the system.

## Table Structure

### Primary Key and Basic Information
```sql
CREATE TABLE [dbo].[DocumentDefinitions] (
    [Id] INT CONSTRAINT [PK_DocumentDefinitions] PRIMARY KEY IDENTITY,
    [Code] NVARCHAR(50) CONSTRAINT [UQ_DocumentDefinitions__Code] UNIQUE NOT NULL,
    [IsOriginalDocument] BIT DEFAULT 1 NOT NULL,
    [Description] NVARCHAR(1024) NOT NULL,
    [Description2] NVARCHAR(1024),
    [Description3] NVARCHAR(1024),
    [TitleSingular] NVARCHAR(50) NOT NULL,
    [TitleSingular2] NVARCHAR(50),
    [TitleSingular3] NVARCHAR(50),
    [TitlePlural] NVARCHAR(50) NOT NULL,
    [TitlePlural2] NVARCHAR(50),
    [TitlePlural3] NVARCHAR(50)
)
```

### UI Specifications
```sql
[SortKey] DECIMAL(9,4),
[Prefix] NVARCHAR(5) NOT NULL,
[CodeWidth] TINYINT DEFAULT 3 NOT NULL,
```

### Visibility Settings
```sql
[PostingDateVisibility] NVARCHAR(50) NOT NULL DEFAULT N'Optional' 
    CONSTRAINT [CK_DocumentDefinitions__PostingDateVisibility] 
    CHECK ([PostingDateVisibility] IN (N'None', N'Optional', N'Required')),
[CenterVisibility] NVARCHAR(50) NOT NULL DEFAULT N'Optional' 
    CONSTRAINT [CK_DocumentDefinitions__CenterVisibility] 
    CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),
```

### Lookup Fields
```sql
[Lookup1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_DocumentDefinitions__Lookup1Visibility] 
    CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
[Lookup1DefinitionId] INT CONSTRAINT [FK_DocumentDefinitions__Lookup1DefinitionId] 
    REFERENCES [dbo].[LookupDefinitions]([Id]),
[Lookup1Label] NVARCHAR(50),
[Lookup1Label2] NVARCHAR(50),
[Lookup1Label3] NVARCHAR(50),

[Lookup2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_DocumentDefinitions__Lookup2Visibility] 
    CHECK ([Lookup2Visibility] IN (N'None', N'Optional', N'Required')),
[Lookup2DefinitionId] INT CONSTRAINT [FK_DocumentDefinitions__Lookup2DefinitionId] 
    REFERENCES [dbo].[LookupDefinitions]([Id]),
[Lookup2Label] NVARCHAR(50),
[Lookup2Label2] NVARCHAR(50),
[Lookup2Label3] NVARCHAR(50),
```

### ZATCA Integration
```sql
[ZatcaDocumentType] NVARCHAR(3) CONSTRAINT [CK_DocumentDefinitions__ZatcaDocumentType] 
    CHECK ([ZatcaDocumentType] IN (N'381', N'383', N'386', N'388', N'389')),
```

### Additional Features
```sql
[ClearanceVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_DocumentDefinitions__ClearanceVisibility] 
    CHECK ([ClearanceVisibility] IN (N'None', N'Optional', N'Required')),
[MemoVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_DocumentDefinitions__MemoVisibility] 
    CHECK ([MemoVisibility] IN (N'None', N'Optional', N'Required')),
[AttachmentVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CONSTRAINT [CK_DocumentDefinitions__AttachmentVisibility] 
    CHECK ([AttachmentVisibility] IN (N'None', N'Optional', N'Required')),
[HasBookkeeping] BIT NOT NULL DEFAULT 1,
[CloseValidateScript] NVARCHAR(MAX),
```

### State Management
```sql
[State] NVARCHAR(50) NOT NULL DEFAULT N'Hidden' 
    CONSTRAINT [CK_DocumentDefinitions__State] 
    CHECK([State] IN (N'Hidden', N'Visible', N'Archived', N'Testing')),
[MainMenuIcon] NVARCHAR(50),
[MainMenuSection] NVARCHAR(50),
[MainMenuSortKey] DECIMAL(9,4),
```

### Audit Fields
```sql
[SavedById] INT NOT NULL CONSTRAINT [FK_DocumentDefinitions__SavedById] 
    REFERENCES [dbo].[Users] ([Id]),
[ValidFrom] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
[ValidTo] DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
```

## Constraints and Relationships

### Primary Key and Uniqueness
- Primary Key: [Id] (auto-incrementing)
- Unique Constraint: [Code] (via UQ_DocumentDefinitions__Code)

### Foreign Keys
1. [Lookup1DefinitionId] → [dbo].[LookupDefinitions].[Id]
2. [Lookup2DefinitionId] → [dbo].[LookupDefinitions].[Id]
3. [SavedById] → [dbo].[Users].[Id]

### Check Constraints
1. [PostingDateVisibility] ∈ {None, Optional, Required}
2. [CenterVisibility] ∈ {None, Optional, Required}
3. [Lookup1Visibility] ∈ {None, Required, Optional}
4. [Lookup2Visibility] ∈ {None, Optional, Required}
5. [ZatcaDocumentType] ∈ {'381', '383', '386', '388', '389'}
6. [ClearanceVisibility] ∈ {None, Optional, Required}
7. [MemoVisibility] ∈ {None, Optional, Required}
8. [AttachmentVisibility] ∈ {None, Optional, Required}
9. [State] ∈ {Hidden, Visible, Archived, Testing}

### System Versioning
- System Versioning Enabled
- History Table: `DocumentDefinitionsHistory`
- Validity Period: [ValidFrom, ValidTo]

## Field Details and Usage

### Key Fields and Their Functionality

1. **IsOriginalDocument** (BIT)
   - When 1: The Code in the Documents table is auto-generated
   - When 0: The Code is manually entered by the user

2. **Prefix** (NVARCHAR(5))
   - Becomes the prefix of codes in the Documents table
   - Combined with SerialNumber to form the displayed Code
   - Display format: Prefix + SerialNumber
   - Code calculation occurs in map.Documents()

3. **Lookup Fields**
   - **Lookup1DefinitionId/Lookup2DefinitionId**: Specify content for Lookup1/2 in Documents table
   - **Lookup1Label/Lookup2Label**: Define field labels on the screen
   - Special meaning in ZATCA integration, otherwise customizable by developers

4. **HasBookkeeping** (BIT)
   - When 1: Enables bookkeeping functionality in the screen
   - Allows users to view accounting transaction conversions

5. **CloseValidateScript** (NVARCHAR(MAX))
   - Contains SQL script for document closure validation
   - Runs when attempting to change document state to 1

6. **MainMenu Fields**
   - Controls document display in the main menu
   - **MainMenuIcon**: Icon representation
   - **MainMenuSection**: Menu section placement
   - **MainMenuSortKey**: Sorting order in menu

## Special Notes
1. Table is managed by Tellma implementation partner (e.g., Banan IT)
3. Multiple language support through Description2/3, TitleSingular2/3, TitlePlural2/3 fields
4. Temporal table with versioning support
5. Supports ZATCA document types for Saudi Arabia tax compliance
6. Lookup fields have special meaning in ZATCA integration but are customizable otherwise
7. Bookkeeping functionality can be toggled via HasBookkeeping field
8. Document codes are dynamically generated using Prefix and SerialNumber
