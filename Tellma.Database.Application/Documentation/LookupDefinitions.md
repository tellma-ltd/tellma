# LookupDefinitions Table Documentation

## Overview
The `LookupDefinitions` table serves as the foundation for managing reference data in the Tellma application. It defines the structure for various types of lookups that are typically static or change infrequently. The table includes two main categories:

1. **System Lookups**
   - Used for internal reference data
   - Examples:
     - Calendar days (work day/holiday tracking)
     - System configuration values
   - Hidden from user interface
   - Used for system-level reference data

2. **User-Managed Lookups**
   - Used for business-specific reference data
   - Examples:
     - Colors
     - Car makes and models
     - Blood types
     - Marital statuses
     - Genders
   - Can be visible in the UI
   - Managed through the application interface

## Data Sources

### System-Defined Lookups
- Pre-populated during installation
- Used for:
  - System configuration
  - Calendar day tracking
  - Internal reference data
- Examples:
  - CalendarDay: Tracks work days and holidays (State: Testing)
  - SYSTEM_CONFIG: System configuration values

### User-Defined Lookups
- Created and managed through the application
- Used for:
  - Business-specific reference data
  - Custom lookups
  - Multi-language support
- Maintained by business users

## Key Characteristics

1. **Data Structure**
   - Simple key-value pairs
   - Supports multi-language
   - No additional attributes needed
   - Used across multiple entities

2. **Multi-language Support**
   - Primary language (TitleSingular/TitlePlural)
   - Secondary language (TitleSingular2/TitlePlural2)
   - Tertiary language (TitleSingular3/TitlePlural3)
   - All translations must be maintained

3. **State Management**
   - Hidden: System lookups not shown in UI
   - Visible: User-managed lookups shown in UI
   - Archived: Retained for historical reference
   - Testing: For development and testing

## Purpose
The LookupDefinitions table serves as the foundation for managing reference data in the Tellma application. It defines the structure for various types of lookups that are typically static or change infrequently.

### Key Relationships
- Each lookup definition must have a unique Code
- The State field determines visibility and usability:
  - Hidden: System lookups not shown in UI
  - Visible: User-managed lookups shown in UI
  - Archived: Retained for historical reference
  - Testing: For development and testing
- When State is Visible, MainMenuSection is required
- The SavedById field references Users table
- Supports system-versioning through ValidFrom/ValidTo columns

### Key Fields

1. **Code Fields**
```sql
[Code] NVARCHAR(50),
[TitleSingular] NVARCHAR(50),
[TitleSingular2] NVARCHAR(50),
[TitleSingular3] NVARCHAR(50),
[TitlePlural] NVARCHAR(50),
[TitlePlural2] NVARCHAR(50),
[TitlePlural3] NVARCHAR(50),
```
   - Code: Unique identifier (e.g., 'COLORS', 'BLOOD_TYPES')
   - TitleSingular/TitlePlural: Primary language names
   - TitleSingular2/TitlePlural2: Secondary language names
   - TitleSingular3/TitlePlural3: Tertiary language names

2. **UI Fields**
```sql
[State] NVARCHAR(50),
[MainMenuIcon] NVARCHAR(50),
[MainMenuSection] NVARCHAR(50),
[MainMenuSortKey] DECIMAL(9,4),
```
   - State: Controls visibility and usability
   - MainMenuIcon: Icon for UI display
   - MainMenuSection: Menu organization
   - MainMenuSortKey: Sort order within menu

3. **Audit Fields**
```sql
[SavedById] INT,
[ValidFrom] DATETIME2,
[ValidTo] DATETIME2,
```
   - SavedById: References Users table
   - ValidFrom/ValidTo: System-versioning timestamps

## Related Tables

### Lookups Table
Contains the actual lookup values with the following key columns:
- `Id`: Primary key
- `DefinitionId`: References LookupDefinitions.Id
- `Name`: Display name
- `Name2`, `Name3`: Additional language names
- `IsActive`: For calendar days, 1 = work day, 0 = holiday/weekend
- `Code`: For calendar days, format is YYYYMMDD
- `SortKey`: Display order
- `CreatedById`, `ModifiedById`: Audit fields
- `CreatedAt`, `ModifiedAt`: Timestamps

### LookupDefinitionReportDefinitions
- Links lookup definitions to report definitions
- Used for generating reports based on lookup data

## Best Practices

### Naming Conventions
- Use uppercase with underscores for `Code` (e.g., 'COLORS', 'BLOOD_TYPES')
- Keep names concise but descriptive
- Use TitleSingular/TitlePlural for proper UI display

### State Management
- **Hidden**: System lookups not shown in UI (e.g., CalendarDay)
- **Visible**: User-managed lookups shown in UI
- **Archived**: Retained for historical reference
- **Testing**: For development and testing

### Multi-language Support
- Always provide at least the primary language (TitleSingular/TitlePlural)
- Use TitleSingular2/TitlePlural2 and TitleSingular3/TitlePlural3 for additional languages
- Keep translations in sync across all language versions

## Example: Calendar Days Implementation

```sql
-- Create calendar days lookup definition
INSERT INTO [dbo].[LookupDefinitions]
    ([Code], [TitleSingular], [TitlePlural], [State], [SavedById])
VALUES
    ('CalendarDay', 'Calendar Day', 'Calendar Days', 'Testing', 1);

-- Add work days and holidays
INSERT INTO [dbo].[Lookups]
    ([DefinitionId], [Name], [Code], [IsActive], [SortKey], [CreatedById], [ModifiedById])
VALUES
    -- Work days (IsActive = 1)
    (SCOPE_IDENTITY(), 'Monday', '20230605', 1, 1.0, 1, 1),
    (SCOPE_IDENTITY(), 'Tuesday', '20230606', 1, 2.0, 1, 1),
    
    -- Holidays (IsActive = 0)
    (SCOPE_IDENTITY(), 'Eid Al-Fitr', '20230607', 0, 3.0, 1, 1),
    (SCOPE_IDENTITY(), 'Weekend', '20230608', 0, 4.0, 1, 1);
```

### Querying Work Days
```sql
-- Get all work days in a date range
SELECT [Name], [Code] 
FROM [dbo].[Lookups] l
JOIN [dbo].[LookupDefinitions] ld ON l.[DefinitionId] = ld.[Id]
WHERE ld.[Code] = 'CalendarDay'
  AND l.[IsActive] = 1
  AND l.[Code] BETWEEN '20230101' AND '20231231'
ORDER BY l.[SortKey];
```

## Performance Considerations
- Lookups are cached by the application for performance
- Keep the number of lookup values reasonable (hundreds, not thousands)
- Use appropriate indexes for frequently queried columns
- Consider partitioning for very large lookup sets
