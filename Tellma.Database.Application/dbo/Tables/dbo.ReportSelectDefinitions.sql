CREATE TABLE [dbo].[ReportSelectDefinitions]
(
	[Id]						INT						 CONSTRAINT [PK_ReportSelectDefinitions] PRIMARY KEY IDENTITY,
	[ReportDefinitionId]		NVARCHAR (50)	NOT NULL CONSTRAINT [FK_ReportSelectDefinitions__DocumentId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Path]						NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
)
