CREATE TABLE [dbo].[CustodyDefinitionReportDefinitions]
(
	[Id]					INT				CONSTRAINT [PK_CustodyDefinitionReportDefinitions] PRIMARY KEY IDENTITY,
	[CustodyDefinitionId]	INT				NOT NULL CONSTRAINT [FK_CustodyDefinitionReportDefinition_CustodyDefinitionId] REFERENCES dbo.[CustodyDefinitions]([Id]) ON DELETE CASCADE,
	[ReportDefinitionId]	INT				NOT NULL CONSTRAINT [FK_CustodyDefinitionReportDefinition_ReportDefinitionId] REFERENCES dbo.[ReportDefinitions]([Id]),
	UNIQUE ([CustodyDefinitionId], [ReportDefinitionId]),
	[Name]					NVARCHAR (255),
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Index]					INT				NOT NULL,
	[SavedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_CustodyDefinitionReportDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[CustodyDefinitionReportDefinitionsHistory]));
GO;