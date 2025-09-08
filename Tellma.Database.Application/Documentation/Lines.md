# Lines Table Documentation

## Overview
The `Lines` table represents individual lines within business documents. Each line is associated with a document and follows a specific line definition that defines its structure and behavior. Lines are organized into grids within documents, with each grid corresponding to a specific line definition.

## Key Characteristics

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

## Purpose
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

## Key Fields

1. **Core Fields**
```sql
[Id] INT,
[DocumentId] INT,
[DefinitionId] INT,
[State] SMALLINT,
[Index] INT
```
   - Id: Primary key
   - DocumentId: Foreign key to Documents table (ON DELETE CASCADE)
   - DefinitionId: Foreign key to LineDefinitions table
   - State: Current state of the line (-4 to +4)
   - Index: Order of the line within the document

2. **Date Fields**
```sql
[PostingDate] DATE,
[CreatedAt] DATETIMEOFFSET(7),
[ModifiedAt] DATETIMEOFFSET(7)
```
   - PostingDate: Required for posted lines
   - CreatedAt: Automatic creation timestamp
   - ModifiedAt: Automatic modification timestamp

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
   - These fields are defined at line level
   - PostingDate and Memo are always line-level fields
   - CustomerId, SupplierId, and EmployeeId are system-set fields
   - All have IsCommon flags for inheritance control

4. **User-Set Fields**
```sql
[Boolean1] BIT,
[Decimal1] DECIMAL(19,4),
[Decimal2] DECIMAL(19,4),
[Text1] NVARCHAR(50),
[Text2] NVARCHAR(50)
```
   - These fields can be exposed to users by developers
   - Users can set these fields through the Tellma interface
   - Used for custom business logic and reporting

5. **System-Set Fields**
```sql
[EmployeeId] INT,
[SupplierId] INT,
[CustomerId] INT
```
   - Set automatically by database triggers in Entries table
   - Used for T-Account generation and account tracking
   - EmployeeId: Tracks employee accrual accounts
   - SupplierId: Tracks purchase invoice accounts
   - CustomerId: Tracks sales invoice accounts

6. **Workflow Fields**
```sql
[LineKey] INT
```
   - Set by sign validate script in LineDefinition
   - Used to link related line definitions in workflows
   - Example: Links line definitions for:
     - Initiate salary contract
     - Amend salary contract
     - Terminate salary contract

7. **User References**
```sql
[CreatedById] INT,
[ModifiedById] INT
```
   - References Users table
   - Tracks who created and modified the line

## Indexes
```sql
CREATE INDEX [IX_Lines__DocumentId] ON [dbo].[Lines]([DocumentId]);
CREATE INDEX [IX_Lines__DefinitionId] ON [dbo].[Lines]([DefinitionId]);
CREATE INDEX [IX_Lines__EmployeeId] ON dbo.Lines([EmployeeId]);
CREATE INDEX [IX_Lines__CustomerId] ON dbo.Lines([CustomerId]);
CREATE INDEX [IX_Lines__SupplierId] ON dbo.Lines([SupplierId]);
```
   - Optimizes queries by document
   - Optimizes queries by line definition
   - Optimizes queries by agent relationships

## Best Practices

1. **State Management**
   - Follow proper state progression (-4 to +4)
   - Ensure PostingDate is set before posting
   - Maintain proper state history

2. **Agent References**
   - Use appropriate agent type (Customer/Supplier/Employee)
   - Maintain proper agent relationships
   - Ensure agent references are valid

3. **Custom Fields**
   - Use Boolean1 for binary flags
   - Use Decimal fields for numeric values
   - Use Text fields for short descriptions
   - Use LineKey for custom identification
   - Use Memo for line descriptions

4. **Audit Trail**
   - Maintain proper creation and modification history
   - Track all state changes
   - Link to appropriate users

## Multi-Tenant Considerations
- Each tenant has its own database
- Line data is isolated per tenant
- Schema is shared across all tenant databases
- Performance optimized for individual tenant databases

## Implementation Notes
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
   - Example: For a stock purchase line:
     - EntryIndex 0: Dr. Current inventories
     - EntryIndex 1: Dr. Current VAT receivables
     - EntryIndex 2: Cr. Cash
   - Each EntryIndex represents a specific accounting entry with its own inheritance rules
   - This applies to all fields with IsCommon flags:
     - AgentIsCommon
     - ResourceIsCommon
     - CurrencyIsCommon
     - CenterIsCommon
     - And other IsCommon flags
   - Note: PostingDate and Memo are line-level fields, not entry-level fields
   - Inheritance is configurable through the UI:
     - Users can override inheritance for specific fields
     - Changes can be made from document to tab level
     - Changes can be made from tab to line level

3. **Agent Relationships**
   - Multiple agent types supported
   - Agent relationships are flexible
   - Proper validation of agent references
