# AgentDefinitionReportDefinitions

The `AgentDefinitionReportDefinitions` table establishes a many-to-many relationship between agent definitions and report definitions, enabling the display of relevant reports directly within an agent's details page. For example, an employee's details page might show related reports such as "Balances," "Leave History," or "Agreements" below their information. This table supports temporal data with system-versioning to track changes over time.

## Table Structure
```sql
CREATE TABLE [dbo].[AgentDefinitionReportDefinitions] (
    [Id]                    INT             PRIMARY KEY IDENTITY,
    [AgentDefinitionId]     INT             NOT NULL,
    [ReportDefinitionId]    INT             NOT NULL,
    [Name]                  NVARCHAR(255)   NULL,
    [Name2]                 NVARCHAR(255)   NULL,
    [Name3]                 NVARCHAR(255)   NULL,
    [Index]                 INT             NOT NULL,
    [SavedById]             INT             NOT NULL,
    [ValidFrom]             DATETIME2       GENERATED ALWAYS AS ROW START NOT NULL,
    [ValidTo]               DATETIME2       GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [UQ_AgentDefinitionReportDefinitions__AgentDefinitionId_ReportDefinitionId] 
        UNIQUE ([AgentDefinitionId], [ReportDefinitionId])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AgentDefinitionReportDefinitionsHistory]));
```

## Key Fields
- **Id**: Unique identifier for each record
- **AgentDefinitionId**: References the agent definition (foreign key to `AgentDefinitions` table)
- **ReportDefinitionId**: References the report definition (foreign key to `ReportDefinitions` table)
- **Name**: Primary name for the report association (localizable)
- **Name2**: Secondary name for the report association (localizable)
- **Name3**: Tertiary name for the report association (localizable)
- **Index**: Order in which reports should be displayed for the agent definition
- **SavedById**: User who last saved the record (references `Users` table)
- **ValidFrom/ValidTo**: System-versioning timestamps for temporal data tracking

## Relationships
- **AgentDefinition**: Many-to-one relationship with `AgentDefinitions` table (on `AgentDefinitionId`)
- **ReportDefinition**: Many-to-one relationship with `ReportDefinitions` table (on `ReportDefinitionId`)
- **SavedBy**: Many-to-one relationship with `Users` table (on `SavedById`)

## Constraints
- **PK_AgentDefinitionReportDefinitions**: Primary key on `Id`
- **FK_AgentDefinitionReportDefinition_AgentDefinitionId**: Foreign key to `AgentDefinitions` table with CASCADE DELETE
- **FK_AgentDefinitionReportDefinition_ReportDefinitionId**: Foreign key to `ReportDefinitions` table
- **FK_AgentDefinitionReportDefinitions__SavedById**: Foreign key to `Users` table
- **UQ_AgentDefinitionReportDefinitions__AgentDefinitionId_ReportDefinitionId**: Ensures each agent definition can only be associated with a specific report definition once

## System Versioning
- The table uses SQL Server's system-versioning (temporal tables) to maintain history
- Historical data is stored in the `AgentDefinitionReportDefinitionsHistory` table
- The `ValidFrom` and `ValidTo` columns are managed automatically by SQL Server

## Usage Notes
- When an agent definition is deleted, all its associated report definitions are automatically removed (CASCADE DELETE)
- The combination of `AgentDefinitionId` and `ReportDefinitionId` must be unique
- The `Index` field controls the display order of reports within an agent's details page
- The `Name`, `Name2`, and `Name3` fields allow for localized report display names (e.g., "Leave History" in different languages)
- Reports are typically displayed as clickable links or tabs within the agent's details interface
- The system uses this association to dynamically generate the list of available reports for each agent type
