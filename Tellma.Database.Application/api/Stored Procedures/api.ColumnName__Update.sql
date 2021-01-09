CREATE PROCEDURE [api].[ColumnName__Update]
	@param1 int = 0,
	@param2 int
AS
UPDATE [LineDefinitionColumns] SET ColumnName = N'InternalReference' WHERE ColumnName = N'AdditionalReference'
UPDATE [ReportDefinitions] SET [Filter] =  REPLACE([Filter],N'AdditionalReference', N'InternalReference') WHERE [Filter] LIKE N'%AdditionalReference%'
UPDATE [ReportParameterDefinitions] SET [KEY] = N'InternalReference' WHERE [KEY] = N'AdditionalReference'
UPDATE [ReportSelectDefinitions] SET [Path] = N'InternalReference' WHERE [Path] = N'AdditionalReference'
UPDATE [UserSettings] SET [VALUE] = REPLACE([VALUE],N'AdditionalReference',N'InternalReference') where [Key] = N'Document/3/select' and [Value] LIKE N'%AdditionalReference%'
UPDATE LineDefinitions SET [GenerateScript] =  REPLACE([GenerateScript],N'AdditionalReference', N'InternalReference') WHERE [GenerateScript] LIKE N'%AdditionalReference%'
UPDATE LineDefinitions SET [PreprocessScript] =  REPLACE([PreprocessScript],N'AdditionalReference', N'InternalReference') WHERE [PreprocessScript] LIKE N'%AdditionalReference%'
UPDATE LineDefinitions SET [ValidateScript] =  REPLACE([ValidateScript],N'AdditionalReference', N'InternalReference') WHERE [ValidateScript] LIKE N'%AdditionalReference%'
UPDATE MarkupTemplates SET [Body] =  REPLACE([Body],N'AdditionalReference', N'InternalReference') WHERE [Body] LIKE N'%AdditionalReference%'
UPDATE Users Set UserSettingsVersion = NewId()
	