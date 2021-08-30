DROP PROCEDURE IF EXISTS [dbo].[mgr_Check__Drop]
GO
CREATE PROCEDURE [dbo].[mgr_Check__Drop]
	@schema_name nvarchar(256) = N'dbo',
	@table_name nvarchar(256),
	@col_name nvarchar(256)
AS
	DECLARE @Command  nvarchar(1000);

	SELECT @Command = 'ALTER TABLE ' + @schema_name + '.[' + @table_name + '] DROP CONSTRAINT ' + d.name
	FROM sys.tables t
	JOIN sys.default_constraints d on d.parent_object_id = t.object_id
	JOIN sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
	WHERE t.name = @table_name
	AND t.schema_id = schema_id(@schema_name)
	AND c.name = @col_name;
	--PRINT @Command
	EXECUTE (@Command);
GO
DROP PROCEDURE IF EXISTS [dbo].mgr_Default__Drop
GO
CREATE PROCEDURE dbo.mgr_Default__Drop
	@schema_name nvarchar(256) = N'dbo',
	@table_name nvarchar(256),
	@col_name nvarchar(256)
AS
	DECLARE @Command  nvarchar(1000);

	SELECT @Command = 'ALTER TABLE ' + @schema_name + '.[' + @table_name + '] DROP CONSTRAINT ' + d.name
	FROM sys.tables t
	JOIN sys.default_constraints d on d.parent_object_id = t.object_id
	JOIN sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
	WHERE t.name = @table_name
	AND t.schema_id = schema_id(@schema_name)
	AND c.name = @col_name;
	--PRINT @Command
	EXECUTE (@Command);
GO

ALTER TABLE [dbo].[AccountTypeRelationDefinitions] SET (SYSTEM_VERSIONING = OFF);
GO
EXECUTE sp_rename
@objname = N'[dbo].[AccountTypeRelationDefinitions].[RelationDefinitionId]',
@newname = N'AgentDefinitionId', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[AccountTypeRelationDefinitionsHistory].[RelationDefinitionId]',
@newname = N'AgentDefinitionId', @objtype = N'COLUMN';
GO
ALTER TABLE [dbo].[AccountTypeRelationDefinitions]
SET(SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeRelationDefinitionsHistory], DATA_CONSISTENCY_CHECK = ON));
GO
ALTER TABLE [dbo].[AccountTypeNotedRelationDefinitions] SET (SYSTEM_VERSIONING = OFF);
GO
EXECUTE sp_rename
@objname = N'[dbo].[AccountTypeNotedRelationDefinitions].[NotedRelationDefinitionId]',
@newname = N'NotedAgentDefinitionId', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[AccountTypeNotedRelationDefinitionsHistory].[NotedRelationDefinitionId]',
@newname = N'NotedAgentDefinitionId', @objtype = N'COLUMN';
GO
ALTER TABLE [dbo].[AccountTypeNotedRelationDefinitions]
SET(SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeNotedRelationDefinitionsHistory], DATA_CONSISTENCY_CHECK = ON));
GO
ALTER TABLE [dbo].[LineDefinitionEntryRelationDefinitions] SET (SYSTEM_VERSIONING = OFF);
GO
EXECUTE sp_rename
@objname = N'[dbo].[LineDefinitionEntryRelationDefinitions].[RelationDefinitionId]',
@newname = N'AgentDefinitionId', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[LineDefinitionEntryRelationDefinitionsHistory].[RelationDefinitionId]',
@newname = N'AgentDefinitionId', @objtype = N'COLUMN';
GO
ALTER TABLE [dbo].[LineDefinitionEntryRelationDefinitions]
SET(SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntryRelationDefinitionsHistory], DATA_CONSISTENCY_CHECK = ON));
GO
ALTER TABLE [dbo].[LineDefinitionEntryNotedRelationDefinitions] SET (SYSTEM_VERSIONING = OFF);
GO
EXECUTE sp_rename
@objname = N'[dbo].[LineDefinitionEntryNotedRelationDefinitions].[NotedRelationDefinitionId]',
@newname = N'NotedAgentDefinitionId', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[LineDefinitionEntryNotedRelationDefinitionsHistory].[NotedRelationDefinitionId]',
@newname = N'NotedAgentDefinitionId', @objtype = N'COLUMN';
GO
ALTER TABLE [dbo].[LineDefinitionEntryNotedRelationDefinitions]
SET(SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntryNotedRelationDefinitionsHistory], DATA_CONSISTENCY_CHECK = ON));
GO
ALTER TABLE [dbo].[RelationDefinitionReportDefinitions] SET (SYSTEM_VERSIONING = OFF);
GO
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitionReportDefinitions].[RelationDefinitionId]',
@newname = N'AgentDefinitionId', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitionReportDefinitionsHistory].[RelationDefinitionId]',
@newname = N'AgentDefinitionId', @objtype = N'COLUMN';
GO
ALTER TABLE [dbo].[RelationDefinitionReportDefinitions]
SET(SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[RelationDefinitionReportDefinitionsHistory], DATA_CONSISTENCY_CHECK = ON));
GO
ALTER TABLE [dbo].[RelationDefinitions] SET (SYSTEM_VERSIONING = OFF);
GO
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitions].[Relation1DefinitionId]',
@newname = N'Agent1DefinitionId', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitions].[Relation1Label]',
@newname = N'Agent1Label', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitions].[Relation1Label2]',
@newname = N'Agent1Label2', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitions].[Relation1Label3]',
@newname = N'Agent1Label3', @objtype = N'COLUMN';
EXEC dbo.mgr_Check__Drop @table_name = N'RelationDefinitions', @col_name = N'Relation1Visibility';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitions].[Relation1Visibility]',
@newname = N'Agent1Visibility', @objtype = N'COLUMN';
GO
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitionsHistory].[Relation1DefinitionId]',
@newname = N'Agent1DefinitionId', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitionsHistory].[Relation1Label]',
@newname = N'Agent1Label', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitionsHistory].[Relation1Label2]',
@newname = N'Agent1Label2', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitionsHistory].[Relation1Label3]',
@newname = N'Agent1Label3', @objtype = N'COLUMN';
EXECUTE sp_rename
@objname = N'[dbo].[RelationDefinitionsHistory].[Relation1Visibility]',
@newname = N'Agent1Visibility', @objtype = N'COLUMN';
GO
ALTER TABLE [dbo].[RelationDefinitions]
SET(SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[RelationDefinitionsHistory], DATA_CONSISTENCY_CHECK = ON));
GO
EXECUTE sp_rename @objname = N'[dbo].[AccountBalances].[RelationId]',@newname = N'AgentId', @objtype = N'COLUMN';
GO
ALTER TABLE dbo.Entries DROP CONSTRAINT IF EXISTS [FK_Entries__WarehouseId];
ALTER TABLE dbo.Entries DROP COLUMN IF EXISTS [WarehouseId];
DROP TABLE IF EXISTS ResourcePacks;
DROP TABLE IF EXISTS ResourceProviders;
DROP TABLE IF EXISTS dbo.[Views];
DROP TABLE IF EXISTS [dbo].[VoucherBooklets];
DROP TABLE IF EXISTS [dbo].[VoucherTypes];
DROP PROCEDURE bll.Lines_Validate__State_Data2;
DROP FUNCTION IF EXISTS bll.fn_RS__RL;
DROP FUNCTION IF EXISTS bll.fn_RSD__RLD;
DROP FUNCTION IF EXISTS dbo.fi_Relations;
DROP VIEW IF EXISTS dbo.Employees;
GO
ALTER TABLE dbo.WorkflowSignatures DROP CONSTRAINT IF EXISTS FK_WorkflowSignatures__PredicateType;
DROP TABLE IF EXISTS  dbo.PredicateTypes;
GO
ALTER TABLE dbo.LineSignatures DROP CONSTRAINT IF EXISTS CK_LineSignatures__RuleType;
DROP TABLE IF EXISTS dbo.RuleTypes
GO
UPDATE [LineDefinitionColumns] SET [ColumnName] = REPLACE([ColumnName] COLLATE SQL_Latin1_General_CP1_CS_AS, N'Relation', N'Agent')
UPDATE [LineDefinitionColumns] SET [Filter] = REPLACE([Filter], N'Relation', N'Agent')
UPDATE [LineDefinitionGenerateParameters] SET [Control] = REPLACE([Control], N'Relation', N'Agent')
UPDATE [LineDefinitionGenerateParameters] SET [ControlOptions] = REPLACE([ControlOptions], N'Relation', N'Agent')
UPDATE [LineDefinitions] SET [GenerateScript] = REPLACE([GenerateScript], N'Relation', N'Agent')
UPDATE [LineDefinitions] SET [PreprocessScript] = REPLACE([PreprocessScript], N'Relation', N'Agent')
UPDATE [LineDefinitions] SET [ValidateScript] = REPLACE([ValidateScript], N'Relation', N'Agent')
UPDATE [MarkupTemplates] SET [Collection] = REPLACE([Collection], N'Relation', N'Agent')
UPDATE [MarkupTemplates] SET [DownloadName] = REPLACE([DownloadName], N'Relation', N'Agent')
UPDATE [MarkupTemplates] SET [Body] = REPLACE([Body], N'Relation', N'Agent')
UPDATE [Permissions] SET [View] = REPLACE([View], N'Relation', N'Agent')
UPDATE [Permissions] SET [Criteria] = REPLACE([Criteria], N'Relation', N'Agent')
UPDATE [RelationDefinitions] SET [PreprocessScript] = REPLACE([PreprocessScript], N'Relation', N'Agent')
UPDATE [RelationDefinitions] SET [ValidateScript] = REPLACE([ValidateScript], N'Relation', N'Agent')
UPDATE [ReportDefinitionDimensionAttributes] SET [Expression] = REPLACE([Expression], N'Relation', N'Agent')
UPDATE [ReportDefinitionDimensions] SET [KeyExpression] = REPLACE([KeyExpression], N'Relation', N'Agent')
UPDATE [ReportDefinitionDimensions] SET [DisplayExpression] = REPLACE([DisplayExpression], N'Relation', N'Agent')
UPDATE [ReportDefinitionDimensions] SET [Control] = REPLACE([Control], N'Relation', N'Agent')
UPDATE [ReportDefinitionDimensions] SET [ControlOptions] = REPLACE([ControlOptions], N'Relation', N'Agent')
UPDATE [ReportDefinitionMeasures] SET [Expression] = REPLACE([Expression], N'Relation', N'Agent')
UPDATE [ReportDefinitionMeasures] SET [Control] = REPLACE([Control], N'Relation', N'Agent')
UPDATE [ReportDefinitionMeasures] SET [ControlOptions] = REPLACE([ControlOptions], N'Relation', N'Agent')
UPDATE [ReportDefinitionMeasures] SET [DangerWhen] = REPLACE([DangerWhen], N'Relation', N'Agent')
UPDATE [ReportDefinitionMeasures] SET [WarningWhen] = REPLACE([WarningWhen], N'Relation', N'Agent')
UPDATE [ReportDefinitionMeasures] SET [SuccessWhen] = REPLACE([SuccessWhen], N'Relation', N'Agent')
UPDATE [ReportDefinitionParameters] SET [DefaultExpression] = REPLACE([DefaultExpression], N'Relation', N'Agent')
UPDATE [ReportDefinitionParameters] SET [Control] = REPLACE([Control], N'Relation', N'Agent')
UPDATE [ReportDefinitionParameters] SET [ControlOptions] = REPLACE([ControlOptions], N'Relation', N'Agent')
UPDATE [ReportDefinitions] SET [Collection] = REPLACE([Collection], N'Relation', N'Agent')
UPDATE [ReportDefinitions] SET [Filter] = REPLACE([Filter], N'Relation', N'Agent')
UPDATE [ReportDefinitions] SET [Having] = REPLACE([Having], N'Relation', N'Agent')
UPDATE [ReportDefinitions] SET [OrderBy] = REPLACE([OrderBy], N'Relation', N'Agent')
UPDATE [ReportDefinitionSelects] SET [Expression] = REPLACE([Expression], N'Relation', N'Agent')
UPDATE [ReportDefinitionSelects] SET [Control] = REPLACE([Control], N'Relation', N'Agent')
UPDATE [ReportDefinitionSelects] SET [ControlOptions] = REPLACE([ControlOptions], N'Relation', N'Agent')
UPDATE [ResourceDefinitions] SET [PreprocessScript] = REPLACE([PreprocessScript], N'Relation', N'Agent')
UPDATE [ResourceDefinitions] SET [ValidateScript] = REPLACE([ValidateScript], N'Relation', N'Agent')
UPDATE [UserSettings] SET [Key] = REPLACE([Key], N'Relation', N'Agent')
UPDATE [UserSettings] SET [Value] = REPLACE([Value], N'Relation', N'Agent')
UPDATE [LineDefinitionColumns] SET [ColumnName] = REPLACE([ColumnName] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [LineDefinitionColumns] SET [Filter] = REPLACE([Filter] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [LineDefinitionGenerateParameters] SET [Control] = REPLACE([Control] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [LineDefinitionGenerateParameters] SET [ControlOptions] = REPLACE([ControlOptions] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [LineDefinitions] SET [GenerateScript] = REPLACE([GenerateScript] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [LineDefinitions] SET [PreprocessScript] = REPLACE([PreprocessScript] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [LineDefinitions] SET [ValidateScript] = REPLACE([ValidateScript] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [MarkupTemplates] SET [Collection] = REPLACE([Collection] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [MarkupTemplates] SET [DownloadName] = REPLACE([DownloadName] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [MarkupTemplates] SET [Body] = REPLACE([Body] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [Permissions] SET [View] = REPLACE([View] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [Permissions] SET [Criteria] = REPLACE([Criteria] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [RelationDefinitions] SET [PreprocessScript] = REPLACE([PreprocessScript] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [RelationDefinitions] SET [ValidateScript] = REPLACE([ValidateScript] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionDimensionAttributes] SET [Expression] = REPLACE([Expression] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionDimensions] SET [KeyExpression] = REPLACE([KeyExpression] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionDimensions] SET [DisplayExpression] = REPLACE([DisplayExpression] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionDimensions] SET [Control] = REPLACE([Control] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionDimensions] SET [ControlOptions] = REPLACE([ControlOptions] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionMeasures] SET [Expression] = REPLACE([Expression] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionMeasures] SET [Control] = REPLACE([Control] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionMeasures] SET [ControlOptions] = REPLACE([ControlOptions] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionMeasures] SET [DangerWhen] = REPLACE([DangerWhen] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionMeasures] SET [WarningWhen] = REPLACE([WarningWhen] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionMeasures] SET [SuccessWhen] = REPLACE([SuccessWhen] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionParameters] SET [DefaultExpression] = REPLACE([DefaultExpression] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionParameters] SET [Control] = REPLACE([Control] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionParameters] SET [ControlOptions] = REPLACE([ControlOptions] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitions] SET [Collection] = REPLACE([Collection] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitions] SET [Filter] = REPLACE([Filter] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitions] SET [Having] = REPLACE([Having] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitions] SET [OrderBy] = REPLACE([OrderBy] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionSelects] SET [Expression] = REPLACE([Expression] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionSelects] SET [Control] = REPLACE([Control] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ReportDefinitionSelects] SET [ControlOptions] = REPLACE([ControlOptions] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ResourceDefinitions] SET [PreprocessScript] = REPLACE([PreprocessScript] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [ResourceDefinitions] SET [ValidateScript] = REPLACE([ValidateScript] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [UserSettings] SET [Key] = REPLACE([Key] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent')
UPDATE [UserSettings] SET [Value] = REPLACE([Value] COLLATE SQL_Latin1_General_CP1_CS_AS, N'relation', N'agent');
GO