# DashboardDefinitionWidgets Table

## Overview
The `DashboardDefinitionWidgets` table defines the widgets (reports) that appear on a dashboard and their layout properties. Each widget represents a report instance displayed on a dashboard with specific positioning and display settings.

## Table Structure
```sql
CREATE TABLE [dbo].[DashboardDefinitionWidgets]
(
    [Id]                            INT             IDENTITY PRIMARY KEY,
    [DashboardDefinitionId]         INT             NOT NULL,
    [ReportDefinitionId]            INT             NOT NULL,
    [OffsetX]                       INT             NOT NULL DEFAULT 0,
    [OffsetY]                       INT             NOT NULL DEFAULT 0,
    [Width]                         INT             NOT NULL DEFAULT 1,
    [Height]                        INT             NOT NULL DEFAULT 1,
    [Title]                         NVARCHAR(50),
    [Title2]                        NVARCHAR(50),
    [Title3]                        NVARCHAR(50),
    [AutoRefreshPeriodInMinutes]    INT             NULL,
    [Index]                         INT             NOT NULL
);
```

## Key Fields

### Identification & Relationships
- **Id**: Unique identifier for the widget
- **DashboardDefinitionId**: References the parent dashboard in `DashboardDefinitions`
- **ReportDefinitionId**: References the report from `ReportDefinitions` that this widget displays

### Layout & Display
- **OffsetX**: Horizontal position of the widget on the dashboard grid (0-based)
- **OffsetY**: Vertical position of the widget on the dashboard grid (0-based)
- **Width**: Width of the widget in grid units (minimum 1)
- **Height**: Height of the widget in grid units (minimum 1)
- **Index**: Determines the z-order of widgets when they overlap

### Customization
- **Title/Title2/Title3**: Custom title for the widget in different languages (overrides the report's default title if specified)
- **AutoRefreshPeriodInMinutes**: Custom refresh rate for this widget (overrides the dashboard's default if specified)

## Relationships
- **DashboardDefinition**: Many-to-one relationship with `DashboardDefinitions` table (on `DashboardDefinitionId`)
  - CASCADE DELETE: When a dashboard is deleted, all its widgets are automatically deleted
- **ReportDefinition**: Many-to-one relationship with `ReportDefinitions` table (on `ReportDefinitionId`)

## Usage Notes
1. **Grid Layout**:
   - The dashboard uses a grid system for widget placement
   - Widgets can be positioned anywhere within the grid using OffsetX and OffsetY
   - Width and Height determine how many grid cells the widget occupies

2. **Widget Customization**:
   - Each widget can have its own title, independent of the report's default title
   - The refresh rate can be set per-widget, allowing different update frequencies for different data

3. **Multi-language Support**:
   - Widget titles support up to three languages through Title/Title2/Title3
   - If a title is not provided, the system falls back to the report's default title

## Constraints
- **PK_DashboardDefinitionWidgets**: Primary key on `Id`
- **FK_DashboardDefinitionWidgets_DashboardDefinitionId**: Foreign key to `DashboardDefinitions(Id)` with CASCADE DELETE
- **FK_DashboardDefinitionWidgets_ReportDefinitionId**: Foreign key to `ReportDefinitions(Id)`
- **CHK_DashboardDefinitionWidgets_Width**: Ensures Width ≥ 1
- **CHK_DashboardDefinitionWidgets_Height**: Ensures Height ≥ 1

## Example Use Cases
1. **Sales Dashboard**:
   - A 2x2 widget showing monthly sales trends
   - A 1x1 KPI widget showing YTD sales
   - A 1x2 widget showing top customers

2. **Inventory Dashboard**:
   - A 3x2 widget showing inventory levels by category
   - A 2x1 widget showing low stock alerts
   - A 1x1 widget showing total inventory value

## Best Practices
- Keep widget sizes reasonable to maintain good performance
- Use meaningful custom titles when multiple instances of the same report appear on a dashboard
- Consider the dashboard's overall layout to ensure widgets don't overlap unintentionally
- Set appropriate refresh rates based on how frequently the underlying data changes
