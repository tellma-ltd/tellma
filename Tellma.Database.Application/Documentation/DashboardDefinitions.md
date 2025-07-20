# DashboardDefinitions Table

## Overview
The `DashboardDefinitions` table stores the basic configuration and metadata for dashboards in Tellma. It defines the dashboard's identity, display properties, and menu integration, while the actual content and access controls are managed in related tables.

## Table Structure
```sql
CREATE TABLE [dbo].[DashboardDefinitions]
(
    [Id]                            INT             IDENTITY PRIMARY KEY,
    [Code]                          NVARCHAR(50)    NOT NULL UNIQUE,
    [Title]                         NVARCHAR(50),
    [Title2]                        NVARCHAR(50),
    [Title3]                        NVARCHAR(50),
    [AutoRefreshPeriodInMinutes]    INT             NOT NULL DEFAULT 5,
    [ShowInMainMenu]               BIT,
    [MainMenuSection]              NVARCHAR(50),   -- NULL means "Miscellaneous"
    [MainMenuIcon]                 NVARCHAR(50),
    [MainMenuSortKey]              DECIMAL(9,4),
    [CreatedAt]                    DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [CreatedById]                  INT             NOT NULL,
    [ModifiedAt]                   DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [ModifiedById]                 INT             NOT NULL
);
```

## Key Fields

### Identification
- **Id**: Unique identifier for the dashboard definition
- **Code**: Unique code used to reference the dashboard (e.g., 'sales-overview', 'inventory-status')

### Multi-language Support
- **Title/Title2/Title3**: Dashboard title in different languages (supports up to 3 languages)

### Behavior
- **AutoRefreshPeriodInMinutes**: How often the dashboard automatically refreshes its data (default: 5 minutes)

### Menu Integration
- **ShowInMainMenu**: Whether the dashboard appears in the main navigation menu
- **MainMenuSection**: Menu section where the dashboard appears (NULL = "Miscellaneous")
- **MainMenuIcon**: Icon to display next to the dashboard in the menu
- **MainMenuSortKey**: Controls the sort order of dashboards within their menu section

### Audit Fields
- **CreatedAt**: When the dashboard was created
- **CreatedById**: User who created the dashboard
- **ModifiedAt**: When the dashboard was last modified
- **ModifiedById**: User who last modified the dashboard

## Relationships
- **CreatedBy/ModifiedBy**: Many-to-one relationships with `Users` table
- **Related Tables**:
  - `DashboardRoleAssignments`: Controls which roles can access this dashboard
  - `DashboardWidgets`: Defines the widgets/reports included in the dashboard
  - `DashboardDefinitionReportDefinitions`: Maps reports to this dashboard

## Usage Notes
1. **Dashboard Composition**:
   - The actual content of a dashboard is defined in related tables
   - Reports are added to dashboards through the `DashboardDefinitionReportDefinitions` table
   - Widget layout and configuration are stored separately

2. **Access Control**:
   - Dashboard visibility is controlled through role assignments
   - Users must have at least one role that's been granted access to view a dashboard

3. **Multi-language Support**:
   - The system will display the appropriate title based on the user's language preference
   - If a translation is not available, it will fall back to the default title

4. **Menu Integration**:
   - Dashboards can be organized into sections in the main menu
   - The sort key determines the order within each section
   - Icons can be specified using standard icon library names

## Constraints
- **PK_DashboardDefinitions**: Primary key on `Id`
- **UQ_DashboardDefinitions__Code**: Ensures dashboard codes are unique
- **FK_DashboardDefinitions__CreatedById**: References `Users(Id)`
- **FK_DashboardDefinitions__ModifiedById**: References `Users(Id)`

## Example Use Cases
1. **Sales Dashboard**:
   - Code: 'sales-performance'
   - Title: 'Sales Performance', 'أداء المبيعات', 'Performances des ventes'
   - Auto-refresh: 10 minutes
   - Menu: Show in 'Analytics' section with chart icon

2. **Inventory Status**:
   - Code: 'inventory-status'
   - Title: 'Inventory Levels', 'مستويات المخزون', 'Niveaux de stock'
   - Auto-refresh: 15 minutes
   - Menu: Show in 'Operations' section with box icon
