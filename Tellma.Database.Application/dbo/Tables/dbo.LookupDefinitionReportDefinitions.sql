CREATE TABLE [dbo].[LookupDefinitionReportDefinitions]
(
	[Id]					INT				CONSTRAINT [PK_LookupDefinitionReportDefinitions] PRIMARY KEY IDENTITY,
	[LookupDefinitionId]	INT				NOT NULL CONSTRAINT [FK_LookupDefinitionReportDefinition_LookupDefinitionId] REFERENCES dbo.[LookupDefinitions]([Id]) ON DELETE CASCADE,
	[ReportDefinitionId]	INT				NOT NULL CONSTRAINT [FK_LookupDefinitionReportDefinition_ReportDefinitionId] REFERENCES dbo.[ReportDefinitions]([Id]),
	UNIQUE ([LookupDefinitionId], [ReportDefinitionId]),
	[Name]					NVARCHAR (255),
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Index]					INT				NOT NULL,
	[SavedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LookupDefinitionReportDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LookupDefinitionReportDefinitionsHistory]));
GO;