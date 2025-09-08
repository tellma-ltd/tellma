# LineDefinitionEntries Table Documentation

## Overview
The `LineDefinitionEntries` table defines the specific entries for each line definition. It specifies how individual entries should be processed within a document line, including their direction, account type, and entry type.

## Key Characteristics

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

## Purpose
The LineDefinitionEntries table defines the structure and behavior of individual entries within a document line. It specifies:

1. **Entry Structure**
   - Number of entries in the line
   - Debit/Credit direction for each entry
   - Parent account type for account selection
   - Mapping of UI fields to entity fields through LineDefinitionColumns

2. **Entry Processing Flow**
   - User fills in the specified fields on the UI
   - Preprocess script populates additional fields
   - System attempts to find a compatible account based on:
     - Parent account type
     - Selected resource
     - Selected agent
     - Noted resource
     - Noted agent

3. **Account Selection Logic**
   - Only considers accounts where IsAutoSelected = 1 (smart accounts)
   - If no compatible smart account is found, leaves the account empty
   - If multiple compatible smart accounts exist, leaves the account empty
   - If a unique compatible smart account exists, selects it automatically

### Key Relationships
- Each entry must have a LineDefinitionId
- Entries are ordered by Index within a line definition
- Links to AccountTypes through ParentAccountTypeId
- Links to EntryTypes through EntryTypeId
- References Users table through SavedById
- Supports system-versioning through ValidFrom/ValidTo columns

### Implementation Considerations
- Ensure smart accounts (IsAutoSelected = 1) exist for every use case
- Maintain uniqueness of smart accounts for automatic selection
- Account selection logic depends on proper account type hierarchy
- Preprocess scripts must correctly populate required fields

### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[LineDefinitionId] INT,
[Index] INT,
[Direction] SMALLINT,
[ParentAccountTypeId] INT,
[EntryTypeId] INT
```
   - Id: Primary key
   - LineDefinitionId: References parent line definition
   - Index: Order of entry within line
   - Direction: Posting direction (-1/1)
   - ParentAccountTypeId: References account type
   - EntryTypeId: References entry type

2. **Audit Fields**
```sql
[SavedById] INT,
[ValidFrom] DATETIME2,
[ValidTo] DATETIME2
```
   - SavedById: References Users table
   - ValidFrom/ValidTo: System-versioning timestamps

## Related Tables

### LineDefinitions
- Defines the structure of the document line
- Controls overall line behavior
- Contains scripts for processing

### AccountTypes
- Defines the account type hierarchy
- Controls account classification
- Links to chart of accounts

### EntryTypes
- Defines the purpose and classification of entries
- Implements IFRS concepts
- Supports financial reporting

### Users
- References for SavedById
- Tracks accountability for record changes

## Best Practices

### Entry Structure
- Use proper Index values for ordering
- Set correct Direction (-1/1) for posting
- Link appropriate AccountTypes
- Use correct EntryTypes for classification

### Account Type Management
- Use ParentAccountTypeId for proper hierarchy
- Maintain proper account type relationships
- Keep account types consistent

### Entry Type Usage
- Use appropriate EntryTypes for classification
- Follow IFRS concepts when applicable
- Keep entry types consistent

## Performance Considerations
- Use proper indexes on frequently queried columns
- Consider partitioning for large entry sets
- Cache frequently used entry definitions
- Use proper indexing on foreign key columns
