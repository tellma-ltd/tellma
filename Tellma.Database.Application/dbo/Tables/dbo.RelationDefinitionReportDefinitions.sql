CREATE TABLE [dbo].[RelationDefinitionReportDefinitions]
(
	[Id]					INT				CONSTRAINT [PK_RelationDefinitionReportDefinitions] PRIMARY KEY IDENTITY,
	[RelationDefinitionId]	INT				NOT NULL CONSTRAINT [FK_RelationDefinitionReportDefinition_RelationDefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]) ON DELETE CASCADE,
	[ReportDefinitionId]	INT				NOT NULL CONSTRAINT [FK_RelationDefinitionReportDefinition_ReportDefinitionId] REFERENCES dbo.[ReportDefinitions]([Id]),
	UNIQUE ([RelationDefinitionId], [ReportDefinitionId]),
	[Name]					NVARCHAR (255),
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Index]					INT				NOT NULL,
	[SavedById]				INT				NOT NULL CONSTRAINT [FK_RelationDefinitionReportDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[RelationDefinitionReportDefinitionsHistory]));
GO;