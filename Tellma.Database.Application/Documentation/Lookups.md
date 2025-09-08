# Lookups Table Documentation

## Overview
The `Lookups` table stores the actual values for lookup definitions. Each row represents a specific value within a lookup definition, such as individual colors, car makes, or calendar days.

## Key Characteristics

1. **Data Structure**
   - Stores actual lookup values
   - Supports multi-language
   - Tracks active/inactive status
   - Maintains sort order
   - Supports inter-tenant reporting

2. **Multi-language Support**
   - Primary language (Name)
   - Secondary language (Name2)
   - Tertiary language (Name3)
   - All translations must be maintained

3. **State Management**
   - IsActive: Controls whether the lookup value is currently active
   - CreatedAt/ModifiedAt: Tracks creation and modification timestamps
   - CreatedById/ModifiedById: Tracks who created/modified the record

## Purpose
The Lookups table stores the actual values for lookup definitions. It works in conjunction with LookupDefinitions to provide a complete reference data solution.

### Key Relationships
- Each lookup must have a DefinitionId referencing LookupDefinitions.Id
- The IsActive field controls whether the lookup value is currently active
- The SortKey field determines display order
- The Code field provides a unique identifier for inter-tenant reporting
- The CreatedById/ModifiedById fields reference Users table

### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[DefinitionId] INT,
[Name] NVARCHAR(50),
[Name2] NVARCHAR(50),
[Name3] NVARCHAR(50),
[IsActive] BIT,
[Code] NVARCHAR(10),
[SortKey] DECIMAL(9,4),
```
   - Id: Primary key
   - DefinitionId: References LookupDefinitions table
   - Name/Name2/Name3: Multi-language display names
   - IsActive: Controls whether the lookup is active
   - Code: Unique identifier for inter-tenant reporting
   - SortKey: Determines display order

2. **Audit Fields**
```sql
[CreatedAt] DATETIMEOFFSET(7),
[CreatedById] INT,
[ModifiedAt] DATETIMEOFFSET(7),
[ModifiedById] INT,
```
   - CreatedAt/ModifiedAt: Timestamps for record changes
   - CreatedById/ModifiedById: References Users table for accountability

## Related Tables

### LookupDefinitions
- Defines the structure and type of lookups
- Each lookup must have a corresponding definition
- Controls visibility and usability through State field

### Users
- References for CreatedById and ModifiedById
- Tracks accountability for record changes

## Best Practices

### Naming Conventions
- Use descriptive names that match the lookup definition
- Keep names concise but meaningful
- Use Name2/Name3 for additional languages

### State Management
- Set IsActive = 1 for active values
- Set IsActive = 0 for inactive values
- Use SortKey to control display order

### Multi-language Support
- Always provide at least the primary language (Name)
- Use Name2/Name3 for additional languages
- Keep translations in sync across all language versions

## Example: Calendar Days Implementation

```sql
-- Add work days and holidays
INSERT INTO [dbo].[Lookups]
    ([DefinitionId], [Name], [Code], [IsActive], [SortKey], [CreatedById], [ModifiedById])
VALUES
    -- Work days (IsActive = 1)
    (1, 'Monday', '20230605', 1, 1.0, 1, 1),
    (1, 'Tuesday', '20230606', 1, 2.0, 1, 1),
    
    -- Holidays (IsActive = 0)
    (1, 'Eid Al-Fitr', '20230607', 0, 3.0, 1, 1),
    (1, 'Weekend', '20230608', 0, 4.0, 1, 1);
```

### Querying Active Lookups
```sql
-- Get all active lookups for a specific definition
SELECT [Name], [Code], [SortKey]
FROM [dbo].[Lookups]
WHERE [DefinitionId] = 1
AND [IsActive] = 1
ORDER BY [SortKey];
```

## Performance Considerations
- Lookups are cached by the application for performance
- Keep the number of lookup values reasonable
- Use appropriate indexes on frequently queried columns
- Consider partitioning for very large lookup sets
- Use SortKey for efficient ordering
