# Tellma Database Documentation

## Table of Contents
1. [System Overview](#system-overview)
2. [Accounting System](#accounting-system)
   - [Accounts](#accounts)
   - [Account Types](#account-types)
   - [Account Classifications](#account-classifications)
   - [Agent Definitions](#agent-definitions)
   - [Resource Definitions](#resource-definitions)
   - [Account Type Agent Definitions](#account-type-agent-definitions)
   - [Account Type Noted Agent Definitions](#account-type-noted-agent-definitions)
   - [Account Type Noted Resource Definitions](#account-type-noted-resource-definitions)
   - [Account Type Resource Definitions](#account-type-resource-definitions)
3. [Document System](#document-system)
   - [Documents](#documents)
   - [Lines](#lines)
   - [Entries](#entries)
   - [Document Definitions](#document-definitions)
   - [Line Definitions](#line-definitions)
   - [Line Definition Entries](#line-definition-entries)
   - [Entry Types](#entry-types)
   - [Workflows](#workflows)
4. [Reference Data](#reference-data)
   - [Centers](#centers)
   - [Currencies](#currencies)
   - [Users](#users)
   - [Settings](#settings)
   - [Lookups](#lookups)
   - [Lookup Definitions](#lookup-definitions)
5. [Appendix](#appendix)
   - [Common Fields](#common-fields)
   - [Multi-Tenant Considerations](#multi-tenant-considerations)
   - [Validation Rules](#validation-rules)

## System Overview

### Key Concepts
1. **Multi-Tenant Architecture**
   - Each tenant has its own database
   - Schema is shared across all tenant databases
   - Data is isolated per tenant

2. **Inheritance Rules**
   - Three-level inheritance hierarchy:
     1. Document header (if Document.IsCommon = 1)
     2. Tab header (if Document.IsCommon = 0 AND DocumentLineDefinitionEntries.IsCommon = 1)
     3. Line level (if both IsCommon flags = 0)

3. **Entry Indexing**
   - EntryIndex represents position within a line definition
   - Used to define specific accounting entries
   - Example: For a stock purchase line:
     - EntryIndex 0: Dr. Current inventories
     - EntryIndex 1: Dr. Current VAT receivables
     - EntryIndex 2: Cr. Cash

4. **Field Types**
   - Common fields (IsCommon = 1): Inherited from header
   - Line-specific fields (IsCommon = 0): Set at line level
   - Entry-specific fields: Set at entry level
   - Special note: PostingDate and Memo are line-level fields

5. **Document Structure**
   - Documents are composed of multiple line definitions
   - Each line definition defines a grid of lines with specific columns and entry mappings
   - Document is organized into multiple tabs, each representing a grid of lines
   - Each tab has its own column structure

6. **Document States**
   - 0: Open
   - 1: Closed
   - -1: Canceled

7. **Clearance Levels**
   - 0: Public
   - 1: Secret
   - 2: Top secret
   - Note: This field alone does not impose security access control
   - Can be used with roles and permissions to control access
   - Example: 
     - Role A: Access to Public documents only
     - Role B: Access to Secret documents
     - Role C: Access to Top Secret documents

## Accounting System

### Accounts
The Accounts table represents the chart of accounts in the system.

#### Key Fields
```sql
[Id] INT,
[Name] NVARCHAR(255),
[Name2] NVARCHAR(255),
[Name3] NVARCHAR(255),
[Code] NVARCHAR(50),
[AccountTypeId] INT,
[CenterId] INT,
[ClassificationId] INT,
[AgentDefinitionId] INT,
[AgentId] INT,
[ResourceDefinitionId] INT,
[ResourceId] INT,
[NotedAgentDefinitionId] INT,
[NotedAgentId] INT,
[NotedResourceDefinitionId] INT,
[NotedResourceId] INT,
[CurrencyId] NCHAR(3),
[EntryTypeId] INT,
```

#### Purpose
- Maintains the chart of accounts
- Controls account visibility and accessibility
- Manages account types and classifications

### Account Classifications

The `AccountClassifications` table is used to create a hierarchical classification system for accounts. This classification system is completely customizable by the user and provides a flexible way to organize accounts according to their needs.

#### Key Features

1. **Hierarchical Structure**
   - Uses `HIERARCHYID` to maintain the tree structure
   - Each row has a `Node` column that represents its position in the hierarchy
   - The `ParentId` column references the parent classification
   - The `IsLeaf` column indicates whether a classification is a leaf node (no children)

2. **Naming and Identification**
   - Each classification has a `Code` (required, unique, max 50 chars)
   - Multiple language support through `Name`, `Name2`, and `Name3` columns
   - The `Code` serves as the basis of the tree structure

3. **Activity Control**
   - `IsActive` column controls visibility in the account classification UI
   - Inactive classifications don't appear when classifying accounts
   - Default value is 1 (active)

4. **Audit Tracking**
   - Complete audit trail with `CreatedAt`, `CreatedById`, `ModifiedAt`, and `ModifiedById`
   - All timestamps are stored with timezone information (DATETIMEOFFSET)

#### Key Fields
```sql
[Id] INT,
[ParentId] INT,
[Node] HIERARCHYID,
[Code] NVARCHAR(50),
[Name] NVARCHAR(255),
[Name2] NVARCHAR(255),
[Name3] NVARCHAR(255),
[IsActive] BIT,
[IsLeaf] BIT,
[CreatedAt] DATETIMEOFFSET(7),
[CreatedById] INT,
[ModifiedAt] DATETIMEOFFSET(7),
[ModifiedById] INT
```

#### Important Notes

1. **AccountTypeParentId**
   - This column is present but not used in the current implementation
   - It was likely intended for future use but remains unused

2. **Tree Structure Maintenance**
   - The table includes triggers to maintain the tree structure:
     - `trIU_AccountClassifications`: Updates leaf status on insert/update
     - `trD_AccountClassifications`: Updates leaf status on delete

3. **Unique Constraints**
   - The `Code` column has a unique constraint
   - The `Node` column has a unique constraint
   - The table uses a non-clustered primary key on `Id`
   - The clustered index is on the `Code` column

4. **Usage**
   - The AccountClassifications table provides a flexible way to organize accounts according to local financial standards and requirements.
   - Examples of usage in different regions:
     - **Senegal**: Follows OHADA accounting standards
     - **Lebanon**: Uses a customized version of OHADA
     - **Ethiopia**: Requires specific financial statement formats for the Ministry of Finance (even though it follows IFRS)

### Account Types
The AccountTypes table defines the types of accounts in the system.

#### Key Fields
```sql
[Id] INT,
[Name] NVARCHAR(255),
[AccountClassificationId] INT,
[IsCommon] BIT,
[IsSystem] BIT,
```

#### Purpose
- Defines account categories
- Controls account type behavior
- Manages account classification relationships

### Entries
The `Entries` table represents the basic unit of data in line definitions. Each entry captures specific information within a line, forming the fundamental building block of data in the system.

#### Key Characteristics
1. **Entry Types**
   - **Transaction Entries**:
     - Use regular accounts
     - Have non-zero MonetaryValue
     - Track financial movements
   - **Statistical Entries**:
     - Use statistical accounts
     - Have zero MonetaryValue
     - Used for requests, inquiries, and non-financial data

2. **Direction and Value**
   - Direction: -1 (Debit) or 1 (Credit)
   - Multiple value types:
     - MonetaryValue: Transaction amount in currency
     - Value: Equivalent in functional currency
     - RValue: Re-instated value in functional currency (for hyper-inflationary economies)
     - PValue: Equivalent in presentation currency

3. **Hyper-Inflationary Economies**
   - RValue is specifically designed for hyper-inflationary economies
   - Used to adjust values for inflation over time
   - Maintains accurate financial records in rapidly changing economic conditions
   - Provides a way to restate historical values in current terms

#### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[LineId] INT,
[Index] INT,
[Direction] SMALLINT,
[AccountId] INT,
[CurrencyId] NCHAR(3),
[CenterId] INT
```

2. **Value Fields**
```sql
[MonetaryValue] DECIMAL(19,4),
[Value] DECIMAL(19,4),
[RValue] DECIMAL(19,4),
[PValue] DECIMAL(19,4)
```

3. **Quantity and Time Fields**
```sql
[Quantity] DECIMAL(19,4),
[UnitId] INT,
[Time1] DATETIME2(2),
[Time2] DATETIME2(2)
```

4. **Reference Fields**
```sql
[AgentId] INT,
[NotedAgentId] INT,
[ResourceId] INT,
[NotedResourceId] INT,
[EntryTypeId] INT,
[ExternalReference] NVARCHAR(50),
[ReferenceSourceId] INT,
[InternalReference] NVARCHAR(50),
[NotedAgentName] NVARCHAR(255),
[NotedAmount] DECIMAL(19,4),
[NotedDate] DATE
```

5. **Audit Fields**
```sql
[CreatedAt] DATETIMEOFFSET(7),
[CreatedById] INT,
[ModifiedAt] DATETIMEOFFSET(7),
[ModifiedById] INT
```

#### Reference Field Details
- **ExternalReference**: Reference from external systems (e.g., bank transaction reference number)
- **InternalReference**: Reference from internal manual systems (e.g., internal voucher number)
- **NotedAgentName**: Name of related agent without full profile (e.g., warehouse picker name)
- **NotedAmount**: Related amount for specific purposes (residual value, taxable amount, etc.)
- **NotedDate**: Related dates for specific purposes (due dates, contract dates, etc.)

#### Constraints and Rules
1. **Key Constraints**
   - Primary key: Id
   - Unique constraint: (LineId, Index)
   - Direction must be -1 or 1
   - CurrencyId is required
   - CenterId is required

2. **Foreign Key Constraints**
   - LineId references Lines table
   - AccountId references Accounts table
   - CurrencyId references Currencies table
   - AgentId references Agents table
   - ResourceId references Resources table
   - CenterId references Centers table
   - EntryTypeId references EntryTypes table
   - CreatedById and ModifiedById reference Users table

### Document System

#### Lines
The `Lines` table represents individual lines within business documents. Each line is associated with a document and follows a specific line definition that defines its structure and behavior. Lines are organized into grids within documents, with each grid corresponding to a specific line definition.

##### Key Characteristics
1. **Line States**
   - -4: Rejected
   - -3: Cancelled
   - -2: Reversed
   - -1: Draft
   - 0: Requested
   - 1: Approved
   - 2: Completed
   - 3: Approved (for non-workflow lines)
   - 4: Posted

2. **Posting Date Rules**
   - A line cannot be posted (i.e., moved to state = 4) if:
     - PostingDate is more than 1 day in the future
   - Allowances:
     - Posting for tomorrow is allowed
     - This supports users in different time zones
     - Example: Far East users posting documents where their date is ahead of Azure data center date

##### Purpose
The Lines table serves as the core data structure for storing individual line items in documents. Each line:

1. **Line Structure**
   - Each line belongs to a document (DocumentId)
   - Each line follows a specific line definition (DefinitionId)
   - Lines are ordered within documents using Index
   - Lines can have multiple agents (Customer, Supplier, Employee)

2. **Custom Fields**
   - Boolean1: Custom boolean field
   - Decimal1, Decimal2: Custom decimal fields
   - Text1, Text2: Custom text fields
   - LineKey: Custom integer field

3. **Audit Trail**
   - CreatedAt: Timestamp of line creation
   - CreatedById: User who created the line
   - ModifiedAt: Timestamp of last modification
   - ModifiedById: User who last modified the line

##### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[DocumentId] INT,
[DefinitionId] INT,
[State] SMALLINT,
[Index] INT
```

2. **Date Fields**
```sql
[PostingDate] DATE,
[CreatedAt] DATETIMEOFFSET(7),
[ModifiedAt] DATETIMEOFFSET(7)
```

3. **Line-Level Fields**
```sql
[CustomerId] INT,
[SupplierId] INT,
[EmployeeId] INT,
[PostingDate] DATE,
[PostingDateIsCommon] BIT,
[Memo] NVARCHAR(255),
[MemoIsCommon] BIT
```

4. **User-Set Fields**
```sql
[Boolean1] BIT,
[Decimal1] DECIMAL(19,4),
[Decimal2] DECIMAL(19,4),
[Text1] NVARCHAR(50),
[Text2] NVARCHAR(50)
```

5. **System-Set Fields**
```sql
[EmployeeId] INT,
[SupplierId] INT,
[CustomerId] INT
```

6. **Workflow Fields**
```sql
[LineKey] INT
```

7. **User References**
```sql
[CreatedById] INT,
[ModifiedById] INT
```

##### Implementation Notes

1. **State Transitions**
   - Workflow lines follow full state progression (-4 to +4)
   - Non-workflow lines use simplified states:
     - Events and Regulatory lines: (-4, 0, 4)
     - Other non-workflow lines: (-2, 0, 2)
   - State changes trigger appropriate business logic

2. **Document Integration**
   - Lines are part of document grids
   - Each line represents a complete accounting entry with multiple sub-entries
   - EntryIndex defines the position of each accounting entry within the line
   - Three-level inheritance hierarchy for each field:
     1. Document header (if Document.[Field]IsCommon = 1)
     2. Tab header (if Document.[Field]IsCommon = 0 AND DocumentLineDefinitionEntries.[Field]IsCommon = 1)
     3. Line level (if both [Field]IsCommon flags = 0)
   - Note: PostingDate and Memo are line-level fields, not entry-level fields
   - Inheritance is configurable through the UI:
     - Users can override inheritance for specific fields
     - Changes can be made from document to tab level
     - Changes can be made from tab to line level

#### Line Definitions
The `LineDefinitions` table defines the structure and behavior of document lines in the system. It specifies how lines should be processed, validated, and displayed in various document types.

##### Document Types
- **20: T for P (Template for Planning)**
  - Used for planning and forecasting
  - Template for creating plan documents
- **40: Plan**
  - Planning documents
  - Used for budgeting and financial planning
- **60: T for E (Template for Event)**
  - Template for creating event-based documents
  - Used as a base for event transactions
- **80: Model**
  - Template documents
  - Used as reusable templates
- **100: Event**
  - Actual transactions affecting financial statements
  - Records actual business events
- **120: Regulatory**
  - Transactions for regulatory purposes only
  - Not of interest to management
  - Used for compliance reporting

##### Best Practices for Code Naming
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

##### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[Code] NVARCHAR(100),
[LineType] TINYINT,
[TitleSingular] NVARCHAR(100),
[TitlePlural] NVARCHAR(100),
[Description] NVARCHAR(1024)
```

2. **Behavior Fields**
```sql
[AllowSelectiveSigning] BIT,
[ViewDefaultsToForm] BIT,
[BarcodeColumnIndex] INT,
[BarcodeProperty] NVARCHAR(50),
[BarcodeExistingItemHandling] NVARCHAR(50),
[BarcodeBeepsEnabled] BIT
```

3. **Script Fields**
```sql
[GenerateScript] NVARCHAR(MAX),
[PreprocessScript] NVARCHAR(MAX),
[ValidateScript] NVARCHAR(MAX),
[SignValidateScript] NVARCHAR(MAX),
[UnsignValidateScript] NVARCHAR(MAX)
```

4. **Audit Fields**
```sql
[SavedById] INT,
[ValidFrom] DATETIME2,
[ValidTo] DATETIME2
```

#### Line Definition Entries
The `LineDefinitionEntries` table defines the specific entries for each line definition. It specifies how individual entries should be processed within a document line, including their direction, account type, and entry type.

##### Key Characteristics
1. **Hierarchical Structure**
   - Each entry belongs to a specific LineDefinition
   - Entries are ordered by Index
   - Supports parent-child account type relationships

2. **Direction Control**
   - Direction can be -1 (debit) or +1 (credit)
   - Controls the posting direction of entries

3. **Account and Entry Type Linking**
   - Links to AccountTypes through ParentAccountTypeId
   - Links to EntryTypes through EntryTypeId
   - Controls the classification and purpose of entries

##### Entry Processing Flow
1. User fills in the specified fields on the UI
2. Preprocess script populates additional fields
3. System attempts to find a compatible account based on:
   - Parent account type
   - Selected resource
   - Selected agent
   - Noted resource
   - Noted agent

##### Account Selection Logic
- Only considers accounts where IsAutoSelected = 1 (smart accounts)
- If no compatible smart account is found, leaves the account empty
- If multiple compatible smart accounts exist, leaves the account empty
- If a unique compatible smart account exists, selects it automatically

##### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[LineDefinitionId] INT,
[Index] INT,
[Direction] SMALLINT,
[ParentAccountTypeId] INT,
[EntryTypeId] INT
```

2. **Audit Fields**
```sql
[SavedById] INT,
[ValidFrom] DATETIME2,
[ValidTo] DATETIME2
```

#### Document Definitions
The DocumentDefinitions table defines screens and document types in the Tellma application. It is managed by Tellma implementation partners and is essential for defining document types across the system.

##### Table Structure

###### Primary Key and Basic Information
```sql
[Id] INT PRIMARY KEY IDENTITY,
[Code] NVARCHAR(50) UNIQUE NOT NULL,
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
```

###### UI Specifications
```sql
[SortKey] DECIMAL(9,4),
[Prefix] NVARCHAR(5) NOT NULL,
[CodeWidth] TINYINT DEFAULT 3 NOT NULL,
```

###### Visibility Settings
```sql
[PostingDateVisibility] NVARCHAR(50) NOT NULL DEFAULT N'Optional' 
    CHECK ([PostingDateVisibility] IN (N'None', N'Optional', N'Required')),
[CenterVisibility] NVARCHAR(50) NOT NULL DEFAULT N'Optional' 
    CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),
```

###### Lookup Fields
```sql
[Lookup1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
[Lookup1DefinitionId] INT FOREIGN KEY REFERENCES [dbo].[LookupDefinitions]([Id]),
[Lookup1Label] NVARCHAR(50),
[Lookup1Label2] NVARCHAR(50),
[Lookup1Label3] NVARCHAR(50),

[Lookup2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CHECK ([Lookup2Visibility] IN (N'None', N'Optional', N'Required')),
[Lookup2DefinitionId] INT FOREIGN KEY REFERENCES [dbo].[LookupDefinitions]([Id]),
[Lookup2Label] NVARCHAR(50),
[Lookup2Label2] NVARCHAR(50),
[Lookup2Label3] NVARCHAR(50),
```

###### ZATCA Integration
```sql
[ZatcaDocumentType] NVARCHAR(3) 
    CHECK ([ZatcaDocumentType] IN (N'381', N'383', N'386', N'388', N'389')),
```

###### Additional Features
```sql
[ClearanceVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CHECK ([ClearanceVisibility] IN (N'None', N'Optional', N'Required')),
[MemoVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CHECK ([MemoVisibility] IN (N'None', N'Optional', N'Required')),
[AttachmentVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None' 
    CHECK ([AttachmentVisibility] IN (N'None', N'Optional', N'Required')),
[HasBookkeeping] BIT NOT NULL DEFAULT 1,
[CloseValidateScript] NVARCHAR(MAX),
```

###### State Management
```sql
[State] NVARCHAR(50) NOT NULL DEFAULT N'Hidden' 
    CHECK([State] IN (N'Hidden', N'Visible', N'Archived', N'Testing')),
[MainMenuIcon] NVARCHAR(50),
[MainMenuSection] NVARCHAR(50),
[MainMenuSortKey] DECIMAL(9,4),
```

###### Audit Fields
```sql
[SavedById] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Users] ([Id]),
[ValidFrom] DATETIMEOFFSET GENERATED ALWAYS AS ROW START NOT NULL,
[ValidTo] DATETIMEOFFSET GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
```

##### Key Fields and Their Functionality

1. **IsOriginalDocument** (BIT)
   - When 1: The Code in the Documents table is auto-generated
   - When 0: The Code is manually entered by the user

2. **Document States**
   - `Hidden`: Not visible in the UI
   - `Visible`: Active and visible in the UI
   - `Archived`: Read-only, for historical reference
   - `Testing`: For testing purposes only

3. **Visibility Controls**
   - Control which fields are shown/required in the UI
   - Available for: PostingDate, Center, Memo, Attachments, Clearance Level
   - Options: None, Optional, Required

4. **Lookup Integration**
   - Supports up to 2 lookup fields per document
   - Each lookup can reference a different LookupDefinition
   - Supports multi-language labels for each lookup

#### Documents
The Documents table represents business documents in the system, each defined by a document definition that specifies its structure and behavior. Documents are composed of multiple line definitions, each defining a grid of lines with specific columns and entry mappings.

##### Document Structure
1. **Document Definition**
   - Defines the overall document structure
   - Consists of multiple line definitions
   - Each line definition defines a grid of lines

2. **Line Definitions**
   - Each line definition defines a separate grid
   - Lines of the same definition are grouped together
   - Each grid has its own set of columns
   - Entries are mapped differently based on LineDefinitionEntries

3. **Tab-Based Layout**
   - Document is organized into multiple tabs
   - Each tab represents a grid of lines
   - Tabs correspond to different line definitions
   - Each tab has its own column structure

##### Key Characteristics
1. **Document States**
   - 1: Closed
   - 0: Open
   - -1: Canceled
   - State changes are tracked in DocumentStatesHistory

2. **Clearance Levels**
   - 0: Public
   - 1: Secret
   - 2: Top secret
   - Note: This field alone does not impose security access control
   - Can be used with roles and permissions to control access
   - Example: 
     - Role A: Access to Public documents only
     - Role B: Access to Secret documents
     - Role C: Access to Top Secret documents

3. **Common vs Line-Specific Properties**
   - Most properties have an IsCommon flag
   - Common properties are copied to all lines
   - Line-specific properties can vary per line

##### Key Fields
```sql
[Id] INT,
[DocumentDefinitionId] INT,
[SerialNumber] NVARCHAR(50),
[PostingDate] DATE,
[PostingDateIsCommon] BIT,
[Memo] NVARCHAR(255),
[MemoIsCommon] BIT,
[State] TINYINT,
[ClearanceLevel] TINYINT,
[CreatedAt] DATETIMEOFFSET(7),
[CreatedById] INT,
[ModifiedAt] DATETIMEOFFSET(7),
[ModifiedById] INT
```

#### Lines
The Lines table represents individual lines within business documents. Each line is associated with a document and follows a specific line definition that defines its structure and behavior. Lines are organized into grids within documents, with each grid corresponding to a specific line definition.

##### Key Characteristics
1. **Line States**
   - -4: Rejected
   - -3: Cancelled
   - -2: Reversed
   - -1: Draft
   - 0: Requested
   - 1: Approved
   - 2: Completed
   - 3: Approved (for non-workflow lines)
   - 4: Posted

2. **Posting Date Rules**
   - A line cannot be posted (i.e., moved to state = 4) if:
     - PostingDate is more than 1 day in the future
   - Allowances:
     - Posting for tomorrow is allowed
     - This supports users in different time zones
     - Example: Far East users posting documents where their date is ahead of Azure data center date

##### Purpose
The Lines table serves as the core data structure for storing individual line items in documents. Each line:

1. **Line Structure**
   - Each line belongs to a document (DocumentId)
   - Each line follows a specific line definition (DefinitionId)
   - Lines are ordered within documents using Index
   - Lines can have multiple agents (Customer, Supplier, Employee)

2. **Custom Fields**
   - Boolean1: Custom boolean field
   - Decimal1, Decimal2: Custom decimal fields
   - Text1, Text2: Custom text fields
   - LineKey: Custom integer field

3. **Audit Trail**
   - CreatedAt: Timestamp of line creation
   - CreatedById: User who created the line
   - ModifiedAt: Timestamp of last modification
   - ModifiedById: User who last modified the line

##### Key Fields
```sql
[Id] INT,
[DocumentId] INT,
[LineDefinitionId] INT,
[DefinitionIndex] INT,
[PostingDate] DATE,
[PostingDateIsCommon] BIT,
[Memo] NVARCHAR(255),
[MemoIsCommon] BIT,
[AgentId] INT,
[CustomerId] INT,
[SupplierId] INT,
[EmployeeId] INT,
[Boolean1] BIT,
[Decimal1] DECIMAL(19,4),
[Decimal2] DECIMAL(19,4),
[Text1] NVARCHAR(50),
[Text2] NVARCHAR(50),
[LineKey] INT,
[CreatedAt] DATETIMEOFFSET(7),
[CreatedById] INT,
[ModifiedAt] DATETIMEOFFSET(7),
[ModifiedById] INT
```

#### Entries
The Entries table represents the basic unit of data in line definitions.

##### Key Characteristics
- Two main types of entries:
  - Transaction Entries:
    - Use regular accounts
    - Have non-zero MonetaryValue
    - Track financial movements
  - Statistical Entries:
    - Use statistical accounts
    - Have zero MonetaryValue
    - Used for requests, inquiries, and non-financial data

##### Key Fields
```sql
[Id] INT,
[LineId] INT,
[Index] INT,
[Direction] SMALLINT,
[AccountId] INT,
[CurrencyId] NCHAR(3),
[CenterId] INT,
[MonetaryValue] DECIMAL(19,4),
[Value] DECIMAL(19,4),
[RValue] DECIMAL(19,4),
[PValue] DECIMAL(19,4),
[Quantity] DECIMAL(19,4),
[UnitId] INT,
[Time1] DATETIME2(2),
[Time2] DATETIME2(2),
[NotedAgentName] NVARCHAR(255),
[NotedAmount] DECIMAL(19,4),
[NotedDate] DATE,
```

## Reference Data

### Centers
The Centers table represents responsibility centers in the system.

#### Key Fields
```sql
[Id] INT,
[ParentId] INT,
[CenterType] NVARCHAR(255),
[Code] NVARCHAR(50),
[Name] NVARCHAR(255),
[Name2] NVARCHAR(255),
[Name3] NVARCHAR(255),
```

### Currencies
The Currencies table manages supported currencies.

#### Key Fields
```sql
[Id] NCHAR(3),
[Name] NVARCHAR(50),
[Name2] NVARCHAR(50),
[Name3] NVARCHAR(50),
[Description] NVARCHAR(255),
[Description2] NVARCHAR(255),
[Description3] NVARCHAR(255),
[NumericCode] SMALLINT,
[E] SMALLINT,
[IsActive] BIT,
```

## Appendix

### Common Fields
1. **IsCommon Flag**
   - Controls inheritance behavior
   - When 1: Inherits from header
   - When 0: Set at line/entry level
   - Special case: PostingDate and Memo are line-level fields

2. **Multi-Tenant Considerations**
   - Each tenant has its own database
   - Schema is shared across all tenant databases
   - Data is isolated per tenant
   - Inheritance rules are consistent across tenants

3. **Validation Rules**
   - RequiredAttribute: Marks database columns as NOT NULL
   - ValidateRequiredAttribute: Used for API validation
     - Returns false for: null values, empty strings, whitespace-only strings
     - Returns true for: non-null values, non-empty strings, actual content
