CREATE TABLE [dbo].[AgentDefinitionReportDefinitions]
(
	[Id]					INT				CONSTRAINT [PK_AgentDefinitionReportDefinitions] PRIMARY KEY IDENTITY,
	[AgentDefinitionId]		INT				NOT NULL CONSTRAINT[FK_AgentDefinitionReportDefinition_AgentDefinitionId] REFERENCES dbo.[AgentDefinitions]([Id]) ON DELETE CASCADE,
	[ReportDefinitionId]	INT				NOT NULL CONSTRAINT [FK_AgentDefinitionReportDefinition_ReportDefinitionId] REFERENCES dbo.[ReportDefinitions]([Id]),
	UNIQUE ([AgentDefinitionId], [ReportDefinitionId]),
	[Name]					NVARCHAR (255),
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Index]					INT				NOT NULL,
	[SavedById]				INT				NOT NULL CONSTRAINT [FK_AgentDefinitionReportDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[RelationDefinitionReportDefinitionsHistory]));
GO;