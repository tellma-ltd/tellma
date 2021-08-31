CREATE TABLE [dbo].[ResourceDefinitionReportDefinitions]
(
	[Id]					INT				CONSTRAINT [PK_ResourceDefinitionReportDefinitions] PRIMARY KEY IDENTITY,
	[ResourceDefinitionId]	INT				NOT NULL CONSTRAINT [FK_ResourceDefinitionReportDefinition_ResourceDefinitionId] REFERENCES dbo.[ResourceDefinitions]([Id]) ON DELETE CASCADE,
	[ReportDefinitionId]	INT				NOT NULL CONSTRAINT [FK_ResourceDefinitionReportDefinition_ReportDefinitionId] REFERENCES dbo.[ReportDefinitions]([Id]),
	CONSTRAINT [UQ_ResourceDefinitionReportDefinitions__ResourceDefinitionId_ReportDefinitionId] UNIQUE ([ResourceDefinitionId], [ReportDefinitionId]),
	[Name]					NVARCHAR (255),
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Index]					INT				NOT NULL,
	[SavedById]				INT				NOT NULL CONSTRAINT [FK_ResourceDefinitionReportDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[ResourceDefinitionReportDefinitionsHistory]));
GO;