# DocumentLineDefinitionEntries Table Documentation

## Overview
The `DocumentLineDefinitionEntries` table defines how entries in a line definition can inherit properties from document or tab headers. Each entry in this table specifies whether a property should be inherited from the document header or tab header, and whether it's common across all entries of that index.

## Key Characteristics

1. **Inheritance Rules**
   - Three-level inheritance hierarchy for each field:
     1. Document header (if Document.[Field]IsCommon = 1)
     2. Tab header (if Document.[Field]IsCommon = 0 AND DocumentLineDefinitionEntries.[Field]IsCommon = 1)
     3. Line level (if both [Field]IsCommon flags = 0)
   - Example for Agent field:
     - Document.AgentIsCommon = 1 → inherits from document header
     - Document.AgentIsCommon = 0 AND DocumentLineDefinitionEntries.AgentIsCommon = 1 → inherits from tab header
     - Both AgentIsCommon = 0 → set at line level
   - This applies to all fields with IsCommon flags:
     - AgentIsCommon
     - ResourceIsCommon
     - CurrencyIsCommon
     - CenterIsCommon
     - PostingDateIsCommon
     - MemoIsCommon
     - And other IsCommon flags

2. **Entry Index**
   - EntryIndex represents the position of an accounting entry within a line
   - Multiple accounting entries can exist within a single line
   - Example: For a stock purchase line (Dr. Current inventories, Dr. Current VAT receivables, Cr. Cash):
     - EntryIndex 0: Dr. Current inventories
     - EntryIndex 1: Dr. Current VAT receivables
     - EntryIndex 2: Cr. Cash

## Purpose
The DocumentLineDefinitionEntries table defines the inheritance behavior of properties in line definitions. It serves three main purposes:

1. **Property Inheritance Configuration**
   - Specifies which properties can inherit from document header
   - Specifies which properties can inherit from tab header
   - Controls inheritance behavior per entry index

2. **Entry Level Configuration**
   - Defines property inheritance rules for each entry index
   - Allows overriding of default inheritance from line definition
   - Provides per-entry control over property inheritance

3. **Tab Header Management**
   - Controls which properties are common across entries in a tab
   - Manages inheritance from tab header when document header inheritance is disabled
   - Provides flexibility in property inheritance hierarchy

## Key Fields

1. **Core Fields**
```sql
[Id] INT,
[DocumentId] INT,
[LineDefinitionId] INT,
[EntryIndex] INT
```
   - Id: Primary key
   - DocumentId: References Documents table (ON DELETE CASCADE)
   - LineDefinitionId: References LineDefinitions table
   - EntryIndex: Entry position in line definition

2. **Line-Level Fields**
```sql
[PostingDate] DATE,
[PostingDateIsCommon] BIT,
[Memo] NVARCHAR(255),
[MemoIsCommon] BIT
```
   - These fields are defined at line level, not entry level
   - PostingDate: Can inherit from document header
   - Memo: Description for statements and reports
   - Both have IsCommon flags for inheritance control

3. **Entry-Level Fields**
```sql
[CurrencyId] NCHAR(3),
[CurrencyIsCommon] BIT,
[CenterId] INT,
[CenterIsCommon] BIT,
[AgentId] INT,
[AgentIsCommon] BIT,
[NotedAgentId] INT,
[NotedAgentIsCommon] BIT,
[ResourceId] INT,
[ResourceIsCommon] BIT,
[NotedResourceId] INT,
[NotedResourceIsCommon] BIT
```
   - These fields are defined at entry level
   - Each field has a corresponding IsCommon flag
   - IsCommon = 1 means property is inherited from header
   - IsCommon = 0 means property is entry-specific

4. **Quantity and Time Fields**
```sql
[Quantity] DECIMAL(19,4),
[QuantityIsCommon] BIT,
[UnitId] INT,
[UnitIsCommon] BIT,
[Time1] DATETIME2(2),
[Time1IsCommon] BIT,
[Duration] DECIMAL(19,4),
[DurationIsCommon] BIT,
[DurationUnitId] INT,
[DurationUnitIsCommon] BIT,
[Time2] DATETIME2(2),
[Time2IsCommon] BIT
```
   - Quantity and time-related fields
   - Each field has corresponding IsCommon flag
   - Supports both common and line-specific values

## Best Practices

1. **Inheritance Configuration**
   - Set IsCommon = 1 for properties that should be inherited
   - Set IsCommon = 0 for properties that should be line-specific
   - Consider inheritance impact on data entry and reporting

2. **Entry Index Usage**
   - EntryIndex is used to define the accounting entry positions within a line
   - Each EntryIndex represents a specific accounting entry in the line
   - Example: For a stock purchase line:
     - EntryIndex 0: Dr. Current inventories
     - EntryIndex 1: Dr. Current VAT receivables
     - EntryIndex 2: Cr. Cash
   - The inheritance rules specified for each EntryIndex apply to that specific accounting entry

3. **Property Inheritance**
   - Configure inheritance per entry
   - Consider both document and tab headers
   - Ensure proper validation of inherited properties

## Implementation Notes

1. **Inheritance Rules**
   - Properties inherit from document header when IsCommon = 1
   - Properties are line-specific when IsCommon = 0
   - Inheritance is per entry index

2. **Special Properties**
   - PostingDate and Memo are defined at line level, not entry level
   - These properties are always common when IsCommon = 1

3. **Validation**
   - Validate inherited properties
   - Ensure proper data types
   - Maintain referential integrity

## Multi-Tenant Considerations
- Each tenant has its own database
- Line definition entries are isolated per tenant
- Schema is shared across all tenant databases
- Inheritance rules are consistent across tenants
